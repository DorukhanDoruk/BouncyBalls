Shader "BouncyBalls/Toon"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _ShadeColor ("Shade Color", Color) = (0.5, 0.5, 0.5, 1)

        [Header(Height Gradient)]
        _GradientScale ("Gradient Scale", Float) = 2
        _GradientBias ("Gradient Bias", Float) = 0.5
        _LightInfluence ("Light Influence", Range(0, 1)) = 0.25

        [Header(Light Bands)]
        _Steps ("Shade Steps", Range(1, 6)) = 3
        _BandSoftness ("Band Softness", Range(0, 0.5)) = 0.5
        _ShadowStrength ("Cast Shadow Strength", Range(0, 1)) = 0.45

        [Header(Specular)]
        _SpecColor ("Specular Color", Color) = (1, 1, 1, 1)
        _SpecSize ("Specular Size", Range(0, 1)) = 0.25
        _SpecSoftness ("Specular Softness", Range(0, 0.5)) = 0.15
        _SpecStrength ("Specular Strength", Range(0, 1)) = 0.15

        [Header(Rim)]
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 4
        _RimStrength ("Rim Strength", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _ShadeColor;
            float _GradientScale;
            float _GradientBias;
            float _LightInfluence;
            float _Steps;
            float _BandSoftness;
            float _ShadowStrength;
            float4 _SpecColor;
            float _SpecSize;
            float _SpecSoftness;
            float _SpecStrength;
            float4 _RimColor;
            float _RimPower;
            float _RimStrength;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float  heightOS   : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.heightOS = input.positionOS.y;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 normalWS = normalize(input.normalWS);
                float3 viewWS = normalize(GetWorldSpaceViewDir(input.positionWS));

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);

                // Drives the shading. A cylinder side has constant N.L along its height,
                // so a top-to-bottom falloff can only come from object space.
                float heightT = saturate(input.heightOS * _GradientScale + _GradientBias);

                // Smoothed bands: 0 softness gives hard toon steps, 0.5 an even ramp.
                float steps = max(1.0, floor(_Steps));
                float scaled = saturate(dot(normalWS, mainLight.direction)) * steps;
                float index = floor(scaled);
                float edge = smoothstep(0.5 - _BandSoftness, 0.5 + _BandSoftness, scaled - index);
                float lightT = saturate((index + edge) / steps);

                float3 albedo = lerp(_ShadeColor.rgb, _BaseColor.rgb, lerp(heightT, lightT, _LightInfluence));

                // Cast shadows darken on top of the gradient rather than feeding into it,
                // so a shadowed disc keeps its own top-to-bottom shape.
                albedo *= lerp(1.0 - _ShadowStrength, 1.0, mainLight.shadowAttenuation);

                float3 halfDir = normalize(mainLight.direction + viewWS);
                float specPower = exp2(lerp(11.0, 1.0, _SpecSize));
                float spec = pow(saturate(dot(normalWS, halfDir)), specPower);
                spec = smoothstep(0.5 - _SpecSoftness, 0.5 + _SpecSoftness, spec);

                float rim = pow(1.0 - saturate(dot(normalWS, viewWS)), _RimPower);

                float3 color = albedo
                    + _SpecColor.rgb * (spec * _SpecStrength * mainLight.shadowAttenuation)
                    + _RimColor.rgb * (rim * _RimStrength);

                return half4(color, _BaseColor.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex shadowVert
            #pragma fragment shadowFrag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            // URP sets this per shadow-casting light.
            float3 _LightDirection;

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            ShadowVaryings shadowVert(ShadowAttributes input)
            {
                ShadowVaryings output = (ShadowVaryings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                output.positionCS = positionCS;
                return output;
            }

            half4 shadowFrag(ShadowVaryings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma vertex depthVert
            #pragma fragment depthFrag
            #pragma multi_compile_instancing

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            DepthVaryings depthVert(DepthAttributes input)
            {
                DepthVaryings output = (DepthVaryings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 depthFrag(DepthVaryings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
