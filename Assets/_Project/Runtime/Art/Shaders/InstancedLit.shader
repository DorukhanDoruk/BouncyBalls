// Per-instance colour so one material can draw every disc and ball in one batch.
// Not SRP Batcher compatible by design: instanced properties and the batcher are mutually exclusive.
Shader "BouncyBalls/InstancedLit"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)

        [Header(Shading)]
        _ShadeStrength("Shade Strength", Range(0, 1)) = 0.35
        _SpecTint("Specular Tint", Range(0, 1)) = 0.45
        _SpecPower("Specular Power", Range(1, 128)) = 32
        _SpecStrength("Specular Strength", Range(0, 1)) = 0.35

        [Header(Outline)]
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth("Outline Width", Range(0, 0.1)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        half _ShadeStrength;
        half _SpecTint;
        half _SpecPower;
        half _SpecStrength;
        half4 _OutlineColor;
        float _OutlineWidth;

        // MaterialPropertyBlock writes land here, one value per instance.
        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
        UNITY_INSTANCING_BUFFER_END(Props)
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 baseColor = UNITY_ACCESS_INSTANCED_PROP(Props, _BaseColor);

                float3 normalWS = normalize(input.normalWS);
                float3 viewWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));

                half lightT = saturate(dot(normalWS, mainLight.direction)) * mainLight.shadowAttenuation;
                half3 albedo = lerp(baseColor.rgb * _ShadeStrength, baseColor.rgb, lightT);

                half3 halfDir = normalize(mainLight.direction + viewWS);
                half specular = pow(saturate(dot(normalWS, halfDir)), _SpecPower) * _SpecStrength * lightT;
                half3 specularColor = lerp(baseColor.rgb, 1.0h, _SpecTint);

                return half4(albedo * mainLight.color + specularColor * specular, baseColor.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front

            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag

            #pragma multi_compile_instancing

            struct OutlineAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 OutlineVert(OutlineAttributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return TransformObjectToHClip(input.positionOS.xyz + input.normalOS * _OutlineWidth);
            }

            half4 OutlineFrag() : SV_Target
            {
                return _OutlineColor;
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

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 ShadowVert(ShadowAttributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }

            half4 ShadowFrag() : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
