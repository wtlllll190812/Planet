Shader "Custom/planet"
{
    Properties
    {
        [HDR]_BaseColor("BaseColor",Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {"RenderPipeline" = "UniversalRenderPipeline" "RenderType" = "Opaque"}

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)         
            half4 _BaseColor;
        CBUFFER_END

        struct a2v
        {
            float4 vertex:POSITION;
        };
        struct v2f
        {
            float4 pos:SV_POSITION;
        };

        v2f vert(a2v v)
        {
            v2f o;
            o.pos = TransformObjectToHClip(v.vertex);
            return o;
        }

        ENDHLSL
        pass
        {
            Tags{ "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            half4 frag(v2f i) :SV_TARGET
            {
                return _BaseColor;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}