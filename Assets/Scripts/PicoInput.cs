using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PicoInput : MonoBehaviour
{
    Pico p;
    // Start is called before the first frame update
    void Start()
    {
        p = GetComponent<Pico>();
    }

    // Update is called once per frame
    void Update()
    {
        p.cls(0);
        if (p.mousePosition.HasValue)
        {
            var color = Input.GetMouseButton(0) ? 5: 9;
            p.circfill(p.mousePosition.Value, 15, color);
        }
        p.flip();
    }
}
