/*


 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeD : MonoBehaviour
{
    struct Tri
    {
        public Vector3 v1 { get; set; }
        public Vector3 v2 { get; set; }
        public Vector3 v3 { get; set; }
        public Vector3 mean { get; set; }
        public Vector3 norm { get; set; }
    }
    private Pico p;
    [Range(1.0f, 100.0f)]
    public float scale = 10.0f;
    public Mesh[] meshes;
    public uint selectedMesh = 0;
    public Transform t;
    Tri[][] tris;

    // Start is called before the first frame update
    void Start()
    {
        p = this.GetComponent<Pico>();

        p._init();
        updateTris();
    }

    void updateTris()
    {
        tris = new Tri[meshes.Length][];
        for (int m = 0; m < meshes.Length; m++)
        {
            var mesh = meshes[m];
            tris[m] = new Tri[mesh.triangles.Length / 3];
            for (int i = 0; i < mesh.triangles.Length / 3; i++)
            {
                var v1 = (mesh.vertices[mesh.triangles[(i * 3) + 0]]);
                var v2 = (mesh.vertices[mesh.triangles[(i * 3) + 1]]);
                var v3 = (mesh.vertices[mesh.triangles[(i * 3) + 2]]);
                var U = v2 - v1;
                var V = v3 - v1;
                // Cross the vectors to get a perpendicular vector, then normalize it.
                var norm = Vector3.Cross(U, V).normalized;

                var mx = (v1.x + v2.x + v3.x) / 3;
                var my = (v1.y + v2.y + v3.y);
                var mz = (v1.z + v2.z + v3.z) / 3;
                var mean = new Vector3(mx, my, mz);

                tris[m][i] = new Tri
                {
                    v1 = v1,
                    v2 = v2,
                    v3 = v3,
                    mean = mean,
                    norm = norm
                };

            }
        }
    }
    bool clockwise(Vector2 a, Vector2 b, Vector2 c) //tests if a triangle's points are clockwise oriented
    {
        return ((c.x - b.x) * (b.y - a.y)) -
                                    ((b.x - a.x) * (c.y - b.y)) > 0;
    }

    Vector2 R(Vector3 v)
    {
        var w = (float)p.Width / 2.0f;
        var h = (float)p.Height / 2.0f;
        var X = v.x;
        var Y = v.y;
        var Z = v.z;
        return new Vector2(w + X / Z * w, h + Y / Z * w);
    }
    bool r = false;
    // Update is called once per frame
    void Update()
    {

        p.cls(0);
        var sel = (selectedMesh + Mathf.FloorToInt(Time.realtimeSinceStartup)) % tris.Length;
        for (var i = 0; i < tris[sel].Length; i++)
        {
            var tri = tris[sel][i];
            var v1 = R((t.rotation * tri.v1 * scale) + t.position);
            var v2 = R((t.rotation * tri.v2 * scale) + t.position);
            var v3 = R((t.rotation * tri.v3 * scale) + t.position);
            var mean = R((t.rotation * tri.mean * scale) + t.position);
            if (!clockwise(v1, v2, v3))
            {
                p.trifill(v1, v2, v3, (i % 15) + 1);
            }
            // p.line(new Vector2(p.Width / 2, p.Height / 2), R(((tri.norm * scale) + t.position)), (byte)(i % 16));
        }
        // foreach (var vert in mesh.sharedMesh.vertices)
        // {
        //     p.pset(R((vert + new Vector3(0, 0, 2)) * scale), 6);
        // }



        p.flip();
        // r = true;
    }
}
