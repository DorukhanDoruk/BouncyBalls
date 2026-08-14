// Inverted hull as its own material, not a second pass on the lit shader:
// URP gathers UniversalForward and SRPDefaultUnlit into one call, so two passes in one shader
// would force a per object draw order and break batching.
Shader "BouncyBalls/InstancedOutline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth("Outline Width", Range(0, 0.2)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "UniversalForward" }

            Cull Front

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            half4 _OutlineColor;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _OutlineWidth)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 Vert(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float width = UNITY_ACCESS_INSTANCED_PROP(Props, _OutlineWidth);
                return TransformObjectToHClip(input.positionOS.xyz + input.normalOS * width);
            }

            half4 Frag() : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
