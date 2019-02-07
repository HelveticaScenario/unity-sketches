using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dithering : MonoBehaviour
{
    // Start is called before the first frame update
    private Pico p;
    [Range(0.0f, 100.0f)]
    public float speed = 1.0f;
    void Start()
    {
        p = this.GetComponent<Pico>();

        p._init();

    }

    bool r = false;
    // Update is called once per frame
    void Update()
    {
        if (r)
        {
            return;
        }
        // r = true;
        p.cls(0);
        var s = Mathf.RoundToInt(Time.realtimeSinceStartup * speed);
        for (var y = 0; y < p.Height / 4; y++)
        {
            if (y % 2 == 0)
            {
                p.hline(0, (int)p.Width - 1, ((y * 4 + 0) + s) % (int)p.Height, new int[] {
                    (int)(y / 2 % 15 + 1)
                });
                p.hline(0, (int)p.Width - 1, ((y * 4 + 1) + s) % (int)p.Height, new int[] {
                    (int)(y / 2 % 15 + 1)
                });
                p.hline(0, (int)p.Width - 1, ((y * 4 + 2) + s) % (int)p.Height, new int[] {
                    (int)(y / 2 % 15 + 1)
                });
                p.hline(0, (int)p.Width - 1, ((y * 4 + 3) + s) % (int)p.Height, new int[] {
                    (int)(y / 2 % 15 + 1)
                });
            }
            else
            {
                var q = (float)y / 2.0f;

                p.hline(0, (int)p.Width - 1, ((y * 4 + 0) + s) % (int)p.Height, new int[] {
                    (int)(Mathf.FloorToInt(q) % 15 + 1),
                    (int)(Mathf.FloorToInt(q) % 15 + 1),
                    (int)(Mathf.CeilToInt(q) % 15 + 1),
                    (int)(Mathf.CeilToInt(q) % 15 + 1),
                });
                p.hline(0, (int)p.Width - 1, ((y * 4 + 1) + s) % (int)p.Height, new int[] {
                    (int)(Mathf.FloorToInt(q) % 15 + 1),
                    (int)(Mathf.FloorToInt(q) % 15 + 1),
                    (int)(Mathf.CeilToInt(q) % 15 + 1),
                    (int)(Mathf.CeilToInt(q) % 15 + 1),
                });
                p.hline(0, (int)p.Width - 1, ((y * 4 + 2) + s) % (int)p.Height, new int[] {
                    (int)(Mathf.FloorToInt(q) % 15 + 1),
                    (int)(Mathf.FloorToInt(q) % 15 + 1),
                    (int)(Mathf.CeilToInt(q) % 15 + 1),
                    (int)(Mathf.CeilToInt(q) % 15 + 1),
                });
                p.hline(0, (int)p.Width - 1, ((y * 4 + 3) + s) % (int)p.Height, new int[] {
                    (int)(Mathf.FloorToInt(q) % 15 + 1),
                    (int)(Mathf.FloorToInt(q) % 15 + 1),
                    (int)(Mathf.CeilToInt(q) % 15 + 1),
                    (int)(Mathf.CeilToInt(q) % 15 + 1),
                });
            }
        }
        p.flip();

    }
}
