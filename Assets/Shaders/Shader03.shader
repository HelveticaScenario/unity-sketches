Shader "Custom/Shader03"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Screen", 2D) = "white" {}
        _PaletteTex ("Palette", 2D) = "black" {}
        _SwapTex ("Swap", 2D) = "black" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _PaletteTex;
        sampler2D _SwapTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            
            // precision mediump float;
            // precision mediump int;
            
            // attribute vec2 a_position;
            // attribute vec2 a_texCoord;
            
            // varying vec2 v_texCoord;
            // void main() {
            //       v_texCoord = a_texCoord;
            //   gl_Position = vec4(a_position,0, 1);
            // }
            // fixed4 index = tex2D(u_screen, v_texCoord);
          //Do a dependency texture read
            // Albedo comes from a texture tinted by color
            fixed index = tex2D (_MainTex, IN.uv_MainTex);
            fixed iii = tex2D (_SwapTex, float2(index, 0));
            fixed4 c = tex2D(_PaletteTex, float2(iii, 0));
            // o.Albedo = c.rgb;
            // o.Albedo = 1; //index.rgb;//float3(IN.uv_MainTex, 0);
            // fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
