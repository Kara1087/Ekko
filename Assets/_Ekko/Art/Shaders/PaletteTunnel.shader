Shader "Unlit/PaletteTunnelURP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // not used, but keeps inspector clean
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Pass
        {
            Name "UnlitPass"
            Tags { "LightMode"="UniversalForward" }

            ZWrite Off
            Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // --------------------------------------------------
            // Shader structures
            // --------------------------------------------------
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // --------------------------------------------------
            // Vertex
            // --------------------------------------------------
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            // --------------------------------------------------
            // Palette function
            // --------------------------------------------------
            float3 palette(float t)
            {
                float3 a = float3(0.5, 0.5, 0.5);
                float3 b = float3(0.5, 0.5, 0.5);
                float3 c = float3(1.0, 1.0, 1.0);
                float3 d = float3(0.263, 0.416, 0.557);

                return a + b * cos(6.28318 * (c * t + d));
            }

            // --------------------------------------------------
            // Fragment
            // --------------------------------------------------
            float4 frag(Varyings IN) : SV_Target
            {
                float2 resolution = _ScreenParams.xy;
                float2 fragCoord = IN.uv * resolution;

                // normalize uv like in Shadertoy
                float2 uv = (fragCoord * 2.0 - resolution.xy) / resolution.y;
                float2 uv0 = uv;
                float3 finalColor = float3(0.0, 0.0, 0.0);

                float time = _Time.y; // Unity’s built-in time (seconds)

                [unroll]
                for (int i = 0; i < 4; i++)
                {
                    uv = frac(uv * 1.5) - 0.5;

                    float d = length(uv) * exp(-length(uv0));

                    float3 col = palette(length(uv0) + i * 0.4 + time * 0.4);

                    d = sin(d * 8.0 + time) / 8.0;
                    d = abs(d);

                    d = pow(0.01 / d, 1.2);

                    finalColor += col * d;
                }

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
