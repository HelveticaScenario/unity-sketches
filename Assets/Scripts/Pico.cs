using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pico : MonoBehaviour
{
    public uint Width;
    public uint Height;
    private Texture2D screen;
    private Texture2D palette;
    private Texture2D swap;
    private Unity.Collections.NativeArray<byte> screenData;
    private Unity.Collections.NativeArray<byte> swapData;
    private Unity.Collections.NativeArray<Color32> paletteData;
    private int[] SidesBufferLeft;
    private int[] SidesBufferRight;
    private bool screenChanged = false;
    private bool swapChanged = false;
    private bool paletteChanged = false;
    private bool initialized = false;
    // private
    // Start is called before the first frame update
    public void _init(FilterMode filterMode = FilterMode.Point)
    {
        if (initialized)
        {
            return;
        }
        screen = new Texture2D((int)Width, (int)Height, TextureFormat.R8, false, false);
        screen.filterMode = filterMode;
        swap = new Texture2D(256, 1, TextureFormat.R8, false, false);
        swap.filterMode = FilterMode.Point;
        palette = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
        palette.filterMode = FilterMode.Point;
        Color32[] colors = {
            new Color32(0, 0, 0, 255), // 0 black
            new Color32(29, 43, 83, 255), // 1 dark blue
            new Color32(126, 37, 83, 255), // 2 dark purple
            new Color32(0, 135, 81, 255), // 3 dark green
            new Color32(171, 82, 54, 255), // 4 brown
            new Color32(95, 87, 79, 255), // 5 dark gray
            new Color32(194, 195, 199, 255), // 6 light gray
            new Color32(255, 241, 232, 255), // 7 white
            new Color32(255, 0, 77, 255), // 8 red
            new Color32(255, 163, 0, 255), // 9 range
            new Color32(255, 236, 39, 255), // 10 yellow
            new Color32(0, 228, 54, 255), // 11 green
            new Color32(41, 173, 255, 255), // 12 blue
            new Color32(131, 118, 156, 255), // 13 indigo
            new Color32(255, 119, 168, 255), // 14 pink
            new Color32(255, 204, 170, 255) // 15 peach
        };
        paletteData = palette.GetRawTextureData<Color32>();
        for (int i = 0; i < colors.Length; i++)
        {
            paletteData[i] = colors[i];
        }
        palette.Apply();
        swapData = swap.GetRawTextureData<byte>();
        for (int i = 0; i < swapData.Length; i++)
        {
            swapData[i] = (byte)(i);
        }
        swap.Apply();
        screenData = screen.GetRawTextureData<byte>();
        for (int i = 0; i < screenData.Length; i++)
        {
            screenData[i] = 0;
        }
        screen.Apply();
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.SetTexture("_MainTex", screen);
        renderer.material.SetTexture("_SwapTex", swap);
        renderer.material.SetTexture("_PaletteTex", palette);


        SidesBufferLeft = new int[(int)Height];
        SidesBufferRight = new int[(int)Height];

        initialized = true;
    }

    void initSidesBuffer()
    {
        for (int i = 0; i < Height; i++)
        {
            SidesBufferLeft[i] = (int)Width;
            SidesBufferRight[i] = -1;
        }
    }

    void setSidePixel(int x, int y)
    {
        if (y >= 0 && y < Height)
        {
            if (x < SidesBufferLeft[y]) SidesBufferLeft[y] = x;
            if (x > SidesBufferRight[y]) SidesBufferRight[y] = x;
        }
    }

    public byte wrapInt(int i)
    {
        while (i < 0)
        {
            i += 256;
        }
        return (byte)(i % 256);
    }
    public float rnd(float max = 1f)
    {
        return Random.Range(0f, max);
    }

    public unsafe void memset(ulong start, ulong length, byte c)
    {
        if (start + length > (ulong)screenData.Length || length == 0)
        {
            return;
        }
        screenData[(int)start] = c;
        if (length == 1)
        {
            return;
        }
        var ptr = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(screenData);
        ulong _s = (ulong)ptr + start;
        var s = (void*)_s;
        ulong _d = _s + 1;
        var d = (void*)_d;
        Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpyReplicate(d, s, 1, (int)length - 1);
        screenChanged = true;
    }

    public unsafe void memcpy(ulong source, ulong dest, ulong length)
    {
        if ((source + length >= (ulong)screenData.Length) || (dest + length >= (ulong)screenData.Length) || length == 0)
        {
            return;
        }
        // screenData[(int)start] = c;
        var ptr = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(screenData);
        var s = (ulong)ptr + source;
        var d = (ulong)ptr + dest;
        Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemMove((void*)d, (void*)s, (long)length);
        screenChanged = true;
    }

    public unsafe void cls(byte c)
    {
        memset(0, (ulong)screenData.Length, c);
    }
    public unsafe void cls2(byte c)
    {
        for (int i = 0; i < screenData.Length; i++)
        {
            screenData[i] = c;
        }
        screenChanged = true;
    }

    public void pset(int x, int y, byte c)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height)
        {
            return;
        }
        screenData[(int)((y * Width) + x)] = c;
        screenChanged = true;
    }
    byte pget(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height)
        {
            return 0;
        }
        return screenData[(int)((y * Width) + x)];
    }
    public void circ(int x, int y, uint radius, byte color)
    {
        int r = (int)radius;
        int _x = -r, _y = 0, err = 2 - 2 * r;
        do
        {
            pset(x - _x, y + _y, color);
            pset(x - _y, y - _x, color);
            pset(x + _x, y - _y, color);
            pset(x + _y, y + _x, color);
            r = err;
            if (r <= _y) err += ++_y * 2 + 1;
            if (r > _x || err > _y) err += ++_x * 2 + 1;
        } while (_x < 0);
    }

    public void circfill(int xm, int ym, uint radius, byte color)
    {
        if (radius <= 0)
        {
            pset(xm, ym, color);
            return;
        }
        if (radius == 1)
        {
            circ(xm, ym, radius, color);
            pset(xm, ym, color);
            return;
        }
        initSidesBuffer();

        int r = (int)radius;
        int x = -r, y = 0, err = 2 - 2 * r;
        do
        {
            setSidePixel(xm - x, ym + y);
            setSidePixel(xm - y, ym - x);
            setSidePixel(xm + x, ym - y);
            setSidePixel(xm + y, ym + x);

            r = err;
            if (r <= y) err += ++y * 2 + 1;
            if (r > x || err > y) err += ++x * 2 + 1;
        } while (x < 0);

        int yt = Mathf.Max(0, ym - (int)radius);
        int yb = Mathf.Min((int)Height, ym + (int)radius + 1);
        byte final_color = color;
        for (int _y = yt; _y < yb; _y++)
        {
            var xl = Mathf.Max(SidesBufferLeft[_y], 0);
            var xr = Mathf.Min(SidesBufferRight[_y], (int)Width - 1);
            hline(xl, xr, _y, final_color);
        }
    }

    void hline(int xl, int xr, int y, byte c)
    {
        memset(((ulong)y * (ulong)Width) + (ulong)xl, (ulong)(xr - xl + 1), c);
    }
    public void line(int x0, int y0, int x1, int y1, int c)
    {
        byte _c = wrapInt(c);
        x0 = Mathf.Min(Mathf.Max(x0, 0), (int)Width - 1);
        x1 = Mathf.Min(Mathf.Max(x1, 0), (int)Width - 1);
        y0 = Mathf.Min(Mathf.Max(y0, 0), (int)Height - 1);
        y1 = Mathf.Min(Mathf.Max(y1, 0), (int)Height - 1);
        if (x0 > x1)
        {
            int tmp = x0;
            x0 = x1;
            x1 = tmp;
        }
        if (y0 > y1)
        {
            int tmp = y0;
            y0 = y1;
            y1 = tmp;
        }

        if (y0 == y1)
        {
            if (x0 == x1)
            {
                pset(x0, y0, _c);
                return;
            }
            hline(x0, x1, y0, _c);
            return;
        }

        var dx = Mathf.Abs(x1 - x0);
        var dy = Mathf.Abs(y1 - y0);
        var sx = (x0 < x1) ? 1 : -1;
        var sy = (y0 < y1) ? 1 : -1;
        var err = dx - dy;
        while (true)
        {
            pset(x0, y0, _c);

            if ((x0 == x1) && (y0 == y1)) break;
            var e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
        screenChanged = true;
    }
    public byte randomColor()
    {
        return (byte)Random.Range(0, 16);
    }

    // Update is called once per frame
    public void flip()
    {
        // var screenData = screen.GetRawTextureData<byte>();
        if (screenChanged)
        {
            screen.Apply();
            screenChanged = false;
        }
        if (paletteChanged)
        {
            palette.Apply();
            paletteChanged = false;
        }
        if (swapChanged)
        {
            swap.Apply();
            swapChanged = false;
        }
    }
}
