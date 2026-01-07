Shader "Custom/WaveDistortion"
{
    Properties
    {
        _WaveCenter ("Wave Center (Screen UV)", Vector) = (0.5, 0.5, 0, 0)
        _WaveRadius ("Wave Radius", Float) = 0.5
        _WaveThickness ("Wave Thickness", Float) = 0.1
        _WaveStrength ("Distortion Strength", Float) = 0.05
        _WaveNoise ("Noise Amount", Float) = 0.02
        _WaveNoiseScale ("Noise Scale", Float) = 20.0
        _WaveFalloff ("Edge Falloff", Float) = 2.0
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "WaveDistortion"
            ZTest Always ZWrite Off Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4 _WaveCenter;
            float _WaveRadius;
            float _WaveThickness;
            float _WaveStrength;
            float _WaveNoise;
            float _WaveNoiseScale;
            float _WaveFalloff;

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float2 center = _WaveCenter.xy;

                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 uvCorrected = float2((uv.x - 0.5) * aspect, uv.y - 0.5);
                float2 centerCorrected = float2((center.x - 0.5) * aspect, center.y - 0.5);

                float2 delta = uvCorrected - centerCorrected;
                float dist = length(delta);
                float2 dir = normalize(delta + 0.0001);

                float ringDist = abs(dist - _WaveRadius);
                float ringMask = 1.0 - saturate(ringDist / (_WaveThickness * 0.5));
                ringMask = pow(ringMask, _WaveFalloff);

                float n = noise(uv * _WaveNoiseScale + _Time.y * 2.0);
                float noiseOffset = (n - 0.5) * 2.0 * _WaveNoise;

                float2 distortion = dir * ringMask * (_WaveStrength + noiseOffset * ringMask);
                float2 distortedUV = uv - distortion;

                distortedUV = saturate(distortedUV);

                return SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, distortedUV);
            }
            ENDHLSL
        }
    }
}