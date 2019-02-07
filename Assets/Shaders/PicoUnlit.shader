Shader "Custom/PicoUnlit"
{
    Properties
    {
        _MainTex ("Screen", 2D) = "white" {}
        _PaletteTex ("Palette", 2D) = "black" {}
        _SwapTex ("Swap", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _PaletteTex;
            float4 _PaletteTex_ST;
            sampler2D _SwapTex;
            float4 _SwapTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed index = tex2D (_MainTex, float2(i.uv.x, (i.uv.y - 1) * -1));
                fixed iii = tex2D (_SwapTex, float2(index, 0));
                fixed4 col = tex2D(_PaletteTex, float2(iii, 0));
                // 1/256 ~= 0.0039
                clip( col.a < 0.0039f ? -1:1 );
                // apply fog
                // UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
