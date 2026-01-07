Shader "Custom/GhostElasticURP"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0.5, 0.8, 1.0, 1.0)
        _Intensity ("Intensity", Range(0, 10)) = 2.0
        _Steps ("Ray Steps", Range(8, 64)) = 32
        
        [Header(Core)]
        _CoreRadius ("Core Radius", Range(0.05, 1.0)) = 0.2
        _CoreSoftness ("Core Softness", Range(0.01, 0.5)) = 0.1
        _CoreBrightness ("Core Brightness", Range(1, 10)) = 3.0
        
        [Header(Aura)]
        _AuraRadius ("Aura Radius", Range(0.1, 2.0)) = 0.5
        _AuraSoftness ("Aura Softness", Range(0.01, 1.0)) = 0.3
        _AuraDensity ("Aura Density", Range(0, 2)) = 0.5
        _NoiseScale ("Noise Scale", Range(0.5, 10.0)) = 3.0
        _NoiseSpeed ("Noise Speed", Range(0, 2)) = 0.5
        _NoiseStrength ("Noise Strength", Range(0, 1.0)) = 0.4
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1.0
        _PulseStrength ("Pulse Strength", Range(0, 0.3)) = 0.1
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent+100" 
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Name "GhostBody"
            Blend One One
            ZWrite Off
            Cull Front
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                float3 camPosOS : TEXCOORD1;
            };
            
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Intensity;
                int _Steps;
                half _CoreRadius;
                half _CoreSoftness;
                half _CoreBrightness;
                half _AuraRadius;
                half _AuraSoftness;
                half _AuraDensity;
                half _NoiseScale;
                half _NoiseSpeed;
                half _NoiseStrength;
                half _PulseSpeed;
                half _PulseStrength;
                float4 _Deform;
                float4 _Velocity;
            CBUFFER_END
            
            float hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }
            
            float noise(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = hash(i);
                float b = hash(i + float3(1, 0, 0));
                float c = hash(i + float3(0, 1, 0));
                float d = hash(i + float3(1, 1, 0));
                float e = hash(i + float3(0, 0, 1));
                float g = hash(i + float3(1, 0, 1));
                float h = hash(i + float3(0, 1, 1));
                float k = hash(i + float3(1, 1, 1));
                
                return lerp(
                    lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y),
                    lerp(lerp(e, g, f.x), lerp(h, k, f.x), f.y),
                    f.z
                );
            }
            
            float fbm(float3 p)
            {
                float value = 0.0;
                float amp = 0.5;
                for (int j = 0; j < 4; j++)
                {
                    value += amp * noise(p);
                    p *= 2.0;
                    amp *= 0.5;
                }
                return value;
            }
            
            float3 deformPoint(float3 p)
            {
                float3 scale = float3(
                    1.0 / (1.0 + _Deform.x),
                    1.0 / (1.0 + _Deform.y),
                    1.0 / (1.0 + _Deform.x)
                );
                return p * scale;
            }
            
            float coreSDF(float3 p, float time)
            {
                float3 pDef = deformPoint(p);
                float d = length(pDef) - _CoreRadius;
                d -= sin(time * _PulseSpeed) * _PulseStrength;
                return d;
            }
            
            float auraSDF(float3 p, float time)
            {
                float3 pDef = deformPoint(p);
                float dist = length(pDef);
                float d = dist - _AuraRadius;
                
                float3 noiseCoord = p * _NoiseScale;
                float angle = time * _NoiseSpeed * 0.3;
                float cosA = cos(angle);
                float sinA = sin(angle);
                noiseCoord = float3(
                    noiseCoord.x * cosA - noiseCoord.z * sinA,
                    noiseCoord.y,
                    noiseCoord.x * sinA + noiseCoord.z * cosA
                );
                
                float n = fbm(noiseCoord);
                d += (n - 0.5) * _NoiseStrength;
                d -= sin(time * _PulseSpeed * 0.7) * _PulseStrength * 0.5;
                
                float velMag = length(_Velocity.xyz);
                if (velMag > 0.01)
                {
                    float wobble = (fbm(p * 5.0 + time) - 0.5) * velMag * 0.03;
                    d += wobble;
                }
                
                float jiggle = (fbm(p * 8.0 + time * 3.0) - 0.5) * abs(_Deform.y) * 0.15;
                d += jiggle;
                
                return d;
            }
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionOS = IN.positionOS.xyz;
                OUT.camPosOS = TransformWorldToObject(GetCameraPositionWS());
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                float3 rayDir = normalize(IN.positionOS - IN.camPosOS);
                float time = _Time.y;
                
                float3 rayPos = IN.positionOS;
                float stepSize = 2.0 / (float)_Steps;
                
                float coreDensity = 0.0;
                float auraDensity = 0.0;
                
                for (int i = 0; i < _Steps; i++)
                {
                    float dCore = coreSDF(rayPos, time);
                    float coreContrib = 1.0 - saturate(dCore / _CoreSoftness);
                    coreContrib *= coreContrib;
                    coreDensity += coreContrib * stepSize;
                    
                    float dAura = auraSDF(rayPos, time);
                    float auraContrib = 1.0 - saturate(dAura / _AuraSoftness);
                    auraContrib *= auraContrib;
                    auraDensity += auraContrib * stepSize * _AuraDensity;
                    
                    rayPos -= rayDir * stepSize;
                }
                
                coreDensity = saturate(coreDensity);
                auraDensity = saturate(auraDensity);
                
                half3 coreCol = _Color.rgb * coreDensity * _CoreBrightness * _Intensity;
                half3 auraCol = _Color.rgb * auraDensity * _Intensity * 0.6;
                half3 finalCol = coreCol + auraCol * (1.0 - coreDensity * 0.5);
                
                float finalAlpha = saturate(coreDensity + auraDensity);
                
                return half4(finalCol, finalAlpha);
            }
            ENDHLSL
        }
    }
}