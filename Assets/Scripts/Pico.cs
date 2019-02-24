using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pico : MonoBehaviour
{
    struct ClipRect
    {
        public int l { get; set; }
        public int t { get; set; }
        public int r { get; set; }
        public int b { get; set; }
    }
    public uint Width;
    public uint Height;
    public FilterMode filterMode;
    public Texture2D spritesheet;
    private Texture2D spritesheetIndexed;
    private Texture2D screen;
    private Texture2D palette;
    private Texture2D swap;
    private Unity.Collections.NativeArray<byte> screenData;
    private Unity.Collections.NativeArray<byte> swapData;
    private Unity.Collections.NativeArray<Color32> paletteData;
    private Unity.Collections.NativeArray<byte> spritesheetIndexedData;
    private int[] SidesBufferLeft;
    private int[] SidesBufferRight;
    private bool screenChanged = false;
    private bool swapChanged = false;
    private bool paletteChanged = false;
    private bool initialized = false;
    private ClipRect clipRect;
    public AnimationCurve a;
    public Vector2? mousePosition { get; set; }
    [Range(0.0f, 1.0f)]
    public float chance = 1.0f;
    public float Chance { get { return chance; } set { chance = value; } }
    public bool dither = false;
    public bool Dither { get { return dither; } set { dither = value; } }
    Camera m_MainCamera;
    void Start()
    {
        m_MainCamera = Camera.main;
        _init();
        // QualitySettings.vSyncCount = 2;
    }

    void Update()
    {
        if (m_MainCamera)
        {
            RaycastHit hit;
            Ray ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit) && hit.transform == this.transform)
            {
                var m = Vector2.Scale(hit.textureCoord, new Vector2(Width - 1, -(Height - 1)));
                mousePosition = new Vector2(m.x, m.y + Height - 1);
            }
            else
            {
                mousePosition = null;
            }
        }

    }

    // private
    // Start is called before the first frame update
    public void _init(FilterMode _filterMode = FilterMode.Point)
    {
        if (spritesheet.height != 256 || spritesheet.width != 256)
        {
            throw new System.ArgumentException();
        }
        if (initialized)
        {
            return;
        }
        screen = new Texture2D((int)Width, (int)Height, TextureFormat.R8, false, false);
        screen.filterMode = this.filterMode;
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
            new Color32(255, 204, 170, 255), // 15 peach
            new Color32(0, 0, 0, 0), // 16 transparent
        };
        // for (byte i = 0; i < 16; i++)
        // {
        //     // byte v = (byte)Mathf.FloorToInt(Mathf.Clamp(a.Evaluate((float)i / 15.0f), 0, 1) );
        //     colors[i] = Color32.Lerp(new Color32(255, 241, 232, 255), new Color32(41, 173, 255, 255), a.Evaluate((float)i / 15.0f));
        // }

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

        clipRect = new ClipRect { l = 0, t = 0, r = (int)Width, b = (int)Height };


        if (spritesheet != null)
        {
            spritesheetIndexed = new Texture2D(spritesheet.width, spritesheet.height, TextureFormat.R8, false, false);
            spritesheetIndexed.filterMode = FilterMode.Point;
            spritesheetIndexedData = spritesheetIndexed.GetRawTextureData<byte>();
            var spritesheetData = spritesheet.GetPixels32(0);

            Dictionary<Color32, byte> colorMap = new Dictionary<Color32, byte>();
            for (int i = 0; i < colors.Length; i++)
            {
                colorMap.Add(colors[i], (byte)i);
            }
            int counter = colorMap.Count;
            for (int i = 0; i < spritesheetData.Length; i++)
            {
                byte index;
                if (!colorMap.TryGetValue(spritesheetData[i], out index))
                {
                    index = (byte)counter;
                    colorMap.Add(spritesheetData[i], index);
                    paletteData[index] = spritesheetData[i];
                    counter += 1;
                }
                var corrected = indexToVector2Int(i, spritesheet.width, spritesheet.height);
                corrected = new Vector2Int(corrected.x, Mathf.Abs(corrected.y - (spritesheet.height - 1)));

                spritesheetIndexedData[vector2IntToIndex(corrected, spritesheet.width, spritesheet.height)] = index;
            }
            spritesheetIndexed.Apply();
            palette.Apply();
        }
        else
        {
            spritesheetIndexed = new Texture2D(256, 256, TextureFormat.R8, false, false);
            spritesheetIndexed.filterMode = FilterMode.Point;
            spritesheetIndexedData = spritesheetIndexed.GetRawTextureData<byte>();
        }

        initialized = true;
    }

    Vector2Int indexToVector2Int(int index, int width, int height)
    {
        return new Vector2Int(index % width, index / height);
    }
    int vector2IntToIndex(Vector2Int v, int width, int height)
    {
        return v.x + (v.y * height);
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

    public unsafe void cls(int c = 0)
    {
        memset(0, (ulong)screenData.Length, wrapInt(c));
    }
    public unsafe void cls2(int c)
    {
        for (int i = 0; i < screenData.Length; i++)
        {
            screenData[i] = wrapInt(c);
        }
        screenChanged = true;
    }

    public void pset(int x, int y, int c)
    {
        if (dither && Random.value > chance)
        {
            return;
        }
        if (x < clipRect.l || y < clipRect.t || x >= clipRect.r || y >= clipRect.b)
        {
            return;
        };

        // y = (y - (int)Height) * -1;

        screenData[(int)((y * Width) + x)] = wrapInt(c);
        screenChanged = true;
    }

    public void pset(Vector2 v, int c)
    {
        pset(
            Mathf.RoundToInt(v.x),
            Mathf.RoundToInt(v.y),
            c
        );
    }

    int pget(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height)
        {
            return 0;
        }
        return screenData[(int)((y * Width) + x)];
    }

    void clip(int x, int y, int width, int height)
    {

        clipRect.l = x;
        clipRect.t = y;
        clipRect.r = x + width;
        clipRect.b = y + height;

        if (clipRect.l < 0) clipRect.l = 0;
        if (clipRect.t < 0) clipRect.t = 0;
        if (clipRect.r > Width) clipRect.r = (int)Width;
        if (clipRect.b > Height) clipRect.b = (int)Height;
    }

    void triPixelFunc(int x, int y, int color)
    {
        // if (x < clipRect.l || y < clipRect.t || x >= clipRect.r || y >= clipRect.b)
        // {
        //     return;
        // };
        setSidePixel(x, y);
    }

    public void rect(int x0, int y0, int x1, int y1, int color)
    {
        if (
            (y0 < (int)clipRect.t && y1 < (int)clipRect.t) ||
            (y0 >= (int)clipRect.b && y1 >= (int)clipRect.b) ||
            (x0 < (int)clipRect.l && x1 < (int)clipRect.l) ||
            (x0 >= (int)clipRect.r && x1 >= (int)clipRect.r)
        )
        {
            return;
        }
        byte c = wrapInt(color);
        if (x0 > x1)
        {
            var tmp = x0;
            x0 = x1;
            x1 = tmp;
        }
        if (y0 > y1)
        {
            var tmp = y0;
            y0 = y1;
            y1 = tmp;
        }
        var _x0 = Mathf.Min(Mathf.Max(x0, clipRect.l), (int)clipRect.r - 1);
        var _x1 = Mathf.Min(Mathf.Max(x1, clipRect.l), (int)clipRect.r - 1);
        var _y0 = Mathf.Min(Mathf.Max(y0, clipRect.t), (int)clipRect.b - 1);
        var _y1 = Mathf.Min(Mathf.Max(y1, clipRect.t), (int)clipRect.b - 1);



        if (y0 >= (int)clipRect.t && y0 < (int)clipRect.b)
        {
            hline(_x0, _x1, _y0, c);
        }
        if (y1 >= (int)clipRect.t && y1 < (int)clipRect.b)
        {
            hline(_x0, _x1, _y1, c);
        }
        if (x0 >= (int)clipRect.l && x0 < (int)clipRect.r)
        {
            for (var y = _y0; y <= _y1; y++)
            {
                pset(_x0, y, c);
            }
        }
        if (x1 >= (int)clipRect.l && x1 < (int)clipRect.r)
        {
            for (var y = _y0; y <= _y1; y++)
            {
                pset(_x1, y, c);
            }
        }

    }
    public void rect(Vector2 v1, Vector2 v2, int color)
    {
        rect(
            Mathf.RoundToInt(v1.x),
            Mathf.RoundToInt(v1.y),
            Mathf.RoundToInt(v2.x),
            Mathf.RoundToInt(v2.y),
            color
        );
    }
    public void rectfill(int x0, int y0, int x1, int y1, int color)
    {
        rectfill(x0, y0, x1, y1, new int[] { color });
    }
    public void rectfill(int x0, int y0, int x1, int y1, int[] color)
    {
        if (color.Length == 0)
        {
            return;
        }
        int[] c = new int[color.Length];
        for (int i = 0; i < color.Length; i++)
        {
            c[i] = wrapInt(color[i]);
        }
        if (
            (y0 < (int)clipRect.t && y1 < (int)clipRect.t) ||
            (y0 >= (int)clipRect.b && y1 >= (int)clipRect.b) ||
            (x0 < (int)clipRect.l && x1 < (int)clipRect.l) ||
            (x0 >= (int)clipRect.r && x1 >= (int)clipRect.r)
        )
        {
            return;
        }
        if (x0 > x1)
        {
            var tmp = x0;
            x0 = x1;
            x1 = tmp;
        }
        if (y0 > y1)
        {
            var tmp = y0;
            y0 = y1;
            y1 = tmp;
        }
        var _x0 = Mathf.Min(Mathf.Max(x0, clipRect.l), (int)clipRect.r - 1);
        var _x1 = Mathf.Min(Mathf.Max(x1, clipRect.l), (int)clipRect.r - 1);
        var _y0 = Mathf.Min(Mathf.Max(y0, clipRect.t), (int)clipRect.b - 1);
        var _y1 = Mathf.Min(Mathf.Max(y1, clipRect.t), (int)clipRect.b - 1);

        for (var y = _y0; y <= _y1; y++)
        {
            hline(_x0, _x1, y, c);
        }

    }
    public void rectfill(Vector2 v1, Vector2 v2, int color)
    {
        rectfill(
            Mathf.RoundToInt(v1.x),
            Mathf.RoundToInt(v1.y),
            Mathf.RoundToInt(v2.x),
            Mathf.RoundToInt(v2.y),
            color
        );
    }

    public void tri(int x1, int y1, int x2, int y2, int x3, int y3, int color)
    {
        line(x1, y1, x2, y2, color);
        line(x2, y2, x3, y3, color);
        line(x3, y3, x1, y1, color);
    }

    public void tri(Vector2 v1, Vector2 v2, Vector2 v3, int color)
    {
        tri(
            Mathf.RoundToInt(v1.x),
            Mathf.RoundToInt(v1.y),
            Mathf.RoundToInt(v2.x),
            Mathf.RoundToInt(v2.y),
            Mathf.RoundToInt(v3.x),
            Mathf.RoundToInt(v3.y),
            color
        );
    }

    public void trifill(int x1, int y1, int x2, int y2, int x3, int y3, int color)
    {

        initSidesBuffer();

        _line(x1, y1, x2, y2, color, triPixelFunc);
        _line(x2, y2, x3, y3, color, triPixelFunc);
        _line(x3, y3, x1, y1, color, triPixelFunc);

        byte final_color = wrapInt(color);
        int yt = Mathf.Max(clipRect.t, Mathf.Min(y1, Mathf.Min(y2, y3)));
        int yb = Mathf.Min(clipRect.b, Mathf.Max(y1, Mathf.Max(y2, y3)) + 1);

        for (int y = yt; y < yb; y++)
        {
            int xl = Mathf.Max(SidesBufferLeft[y], clipRect.l);
            int xr = Mathf.Min(SidesBufferRight[y], clipRect.r - 1);
            hline(xl, xr, y, final_color);
        }
    }

    public void trifill(Vector2 v1, Vector2 v2, Vector2 v3, int color)
    {
        trifill(
            Mathf.RoundToInt(v1.x),
            Mathf.RoundToInt(v1.y),
            Mathf.RoundToInt(v2.x),
            Mathf.RoundToInt(v2.y),
            Mathf.RoundToInt(v3.x),
            Mathf.RoundToInt(v3.y),
            color
        );
    }

    public void circ(int x, int y, uint radius, int color)
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
    public void circ(Vector2 v, uint radius, int color)
    {
        circ(
            Mathf.RoundToInt(v.x),
            Mathf.RoundToInt(v.y),
            radius,
            color
        );
    }

    public void circfill(int xm, int ym, uint radius, int color)
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
        int final_color = color;
        for (int _y = yt; _y < yb; _y++)
        {
            int xl = Mathf.Max(SidesBufferLeft[_y], clipRect.l);
            int xr = Mathf.Min(SidesBufferRight[_y], clipRect.r - 1);
            hline(xl, xr, _y, final_color);
        }
    }

    public void circfill(Vector2 v, uint radius, int color)
    {
        circfill(
            Mathf.RoundToInt(v.x),
            Mathf.RoundToInt(v.y),
            radius,
            color
        );
    }
    public unsafe void hline(int x0, int x1, int y, int[] cs)
    {
        if (
            cs.Length == 0 ||
            (x0 < clipRect.l && x1 < clipRect.l) ||
            (x0 >= clipRect.r && x1 >= clipRect.r) ||
            y < clipRect.t ||
            y >= clipRect.b
        )
        {
            return;
        }

        if (x0 > x1)
        {
            var tmp = x0;
            x0 = x1;
            x1 = tmp;
        }

        x0 = Mathf.Min(Mathf.Max(x0, clipRect.l), clipRect.r - 1);
        x1 = Mathf.Min(Mathf.Max(x1, clipRect.l), clipRect.r - 1);
        if (cs.Length == 1)
        {
            hline(x0, x1, y, cs[0]);
            return;
        }
        else
        {
            var len = x1 - x0 + 1;
            var rem = len % cs.Length;
            len = len - rem;
            len = len / cs.Length;
            if (len > 0)
            {
                var ptr = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(screenData);
                for (int i = 0; i < cs.Length; i++)
                {
                    pset(i + x0, y, cs[i]);
                }
                ulong _s = (ulong)ptr + ((ulong)y * (ulong)Width) + (ulong)x0;
                var s = (void*)_s;
                ulong _d = _s + (ulong)cs.Length;
                var d = (void*)_d;
                Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpyReplicate(d, s, cs.Length, (int)len - 1);
            }
            for (int i = 0; i < rem; i++)
            {
                pset(len * cs.Length + i + x0, y, cs[i]);
            }
        }
        screenChanged = true;
    }

    public void hline(int xl, int xr, int y, int c)
    {

        if (dither)
        {
            for (int i = xl; i <= xr; i++)
            {
                pset(i, y, c);
            }
        }
        else
        {
            memset(((ulong)y * (ulong)Width) + (ulong)xl, (ulong)(xr - xl + 1), wrapInt(c));
        }
    }
    delegate void linePixelFunc(int x, int y, int c);

    public void line(int x0, int y0, int x1, int y1, int c)
    {
        _line(x0, y0, x1, y1, c, pset);
    }
    public void line(Vector2 v1, Vector2 v2, int color)
    {
        _line(
            Mathf.RoundToInt(v1.x),
            Mathf.RoundToInt(v1.y),
            Mathf.RoundToInt(v2.x),
            Mathf.RoundToInt(v2.y),
            color,
            pset
        );
    }
    private void _line(int x0, int y0, int x1, int y1, int c, linePixelFunc func)
    {
        byte _c = wrapInt(c);

        if (y0 == y1)
        {
            if (x0 == x1)
            {
                func(x0, y0, _c);
                return;
            }
            if (func == pset)
            {
                if (x0 > x1)
                {
                    int tmp = x0;
                    x0 = x1;
                    x1 = tmp;
                }
                if (
                    y0 >= (int)clipRect.t && y0 < (int)clipRect.b &&
                    (x0 >= (int)clipRect.l || x1 >= (int)clipRect.l || x0 < (int)clipRect.r || x1 < (int)clipRect.r)
                )
                {
                    x0 = Mathf.Min(Mathf.Max(x0, clipRect.l), (int)clipRect.r - 1);
                    x1 = Mathf.Min(Mathf.Max(x1, clipRect.l), (int)clipRect.r - 1);
                    hline(x0, x1, y0, _c);
                }

                return;
            }
        }

        var dx = Mathf.Abs(x1 - x0);
        var dy = Mathf.Abs(y1 - y0);
        var sx = (x0 < x1) ? 1 : -1;
        var sy = (y0 < y1) ? 1 : -1;
        var err = dx - dy;
        while (true)
        {
            func(x0, y0, _c);

            if ((x0 == x1) && (y0 == y1)) break;
            var e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
        screenChanged = true;
    }

    public void spr(uint n, int x, int y, uint scaleX = 1, uint scaleY = 1, uint w = 1, uint h = 1)
    {
        int srcx = (int)n % (spritesheetIndexed.width / 8) * 8;
        int srcy = (int)n / (spritesheetIndexed.height / 8) * 8;
        for (int _y = 0; _y < (8 * h * scaleY); _y++)
        {
            for (int _x = 0; _x < (8 * w * scaleX); _x++)
            {
                psetFromSprite((srcx + _x) / (int)scaleX, (srcy + _y) / (int)scaleY, x + _x, y + _y);
            }
        }
    }
    public void spr(uint n, Vector2 v, uint scaleX = 1, uint scaleY = 1, uint w = 1, uint h = 1)
    {
        spr(
             n,
             Mathf.RoundToInt(v.x),
             Mathf.RoundToInt(v.y),
             scaleX,
             scaleY,
             w,
             h
         );
    }


    void psetFromSprite(int srcx, int srcy, int destx, int desty)
    {
        if (dither && Random.value > chance)
        {
            return;
        }
        if (destx < clipRect.l || desty < clipRect.t || destx >= clipRect.r || desty >= clipRect.b)
        {
            return;
        };
        if (srcx < 0 || srcx >= spritesheet.width || srcy < 0 || srcy >= spritesheet.height)
        {
            return;
        }

        screenData[(int)((desty * Width) + destx)] = spritesheetIndexedData[(int)((srcy * spritesheet.width) + srcx)];
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
