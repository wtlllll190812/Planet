Shader "Custom/sun"
{
    Properties
    {
        _MainTex("MainTex",2D) = "White"{}
        [HDR]_FirColor("FirColor",Color) = (1,1,1,1)
        _FresnelPow("FresnelPow",Float)=0
    }
    SubShader
    {
        Cull off
        Tags
        {"RenderPipeline" = "UniversalRenderPipeline" "RenderType" = "Opaque"}

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)         
            float4 _MainTex_ST;
            half4 _FirColor;
            float _FresnelPow;
        CBUFFER_END
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);


        struct a2v
        {
            float4 vertex:POSITION;
            float2 uv:TEXCOORD;
            float3 normal:NORMAL;
        };
        struct v2f
        {
            float4 pos:SV_POSITION;
            float2 uv:TEXCOORD;
            float3 worldNormal:TEXCOORD1;
            float3 worldPos:TEXCOORD2;
        };

        v2f vert(a2v v)
        {
            v2f o;
            o.pos = TransformObjectToHClip(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv,_MainTex);
            o.worldNormal=TransformObjectToWorldNormal(v.normal);
            o.worldPos=mul(unity_ObjectToWorld, v.vertex).xyz;
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
                half3 worldNormal=normalize(i.worldNormal);
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
                float dotValue = pow(1 - saturate(dot(worldNormal,viewDir)),_FresnelPow);
                
                half4 texColor=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                half4 sunColor=lerp(texColor,_FirColor,pow(dot(texColor,_FirColor),2));
                half4 finalColor=lerp(sunColor,_FirColor,dotValue);
                return finalColor;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}