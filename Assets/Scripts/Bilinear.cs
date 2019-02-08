using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bilinear : MonoBehaviour
{
    struct Dim
    {
        public int width { get; set; }
        public int height { get; set; }
    }
    Dim gridDim = new Dim { width = 3, height = 3 };
    float[,] grid;
    public float changeBy = 0.005f;

    private Pico p;
    public uint radius = 1;
    // Start is called before the first frame update
    void Start()
    {
        grid = makeGrid(gridDim.width, gridDim.height, (row, col) => Random.Range(0.0f, 1.0f));
        p = this.GetComponent<Pico>();
        p._init();
    }

    bool inBounds(int x, int y, int bound_x, int bound_y)
    {
        if (x < 0)
        {
            return false;
        }
        else if (y < 0)
        {
            return false;
        }
        else if (x > bound_x)
        {
            return false;
        }
        else if (y > bound_y)
        {
            return false;
        }
        return true;
    }

    delegate float gridInitializer(int row, int col);

    float[,] makeGrid(int width, int height, gridInitializer f)
    {
        float[,] grid = new float[height, width];
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                grid[row, col] = f(row, col);
            }
        }
        return grid;
    }

    float inner(float f00, float f10, float f01, float f11, float x, float y)
    {
        var un_x = 1.0f - x;
        var un_y = 1.0f - y;
        return (f00 * un_x * un_y + f10 * x * un_y + f01 * un_x * y + f11 * x * y);
    }


    float wrap(float v)
    {
        while (v < 0)
        {
            v += 1;
        }

        while (v > 1)
        {
            v -= 1;
        }

        return v;
    }

    float e = 0.0f;
    // Update is called once per frame
    void Update()
    {
        p.cls(0);

        // var inner_range_row = Mathf.Floor(32.0f / (grid_dim.height - 1)) - 1;

        // var inner_range_col = Mathf.Floor(32.0f / (grid_dim.width - 1)) - 1;

        // for (int y = 0; y < p.Height; y += 8)
        // {
        //     for (int x = 0; x < p.Width; x += 8)
        //     {
        //         if (((y * p.Height) + x) % 16 == 0)
        //         {
        //             p.circfill(x, y, radius, 6);

        //         }
        //         else
        //         {
        //             p.circ(x, y, radius, 6);
        //         }
        //     }
        // }
        var inner_range_row = Mathf.FloorToInt(32 / (gridDim.height - 1)) - 1;
        var inner_range_col = Mathf.FloorToInt(32 / (gridDim.width - 1)) - 1;

        for (int q_col = 1; q_col < gridDim.width; q_col++)
        {
            for (int q_row = 1; q_row < gridDim.height; q_row++)
            {
                var points = new float[,]{
                    { grid[q_col-1,q_row-1], grid[q_col-1,q_row] },
                    { grid[q_col,q_row-1],   grid[q_col,q_row]   }
                };
                for (int col = 0; col <= inner_range_col; col++)
                {
                    for (int row = 0; row <= inner_range_row; row++)
                    {
                        float row_norm = (float)row / (float)inner_range_row;
                        float col_norm = (float)col / (float)inner_range_col;
                        float v = inner(points[0, 0], points[1, 0], points[0, 1], points[1, 1], col_norm, row_norm);
                        var c = (byte)Mathf.RoundToInt(wrap(v + e) * 15);
                        p.circ(
                            (
                                (q_col - 1) *
                                inner_range_col +
                                1 +
                                col

                            ) * 4,
                            (
                                (q_row - 1) *
                                inner_range_row +
                                1 +
                                row
                            ) * 4,
                            3,
                            c
                        );
                        // p.pset(col, row, c);
                    }
                }
            }
        }
        e += Time.deltaTime * changeBy;
        e = wrap(e);

        p.flip();
    }
}
