using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangles : MonoBehaviour
{
    private Pico p;
    Vector2 tl;
    Vector2 tr;
    Vector2 bl;
    Vector2 br;
    Vector2 middle;
    [Range(1.0f, 50f)]
    public float scale = 1.0f;
    [Range(0, 15)]
    public byte Color1 = 7;
    [Range(0, 15)]
    public byte Color2 = 12;
    [Range(0, 15)]
    public byte Color3 = 5;
    [Range(0, 15)]
    public byte Color4 = 13;

    // Start is called before the first frame update
    void Start()
    {
        p = this.GetComponent<Pico>();
        p._init();
        tl = new Vector2(0, 0) / 10;
        tr = new Vector2(p.Width - 1, 0) / 10;
        bl = new Vector2(0, p.Height - 1) / 10;
        br = new Vector2(p.Width - 1, p.Height - 1) / 10;
        middle = new Vector2(p.Width / 2, p.Height / 2) / 10;
    }

    void Pir(float x, float y, float w, float h, float cx, float cy)
    {
        var v1 = new Vector2(x, y);
        var v2 = new Vector2((w / 2) + cx + x, (h / 2) + cy + y);
        var v3 = new Vector2(x + w, y);
        var v4 = new Vector2(x + w, y + h);
        var v5 = new Vector2(x, y + h);
        p.trifill(
            v1,
            v2,
            v3,
            Color1
        );
        p.trifill(
            v3,
            v2,
            v4,
            Color2
        );
        p.trifill(
            v1,
            v2,
            v5,
            Color3
        );
        p.trifill(
            v5,
            v2,
            v4,
            Color4
        );
    }

    // Update is called once per frame
    void Update()
    {
        p.cls(0);
        // for (int x = 0; x < p.Width-1; x += 28)
        // {
        //     for (int y = 0; y < p.Height-1; x += 28)
        //     {
        //         var cx = 12 * Mathf.Sin(Time.realtimeSinceStartup * (x + y + 1));
        //         var cy = 12 * Mathf.Cos(Time.realtimeSinceStartup * (x + y + 1));
        //         Pir(x, y, 25, 25, x + cx, y + cy);
        //     }
        // }
        // Pir(10,10,50,50,0,0);
        // var width = (int)p.Width / 8;
        // var height = (int)p.Height / 8;
        for (int x = 0; x < p.Width; x += 28)
        {
            for (int y = 0; y < p.Height; y += 28)
            {
                var cx = scale * Mathf.Sin(Time.realtimeSinceStartup / 30 * (x + y + 2));
                var cy = scale * Mathf.Cos(Time.realtimeSinceStartup / 30 * (x + y + 2));
                Pir(x, y, 25, 25, cx, cy);
            }
        }

        p.flip();
    }
}
