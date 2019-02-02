using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterferencePattern : MonoBehaviour
{
    public uint radius = 1;
    public Vector2[] points = new Vector2[2] { new Vector2(30, 30), new Vector2(90, 90) };
    [Range(1, 100)]
    public uint mod = 50;
    [Range(1, 100)]
    public uint limit = 20;
    [Range(1.0f, 10.0f)]
    public float mag = 2.0f;
    [Range(0, 15)]
    public byte insideColor = 7;
    [Range(0, 15)]
    public byte outsideColor = 12;
    private Pico p;
    // Start is called before the first frame update



    void Start()
    {
        p = this.GetComponent<Pico>();
        p._init(FilterMode.Bilinear);
    }

    // Update is called once per frame
    void Update()
    {
        var vec = new Vector2(0, 0);
        var t = Time.realtimeSinceStartup * mag;

        for (int y = 0; y < p.Height; y++)
        {
            for (int x = 0; x < p.Width; x++)
            {
                vec.Set(x, y);
                var inside = 0;
                for (int i = 0; i < points.Length; i++)
                {
                    inside += Mathf.FloorToInt((vec - points[i]).magnitude + t) % mod < limit ? 1 : 0;
                }
                if (inside == 1)
                {
                    p.pset(x, y, insideColor);
                }
                else
                {
                    p.pset(x, y, outsideColor);
                }
            }
        }

        p.flip();
    }
}
