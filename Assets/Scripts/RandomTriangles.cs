using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTriangles : MonoBehaviour
{
    private Pico p;
    bool rendered = false;
    // Start is called before the first frame update
    void Start()
    {
        p = this.GetComponent<Pico>();
        p._init();
    }

    public Vector2 RandomVector2(float x_max, float y_max, float x_min = 0.0f, float y_min = 0.0f)
    {
        return new Vector2(Random.Range(x_min, x_max), Random.Range(y_min, y_max));
    }
    public Vector2 RandomVector2(int x_max, int y_max, int x_min = 0, int y_min = 0)
    {
        return new Vector2(Random.Range(x_min, x_max), Random.Range(y_min, y_max));
    }

    // Update is called once per frame
    void Update()
    {
        if (rendered)
        {
            return;
        }
        // p.cls(0);
        // p.pset(0,0,7);
        for (int i = 0; i < 1; i++)
        {
            p.trifill(
                RandomVector2((int)p.Width, (int)p.Height),
                RandomVector2((int)p.Width, (int)p.Height),
                RandomVector2((int)p.Width, (int)p.Height),
                Random.Range(0, 16)
            );
        }
        // p.rect(-1, -1, (int)p.Width - 1, (int)p.Height - 1, 7);

        p.flip();
        // rendered = true;
    }
}
