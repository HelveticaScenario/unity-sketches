Shader "Custom/Tess" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
       
        _Height ("Height", 2D) = "black"{}
        _HeightmapTiling ("Heightmap tiling", Float) = 1.0
        _Tess ("Tessellation", Range(1,32)) = 4
        _Displacement ("Displacement", Range(0, 10.0)) = 0.3
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
       
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:disp tessellate:tessDistance
 
        #pragma target 5.0
        #include "Tessellation.cginc"
 
        struct appdata {
        float4 vertex : POSITION;
        float4 tangent : TANGENT;
        float3 normal : NORMAL;
        float2 texcoord : TEXCOORD0;
        float2 texcoord1 : TEXCOORD1;
        float2 texcoord2 : TEXCOORD2;
        };
 
            float _Tess;
            sampler2D _Height;
            float _HeightmapTiling;
 
            float4 tessDistance (appdata v0, appdata v1, appdata v2) {
                float minDist = .25;
                float maxDist = 15.0;
                return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
            }
           
            float _Displacement;
 
            void disp (inout appdata v)
            {
                float d = tex2Dlod( _Height , float4(v.texcoord.xy * _HeightmapTiling,0,0)).a * _Displacement;
                v.vertex.xyz += v.normal * d;
            }
 
        sampler2D _MainTex;
 
        struct Input {
            float2 uv_MainTex;
        };
 
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
 
        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
 