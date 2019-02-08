using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickPoints : MonoBehaviour
{
    // Start is called before the first frame update
    Pico p;
    struct Point
    {
        public int color { get; set; }
        public Vector2 coords { get; set; }
    }
    List<Point> points;
    Delaunator d;
    float getX(Point e)
    {
        return e.coords.x;
    }
    float getY(Point e)
    {
        return e.coords.y;
    }
    void Start()
    {
        p = GetComponent<Pico>();
        points = new List<Point>(100);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            points = new List<Point>(100);
        }
        p.cls();

        if (Input.GetMouseButtonDown(0) && p.mousePosition.HasValue)
        {
            points.Add(new Point { color = Random.Range(1, 16), coords = p.mousePosition.Value });
        }
        var hasMouse = p.mousePosition.HasValue;
        if (hasMouse)
        {
            points.Add(new Point { color = 7, coords = p.mousePosition.Value });
        }
        if (points.Count >= 3)
        {
            d = Delaunator.from(points, getX, getY);
        }
        if (points.Count >= 3)
        {
            for (int i = 0; i < d.triangles.Length; i += 3)
            {
                var v1 = points[(int)d.triangles[i]].coords;
                var v2 = points[(int)d.triangles[i + 1]].coords;
                var v3 = points[(int)d.triangles[i + 2]].coords;

                p.tri(v1, v2, v3, 7);
                var center = Delaunator.circumcenter(v1, v2, v3);
                var r = (uint)Mathf.RoundToInt(Vector2.Distance(center, v1));

                p.circ(center, r, 12);
            }
        }
        foreach (var point in points)
        {
            p.circfill(point.coords, 3, point.color);
            p.circ(point.coords, 3, 7);
        }
        if (p.mousePosition.HasValue)
        {
            p.circ(p.mousePosition.Value, 3, 7);

        }
        if (hasMouse)
        {

            points.RemoveAt(points.Count - 1);
        }
        p.flip();
    }
}
