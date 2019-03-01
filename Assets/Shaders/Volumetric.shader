// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Volumetric"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Center ("Center", Vector) = (0,0,0,0)
        _Color ("Color", Color) = (1,1,1,1)
        _Radius ("Radius", Float) = 1
        _Steps ("Steps", Int) = 64
        _MinDistance ("Min Distance", Float) = 0.01
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
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            #define STEPS 64
            #define MIN_DISTANCE 0.001

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 wPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float3 _Center;
            float _Radius;
            float _MinDistance;
            int _Steps;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz; 
                // UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            bool sphereHit (float3 p)
            {
                return distance(p,_Center) < _Radius;
            }

            float map(float3 p)
            {
                return distance(p,_Center) - _Radius;
            }
           

            fixed4 simpleLambert (fixed3 normal) {
                fixed3 lightDir = _WorldSpaceLightPos0.xyz;	// Light direction
                fixed3 lightCol = _LightColor0.rgb;		// Light color

                fixed NdotL = max(dot(normal, lightDir),0);
                fixed4 c;
                c.rgb = _Color * lightCol * NdotL;
                c.a = 1;
                return c;
            }


            float3 normal (float3 p)
            {
                const float eps = 0.01;

                return normalize
                (	float3
                    (	map(p + float3(eps, 0, 0)	) - map(p - float3(eps, 0, 0)),
                        map(p + float3(0, eps, 0)	) - map(p - float3(0, eps, 0)),
                        map(p + float3(0, 0, eps)	) - map(p - float3(0, 0, eps))
                    )
                );
            }

            fixed4 renderSurface(float3 p)
            {
                float3 n = normal(p);
                return simpleLambert(n);
            }

            fixed4 raymarch (float3 position, float3 direction)
            {
                for (int i = 0; i < _Steps; i++)
                {
                    float distance = map(position);
                    if (distance < _MinDistance)
                        return renderSurface(position);

                    position += distance * direction;
                }
                return fixed4(1,1,1,1);
            }

            

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float3 worldPosition = i.wPos;
                float3 viewDirection = normalize(i.wPos - _WorldSpaceCameraPos);
                return raymarch(worldPosition, viewDirection);
            }
            ENDCG
        }
    }
}
