Shader "Custom/planet"
{
    Properties
    {
        _MainTex("MainTex",2D)="white"{}
        _BaseColor("BaseColor",Color) = (1,1,1,1)
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
            float4 _MainTex_ST;
        CBUFFER_END
        
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        struct a2v
        {
            float4 vertex:POSITION;
            float3 normal:NORMAL;
            float2 uv:TEXCOORD;
        };
        struct v2f
        {
            float4 pos:SV_POSITION;
            float3 worldNormal:TEXCOORD1;
            float3 worldPos:TEXCOORD2;
            float2 uv:TEXCOORD3;
        };

        v2f vert(a2v v)
        {
            v2f o;
            o.pos = TransformObjectToHClip(v.vertex);
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.worldNormal=TransformObjectToWorldNormal(v.normal);
            o.uv = TRANSFORM_TEX(v.uv,_MainTex);
            return o;
        }

        ENDHLSL
        pass
        {
            Tags{ "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS //主光源阴影
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE //主光源层级阴影是否开启
            #pragma multi_compile _ _SHADOWS_SOFT //软阴影


            half4 frag(v2f i) :SV_TARGET
            {
                half4 color=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*_BaseColor;

                half3 ambient = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
                float4 SHADOW_COORDS = TransformWorldToShadowCoord(i.worldPos);
                Light mainLight = GetMainLight(SHADOW_COORDS);
                half shadow = mainLight.shadowAttenuation;
                
                half3 lightDir=normalize(_MainLightPosition.xyz);
                half3 worldNormal=normalize(i.worldNormal);
                half lambert=saturate(dot(worldNormal,lightDir)*0.5+0.5)*shadow;

                return color*lambert+half4(ambient,1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}