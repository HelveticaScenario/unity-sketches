using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiJack;

public class InterferencePattern : MonoBehaviour
{
    public int radius = 1;
    public int Radius { get { return radius; } set { radius = value; } }

    public Vector2[] points = new Vector2[2] { new Vector2(30, 30), new Vector2(90, 90) };

    [Range(1, 100)]
    public int mod = 50;
    public int Mod { get { return mod; } set { mod = value; } }

    [Range(1, 100)]
    public int limit = 20;
    public int Limit { get { return limit; } set { limit = value; } }

    [Range(1.0f, 10.0f)]
    public float mag = 2.0f;
    public float Mag { get { return mag; } set { mag = value; } }

    [Range(0, 15)]
    public int insideColor = 7;
    public int InsideColor { get { return insideColor; } set { insideColor = value; } }

    [Range(0, 15)]
    public int outsideColor = 12;
    public int OutsideColor { get { return outsideColor; } set { outsideColor = value; } }

    [Range(0.0f, 1.0f)]
    public float chance = 0.5f;
    public float Chance { get { return chance; } set { chance = value; } }
    private Pico p;
    // Start is called before the first frame update
    void Start()
    {
        p = this.GetComponent<Pico>();
        p._init(FilterMode.Bilinear);

    }
    float t = 0.0f;
    // Update is called once per frame
    void Update()
    {
        var vec = new Vector2(0, 0);
        t += Time.deltaTime * mag;
        mag = MidiMaster.GetKnob(MidiChannel.Ch16, 1, 0) * 10;
        // p.cls(outsideColor);
        for (int y = 0; y < p.Height; y++)
        {
            for (int x = 0; x < p.Width; x++)
            {
                // if (Random.value >= chance)
                // {
                //     continue;
                // }
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
