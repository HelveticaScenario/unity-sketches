using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class Delaunator
{
    public delegate float GetF<T>(T e);
    public static Delaunator from<T>(List<T> points, GetF<T> getX, GetF<T> getY)
    {
        var n = points.Count;
        var coords = new float[n * 2];

        for (var i = 0; i < n; i++)
        {
            var p = points[i];
            coords[2 * i] = getX(p);
            coords[2 * i + 1] = getY(p);
        }

        return new Delaunator(coords);
    }
    uint[] EDGE_STACK = new uint[512];

    public float[] coords { get; private set; }
    public uint[] triangles { get; private set; }
    public int[] halfedges { get; private set; }
    public uint[] hullPrev { get; private set; }
    public uint[] hullNext { get; private set; }
    public uint[] hullTri { get; private set; }
    int _hashSize;
    public uint hullStart { get; private set; }
    public uint[] hull { get; private set; }
    public uint trianglesLen { get; private set; }
    float _cx;
    float _cy;

    public Delaunator(float[] coords)
    {
        var num = coords.Length >> 1;

        this.coords = coords;

        // arrays that will store the triangulation graph
        var maxTriangles = 2 * num - 5;
        var triangles = this.triangles = new uint[maxTriangles * 3];
        var halfedges = this.halfedges = new int[maxTriangles * 3];

        // temporary arrays for tracking the edges of the advancing convex hull
        this._hashSize = Mathf.CeilToInt(Mathf.Sqrt(num));
        var hullPrev = this.hullPrev = new uint[num]; // edge to prev edge
        var hullNext = this.hullNext = new uint[num]; // edge to next edge
        var hullTri = this.hullTri = new uint[num]; // edge to adjacent triangle
        var hullHash = Enumerable.Repeat(-1, this._hashSize).ToArray();//new int[this._hashSize];//.fill(-1); // angular edge hash


        // populate an array of point indices; calculate input data bbox
        var ids = new uint[num];
        var minX = float.PositiveInfinity;
        var minY = float.PositiveInfinity;
        var maxX = float.NegativeInfinity;
        var maxY = float.NegativeInfinity;

        for (uint i = 0; i < num; i++)
        {
            var x = coords[2 * i];
            var y = coords[2 * i + 1];
            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
            ids[i] = i;
        }
        var cx = (minX + maxX) / 2;
        var cy = (minY + maxY) / 2;

        var minDist = float.PositiveInfinity;
        uint i0 = 0;
        uint i1 = 0;
        uint i2 = 0;

        // pick a seed point close to the center
        for (uint i = 0; i < num; i++)
        {
            var d = dist(cx, cy, coords[2 * i], coords[2 * i + 1]);
            if (d < minDist)
            {
                i0 = i;
                minDist = d;
            }
        }
        var i0x = coords[2 * i0];
        var i0y = coords[2 * i0 + 1];

        minDist = float.PositiveInfinity;

        // find the point closest to the seed
        for (uint i = 0; i < num; i++)
        {
            if (i == i0) continue;
            var d = dist(i0x, i0y, coords[2 * i], coords[2 * i + 1]);
            if (d < minDist && d > 0)
            {
                i1 = i;
                minDist = d;
            }
        }
        var i1x = coords[2 * i1];
        var i1y = coords[2 * i1 + 1];

        var minRadius = float.PositiveInfinity;

        // find the third point which forms the smallest circumcircle with the first two
        for (uint i = 0; i < num; i++)
        {
            if (i == i0 || i == i1) continue;
            var r = circumradius(i0x, i0y, i1x, i1y, coords[2 * i], coords[2 * i + 1]);
            if (r < minRadius)
            {
                i2 = i;
                minRadius = r;
            }
        }
        var i2x = coords[2 * i2];
        var i2y = coords[2 * i2 + 1];

        if (minRadius == float.PositiveInfinity)
        {
            throw new System.Exception("No Delaunay triangulation exists for this input.");
        }

        // swap the order of the seed points for counter-clockwise orientation
        if (orient(i0x, i0y, i1x, i1y, i2x, i2y))
        {
            var i = i1;
            var x = i1x;
            var y = i1y;
            i1 = i2;
            i1x = i2x;
            i1y = i2y;
            i2 = i;
            i2x = x;
            i2y = y;
        }

        var center = circumcenter(i0x, i0y, i1x, i1y, i2x, i2y);
        this._cx = center.x;
        this._cy = center.y;

        var dists = new float[num];
        for (var i = 0; i < num; i++)
        {
            dists[i] = dist(coords[2 * i], coords[2 * i + 1], center.x, center.y);
        }

        // sort the points by distance from the seed triangle circumcenter
        quicksort(ids, dists, 0, num - 1);

        // set up the seed triangle as the starting hull
        this.hullStart = i0;
        var hullSize = 3;

        hullNext[i0] = hullPrev[i2] = i1;
        hullNext[i1] = hullPrev[i0] = i2;
        hullNext[i2] = hullPrev[i1] = i0;

        hullTri[i0] = 0;
        hullTri[i1] = 1;
        hullTri[i2] = 2;

        hullHash[this._hashKey(i0x, i0y)] = (int)i0;
        hullHash[this._hashKey(i1x, i1y)] = (int)i1;
        hullHash[this._hashKey(i2x, i2y)] = (int)i2;

        this.trianglesLen = 0;
        this._addTriangle(i0, i1, i2, -1, -1, -1);
        float xp = 0.0f, yp = 0.0f;
        for (int k = 0; k < ids.Length; k++)
        {
            var i = ids[k];
            var x = coords[2 * i];
            var y = coords[2 * i + 1];

            // skip near-duplicate points
            if (k > 0 && Mathf.Abs(x - xp) <= float.Epsilon && Mathf.Abs(y - yp) <= float.Epsilon) continue;
            xp = x;
            yp = y;

            // skip seed triangle points
            if (i == i0 || i == i1 || i == i2) continue;

            // find a visible edge on the convex hull using edge hash
            int start = 0;
            var key = _hashKey(x, y);
            for (var j = 0; j < this._hashSize; j++)
            {
                start = hullHash[(key + j) % this._hashSize];
                if (start != -1 && start != hullNext[start]) break;
            }

            start = (int)hullPrev[start];
            int e = start;
            uint q = hullNext[e];
            while (!orient(x, y, coords[2 * e], coords[2 * e + 1], coords[2 * q], coords[2 * q + 1]))
            {
                e = (int)q;
                if (e == start)
                {
                    e = -1;
                    break;
                }
                q = hullNext[e];
            }
            if (e == -1) continue; // likely a near-duplicate point; skip it

            // add the first triangle from the point
            var t = _addTriangle((uint)e, i, hullNext[e], -1, -1, (int)hullTri[e]);

            // recursively flip triangles from the point until they satisfy the Delaunay condition
            hullTri[i] = _legalize(t + 2);
            hullTri[e] = t; // keep track of boundary triangles on the hull
            hullSize++;

            // walk forward through the hull, adding more triangles and flipping recursively
            var n = hullNext[e];
            q = hullNext[n];
            while (orient(x, y, coords[2 * n], coords[2 * n + 1], coords[2 * q], coords[2 * q + 1]))
            {
                t = _addTriangle(n, i, q, (int)hullTri[i], -1, (int)hullTri[n]);
                hullTri[i] = _legalize(t + 2);
                hullNext[n] = n; // mark as removed
                hullSize--;
                n = q;
                q = hullNext[n];
            }

            // walk backward from the other side, adding more triangles and flipping
            if (e == start)
            {
                q = hullPrev[e];
                while (orient(x, y, coords[2 * q], coords[2 * q + 1], coords[2 * e], coords[2 * e + 1]))
                {
                    t = _addTriangle(q, i, (uint)e, -1, (int)hullTri[e], (int)hullTri[q]);
                    _legalize(t + 2);
                    hullTri[q] = t;
                    hullNext[e] = (uint)e; // mark as removed
                    hullSize--;
                    e = (int)q;
                    q = hullPrev[e];
                }
            }

            // update the hull indices
            this.hullStart = hullPrev[i] = (uint)e;
            hullNext[e] = hullPrev[n] = i;
            hullNext[i] = n;

            // save the two new edges in the hash table
            hullHash[this._hashKey(x, y)] = (int)i;
            hullHash[this._hashKey(coords[2 * e], coords[2 * e + 1])] = e;
        }

        this.hull = new uint[hullSize];
        var _e = this.hullStart;
        for (var i = 0; i < hullSize; i++)
        {
            this.hull[i] = _e;
            _e = hullNext[_e];
        }
        this.hullPrev = this.hullNext = this.hullTri = null; // get rid of temporary arrays

        // trim typed triangle mesh arrays
        var newTriangles = new uint[this.trianglesLen];
        var newHalfEdges = new int[this.trianglesLen];
        Array.Copy(triangles, newTriangles, this.trianglesLen);
        Array.Copy(halfedges, newHalfEdges, this.trianglesLen);
        this.triangles = newTriangles;
        this.halfedges = newHalfEdges;
    }


    int _hashKey(float x, float y)
    {
        return Mathf.FloorToInt(pseudoAngle(x - this._cx, y - this._cy) * this._hashSize) % this._hashSize;
    }

    uint _legalize(uint a)
    {
        // var {triangles, coords, halfedges} = this;

        uint i = 0;
        uint ar = 0;

        // recursion eliminated with a fixed-size stack
        while (true)
        {
            int _b = halfedges[a];

            /* if the pair of triangles doesn't satisfy the Delaunay condition
             * (p1 is inside the circumcircle of [p0, pl, pr]), flip them,
             * then do the same check/flip recursively for the new pair of triangles
             *
             *           pl                    pl
             *          /||\                  /  \
             *       al/ || \bl            al/    \a
             *        /  ||  \              /      \
             *       /  a||b  \    flip    /___ar___\
             *     p0\   ||   /p1   =>   p0\---bl---/p1
             *        \  ||  /              \      /
             *       ar\ || /br             b\    /br
             *          \||/                  \  /
             *           pr                    pr
             */
            uint a0 = a - a % 3;
            ar = a0 + (a + 2) % 3;

            if (_b == -1)
            { // convex hull edge
                if (i == 0) break;
                a = EDGE_STACK[--i];
                continue;
            }
            uint b = (uint)_b;

            uint b0 = b - b % 3;
            uint al = a0 + (a + 1) % 3;
            uint bl = b0 + (b + 2) % 3;

            uint p0 = triangles[ar];
            uint pr = triangles[a];
            uint pl = triangles[al];
            uint p1 = triangles[bl];

            var illegal = inCircle(
                coords[2 * p0], coords[2 * p0 + 1],
                coords[2 * pr], coords[2 * pr + 1],
                coords[2 * pl], coords[2 * pl + 1],
                coords[2 * p1], coords[2 * p1 + 1]);

            if (illegal)
            {
                triangles[a] = p1;
                triangles[b] = p0;

                var hbl = halfedges[bl];

                // edge swapped on the other side of the hull (rare); fix the halfedge reference
                if (hbl == -1)
                {
                    var e = this.hullStart;
                    do
                    {
                        if (this.hullTri[e] == bl)
                        {
                            this.hullTri[e] = a;
                            break;
                        }
                        e = this.hullNext[e];
                    } while (e != this.hullStart);
                }
                this._link(a, hbl);
                this._link(b, halfedges[ar]);
                this._link(ar, (int)bl);

                var br = b0 + (b + 1) % 3;

                // don't worry about hitting the cap: it can only happen on extremely degenerate input
                if (i < EDGE_STACK.Length)
                {
                    EDGE_STACK[i++] = br;
                }
            }
            else
            {
                if (i == 0) break;
                a = EDGE_STACK[--i];
            }
        }

        return ar;
    }
    void _link(uint a, int b)
    {
        this.halfedges[a] = b;
        if (b != -1) this.halfedges[b] = (int)a;
    }

    // add a new triangle given vertex indices and adjacent half-edge ids
    uint _addTriangle(uint i0, uint i1, uint i2, int a, int b, int c)
    {
        var t = this.trianglesLen;

        this.triangles[t] = i0;
        this.triangles[t + 1] = i1;
        this.triangles[t + 2] = i2;

        this._link(t, a);
        this._link(t + 1, b);
        this._link(t + 2, c);

        this.trianglesLen += 3;

        return t;
    }

    public static float pseudoAngle(float dx, float dy)
    {
        var p = dx / (Mathf.Abs(dx) + Mathf.Abs(dy));
        return (dy > 0 ? 3 - p : 1 + p) / 4; // [0..1]
    }

    public static float dist(float ax, float ay, float bx, float by)
    {
        var dx = ax - bx;
        var dy = ay - by;
        return dx * dx + dy * dy;
    }
    public static float dist(Vector2 a, Vector2 b)
    {
        return dist(a.x, a.y, b.x, b.y);
    }

    public static bool orient(float px, float py, float qx, float qy, float rx, float ry)
    {
        return (qy - py) * (rx - qx) - (qx - px) * (ry - qy) < 0;
    }
    public static bool orient(Vector2 p, Vector2 q, Vector2 r)
    {
        return orient(p.x, p.y, q.x, q.y, r.x, r.y);
    }

    public static bool inCircle(float ax, float ay, float bx, float by, float cx, float cy, float px, float py)
    {
        var dx = ax - px;
        var dy = ay - py;
        var ex = bx - px;
        var ey = by - py;
        var fx = cx - px;
        var fy = cy - py;

        var ap = dx * dx + dy * dy;
        var bp = ex * ex + ey * ey;
        var cp = fx * fx + fy * fy;

        return dx * (ey * cp - bp * fy) -
               dy * (ex * cp - bp * fx) +
               ap * (ex * fy - ey * fx) < 0;
    }
    public static bool inCircle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
    {
        return inCircle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y);
    }
    public static float circumradius(float ax, float ay, float bx, float by, float cx, float cy)
    {
        var dx = bx - ax;
        var dy = by - ay;
        var ex = cx - ax;
        var ey = cy - ay;

        var bl = dx * dx + dy * dy;
        var cl = ex * ex + ey * ey;
        var d = 0.5f / (dx * ey - dy * ex);

        var x = (ey * bl - dy * cl) * d;
        var y = (dx * cl - ex * bl) * d;

        return x * x + y * y;
    }
    public static float circumradius(Vector2 a, Vector2 b, Vector2 c)
    {
        return circumradius(a.x, a.y, b.x, b.y, c.x, c.y);
    }


    public static Vector2 circumcenter(float ax, float ay, float bx, float by, float cx, float cy)
    {
        var dx = bx - ax;
        var dy = by - ay;
        var ex = cx - ax;
        var ey = cy - ay;

        var bl = dx * dx + dy * dy;
        var cl = ex * ex + ey * ey;
        var d = 0.5f / (dx * ey - dy * ex);

        var x = ax + (ey * bl - dy * cl) * d;
        var y = ay + (dx * cl - ex * bl) * d;

        return new Vector2(x, y);
    }
    public static Vector2 circumcenter(Vector2 a, Vector2 b, Vector2 c)
    {
        return circumcenter(a.x, a.y, b.x, b.y, c.x, c.y);
    }

    public static void quicksort(uint[] ids, float[] dists, int left, int right)
    {
        if (right - left <= 20)
        {
            for (var i = left + 1; i <= right; i++)
            {
                var temp = ids[i];
                var tempDist = dists[temp];
                var j = i - 1;
                while (j >= left && dists[ids[j]] > tempDist) ids[j + 1] = ids[j--];
                ids[j + 1] = temp;
            }
        }
        else
        {
            var median = (left + right) >> 1;
            var i = left + 1;
            var j = right;
            swap(ids, median, i);
            if (dists[ids[left]] > dists[ids[right]]) swap(ids, left, right);
            if (dists[ids[i]] > dists[ids[right]]) swap(ids, i, right);
            if (dists[ids[left]] > dists[ids[i]]) swap(ids, left, i);

            var temp = ids[i];
            var tempDist = dists[temp];
            while (true)
            {
                do i++; while (dists[ids[i]] < tempDist);
                do j--; while (dists[ids[j]] > tempDist);
                if (j < i) break;
                swap(ids, i, j);
            }
            ids[left + 1] = ids[j];
            ids[j] = temp;

            if (right - i + 1 >= j - left)
            {
                quicksort(ids, dists, i, right);
                quicksort(ids, dists, left, j - 1);
            }
            else
            {
                quicksort(ids, dists, left, j - 1);
                quicksort(ids, dists, i, right);
            }
        }
    }

    public static void swap<T>(T[] arr, int i, int j)
    {
        var tmp = arr[i];
        arr[i] = arr[j];
        arr[j] = tmp;
    }

}