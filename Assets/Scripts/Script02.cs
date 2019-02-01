using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script02 : MonoBehaviour
{
    private Pico p;
    public uint radius = 3;
    // Start is called before the first frame update
    void Start()
    {
        p = this.GetComponent<Pico>();
        p._init();
    }

    // Update is called once per frame
    void Update()
    {
        p.cls(0);

        for (int y = 0; y < p.Height; y += 8)
        {
            for (int x = 0; x < p.Width; x += 8)
            {
                if (((y * p.Height) + x) % 16 == 0)
                {
                    p.circfill(x, y, radius, 6);

                }
                else
                {
                    p.circ(x, y, radius, 6);
                }
            }
        }

        p.flip();
    }
}
