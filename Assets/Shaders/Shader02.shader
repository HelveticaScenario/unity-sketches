Shader "Unlit/Shader02" {
    SubShader {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf Standard
        struct Input {
            float4 color : COLOR;
            float3 viewDir;
            float4 screenPos;
            float3 worldPos;
            float3 worldRefl;
            float3 worldNormal;
        };
        void surf (Input IN, inout SurfaceOutputStandard o) {
            o.Albedo = IN.worldPos; // 1 = (1,1,1,1) = white
        }
        ENDCG
    }
    Fallback "Diffuse"
}