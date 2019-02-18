using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PicoSprites : MonoBehaviour
{
    Pico p;
    // Start is called before the first frame update
    Vector2 v = new Vector2(4, 4);
    void Start()
    {
        p = GetComponent<Pico>();

    }

    // Update is called once per frame
    void Update()
    {
        p.cls();
        if (p.mousePosition.HasValue)
        {
            if (Input.GetMouseButton(0))
            {
                p.spr(0, p.mousePosition.Value - (v * 3), 3, 3, 2, 2);
            }
            else if (Input.GetMouseButton(1))
            {
                p.spr(0, p.mousePosition.Value - (v * new Vector2(3, 1)), 3, 1, 2, 2);
            }
            else
            {
                p.spr(0, p.mousePosition.Value - v, 1, 1, 2, 2);
            }
        }
        p.flip();
    }
}
