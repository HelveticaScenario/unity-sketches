using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PicoReactionDiffusion : MonoBehaviour
{
    public struct Cell
    {
        public float A { get; set; }
        public float B { get; set; }
    }
    bool useBack = false;
    [Range(0.0f, 1.0f)]
    public float Da = 1.0f;
    [Range(0.0f, 1.0f)]
    public float Db = 0.5f;
    [Range(0.0f, 0.1f)]
    public float f = 0.055f;
    [Range(0.0f, 0.1f)]
    public float k = 0.062f;
    public ComputeShader compute;
    float centerWeight = -1;
    float adjacentWeight = 0.2f;
    float diagonalWeight = 0.05f;
    // Cell[,] front;
    // Cell[,] back;
    ComputeBuffer frontBuffA;
    ComputeBuffer frontBuffB;
    ComputeBuffer backBuffA;
    ComputeBuffer backBuffB;
    float[] arrA;
    float[] arrB;
    int w;
    int h;
    Pico p;

    int kernel;
    // Start is called before the first frame update

    void runShader(ComputeBuffer SourceA, ComputeBuffer SourceB, ComputeBuffer DestA, ComputeBuffer DestB)
    {
        compute.SetInt("w", w);
        compute.SetInt("h", h);
        compute.SetFloat("Da", Da);
        compute.SetFloat("Db", Db);
        compute.SetFloat("f", f);
        compute.SetFloat("k", k);
        compute.SetFloat("centerWeight", centerWeight);
        compute.SetFloat("adjacentWeight", adjacentWeight);
        compute.SetFloat("diagonalWeight", diagonalWeight);
        compute.SetBuffer(kernel, "SourceA", SourceA);
        compute.SetBuffer(kernel, "SourceB", SourceB);
        compute.SetBuffer(kernel, "DestA", DestA);
        compute.SetBuffer(kernel, "DestB", DestB);
        compute.Dispatch(kernel, w / 8, h / 8, 1);
    }

    void Start()
    {
        p = GetComponent<Pico>();
        w = (int)p.Width;
        h = (int)p.Height;
        kernel = compute.FindKernel("CSMain");

        frontBuffA = new ComputeBuffer(w * h, sizeof(float));
        frontBuffB = new ComputeBuffer(w * h, sizeof(float));
        backBuffA = new ComputeBuffer(w * h, sizeof(float));
        backBuffB = new ComputeBuffer(w * h, sizeof(float));
        arrA = new float[w * h];
        arrB = new float[w * h];

        // front = new Cell[p.Width, p.Height];
        // back = new Cell[p.Width, p.Height];

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                arrA[(y * w) + x] = 1.0f;
                if (Random.value < 0.1f)
                {
                    arrB[(y * w) + x] = 1.0f;
                }
                else
                {
                    arrB[(y * w) + x] = 0.0f;
                }
            }
        }
        frontBuffA.SetData(arrA);
        backBuffA.SetData(arrA);
        frontBuffB.SetData(arrB);
        backBuffB.SetData(arrB);
    }

    // float convolveA(int x, int y, ref Cell[,] arr)
    // {
    //     float v = 0;
    //     for (int _x = (x - 1) < 0 ? 0 : (x - 1); _x <= x + 1 && _x < w; _x++)
    //     {
    //         for (int _y = (y - 1) < 0 ? 0 : (y - 1); _y <= y + 1 && _y < h; _y++)
    //         {
    //             if (_x == x && _y == y)
    //             {
    //                 v += arr[_x, _y].A * centerWeight;
    //             }
    //             else if (_x == x || _y == y)
    //             {
    //                 v += arr[_x, _y].A * adjacentWeight;
    //             }
    //             else
    //             {
    //                 v += arr[_x, _y].A * diagonalWeight;
    //             }
    //         }
    //     }
    //     return v;
    // }

    // float convolveB(int x, int y, ref Cell[,] arr)
    // {
    //     float v = 0;
    //     for (int _x = (x - 1) < 0 ? 0 : (x - 1); _x <= x + 1 && _x < w; _x++)
    //     {
    //         for (int _y = (y - 1) < 0 ? 0 : (y - 1); _y <= y + 1 && _y < h; _y++)
    //         {
    //             if (_x == x && _y == y)
    //             {
    //                 v += arr[_x, _y].B * centerWeight;
    //             }
    //             else if (_x == x || _y == y)
    //             {
    //                 v += arr[_x, _y].B * adjacentWeight;
    //             }
    //             else
    //             {
    //                 v += arr[_x, _y].B * diagonalWeight;
    //             }
    //         }
    //     }
    //     return v;
    // }


    // Update is called once per frame
    void Update()
    {
        var t = 1;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    arrA[(y * w) + x] = 1.0f;
                    arrB[(y * w) + x] = 0.0f;
                }
            }
            frontBuffA.SetData(arrA);
            frontBuffB.SetData(arrB);
            backBuffA.SetData(arrA);
            backBuffB.SetData(arrB);
        }


        if (p.mousePosition.HasValue && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
        {
            var m = p.mousePosition.Value;
            if (useBack)
            {
                backBuffB.GetData(arrB);
                arrB[Mathf.FloorToInt(m.x) + (Mathf.FloorToInt(m.y) * w)] = Input.GetMouseButton(0) ? 1 : 0;
                backBuffB.SetData(arrB);
            }
            else
            {
                frontBuffB.GetData(arrB);
                arrB[Mathf.FloorToInt(m.x) + (Mathf.FloorToInt(m.y) * w)] = Input.GetMouseButton(0) ? 1 : 0;
                frontBuffB.SetData(arrB);
            }
        }

        if (useBack)
        {
            runShader(backBuffA, backBuffB, frontBuffA, frontBuffB);
            frontBuffA.GetData(arrA);
            frontBuffB.GetData(arrB);
        }
        else
        {
            runShader(frontBuffA, frontBuffB, backBuffA, backBuffB);
            backBuffA.GetData(arrA);
            backBuffB.GetData(arrB);
        }


        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                // float A;
                // float B;
                // if (useBack)
                // {
                //     A = back[x, y].A;
                //     B = back[x, y].B;
                //     A += ((Da * convolveA(x, y, ref back)) - (A * B * B) + (f * (1 - A))) * t;
                //     B += ((Db * convolveB(x, y, ref back)) + (A * B * B) - ((k + f) * B)) * t;
                //     if (A < 0)
                //     {
                //         A = 0;
                //     }
                //     else if (A > 1)
                //     {
                //         A = 1;
                //     }
                //     if (B < 0)
                //     {
                //         B = 0;
                //     }
                //     else if (B > 1)
                //     {
                //         B = 1;
                //     }
                //     front[x, y].A = A;
                //     front[x, y].B = B;
                // }
                // else
                // {
                //     A = front[x, y].A;
                //     B = front[x, y].B;
                //     A += ((Da * convolveA(x, y, ref front)) - (A * B * B) + (f * (1 - A))) * t;
                //     B += ((Db * convolveB(x, y, ref front)) + (A * B * B) - ((k + f) * B)) * t;
                //     if (A < 0)
                //     {
                //         A = 0;
                //     }
                //     else if (A > 1)
                //     {
                //         A = 1;
                //     }
                //     if (B < 0)
                //     {
                //         B = 0;
                //     }
                //     else if (B > 1)
                //     {
                //         B = 1;
                //     }
                //     back[x, y].A = A;
                //     back[x, y].B = B;
                // }
                // var c = Mathf.FloorToInt((A / (A + B)) * 15);
                // if (Random.value < 0.0001)
                // {
                //     print(A + " " + B + " " + (A / (A + B)) + " " + c);

                // }
                // if (Random.value > 0.2f) {
                //     continue;
                // }
                var a = arrA[(y * w) + x];
                var b = arrB[(y * w) + x];
                var c = (int)((a / (a + b)) * 15);
                p.pset(x, y, c);
            }
        }
        useBack = !useBack;
        p.flip();
    }

    void OnDestroy()
    {
        frontBuffA.Release();
        frontBuffA = null;
        frontBuffB.Release();
        frontBuffB = null;
        backBuffA.Release();
        backBuffA = null;
        backBuffB.Release();
        backBuffB = null;
    }
}
