Shader "Custom/GhostEyesURP"
{
    Properties
    {
        _EyeColor ("Eye Color", Color) = (0.02, 0.02, 0.02, 1.0)
        _EyeSpacing ("Eye Spacing", Range(0.0, 0.5)) = 0.25
        _EyeWidth ("Eye Width", Range(0.01, 0.5)) = 0.15
        _EyeHeight ("Eye Height", Range(0.01, 0.5)) = 0.1
        _Softness ("Edge Softness", Range(0.0, 0.3)) = 0.05
        _BlinkInterval ("Blink Interval", Range(1, 10)) = 4.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent+200" 
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
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
            
            CBUFFER_START(UnityPerMaterial)
                half4 _EyeColor;
                half _EyeSpacing;
                half _EyeWidth;
                half _EyeHeight;
                half _Softness;
                half _BlinkInterval;
            CBUFFER_END
            
            float getBlinkFactor(float time)
            {
                float cycle = fmod(time, _BlinkInterval);
                float blinkDuration = 0.15;
                float blinkStart = _BlinkInterval - blinkDuration;
                
                if (cycle > blinkStart)
                {
                    float t = (cycle - blinkStart) / blinkDuration;
                    return 1.0 - sin(t * 3.14159);
                }
                return 1.0;
            }
            
            float sdEllipse(float2 p, float2 radii)
            {
                float2 pp = p / radii;
                return (length(pp) - 1.0) * min(radii.x, radii.y);
            }
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                float3 worldPos = unity_ObjectToWorld._m03_m13_m23;
                float3 camRight = UNITY_MATRIX_V[0].xyz;
                float3 camUp = UNITY_MATRIX_V[1].xyz;
                
                float scaleX = length(unity_ObjectToWorld._m00_m10_m20);
                float scaleY = length(unity_ObjectToWorld._m01_m11_m21);
                
                float3 offset = IN.positionOS.x * camRight * scaleX 
                              + IN.positionOS.y * camUp * scaleY;
                
                OUT.positionHCS = TransformWorldToHClip(worldPos + offset);
                OUT.uv = IN.uv;
                
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                float blink = getBlinkFactor(_Time.y);
                
                float2 uv = IN.uv - 0.5;
                
                float2 leftCenter = float2(-_EyeSpacing, 0.0);
                float2 rightCenter = float2(_EyeSpacing, 0.0);
                
                float2 eyeRadii = float2(_EyeWidth, _EyeHeight * max(0.01, blink));
                
                float dLeft = sdEllipse(uv - leftCenter, eyeRadii);
                float dRight = sdEllipse(uv - rightCenter, eyeRadii);
                float d = min(dLeft, dRight);
                float alpha = 1.0 - smoothstep(-_Softness, _Softness, d);
                
                alpha *= step(0.01, blink);
                
                return half4(_EyeColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}