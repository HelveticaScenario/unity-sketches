using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomLines : MonoBehaviour
{
    private Pico p;
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

        for (int y = 0; y < p.Height; y++)
        {
            for (int x = 0; x < p.Width; x++)
            {

                p.pset(x, y, (byte)Random.Range(0, 2));
            }
        }
        for (int i = 0; i < 100; i++)
        {
            p.line(Random.Range(0, (int)p.Width), Random.Range(0, (int)p.Height), Random.Range(0, (int)p.Width), Random.Range(0, (int)p.Height), p.randomColor());
        }

        p.flip();
    }
}
