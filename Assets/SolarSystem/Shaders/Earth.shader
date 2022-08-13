Shader "Custom/Earth"
{
    Properties
    {
        _MainTex("MainTex",2D) = "White"{}
        [HDR]_BaseColor("BaseColor",Color) = (1,1,1,1)
        _FresnelPow("FresnelPow",Float)=0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)         
            float4 _MainTex_ST;
            half3 _BaseColor;
            float _FresnelPow;
        CBUFFER_END
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        ENDHLSL
        pass
        {
            Tags{"LightMode"="UniversalForward"} 
            HLSLPROGRAM
            #pragma fragment frag
            #pragma vertex vert

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS //主光源阴影
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE //主光源层级阴影是否开启
            #pragma multi_compile _ _SHADOWS_SOFT //软阴影

            struct a2v
            {
                float4 vertex:POSITION;
                float3 normal:NORMAL;
                float2 uv:TEXCOORD;
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
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal=TransformObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                return o;
            }
            half4 frag(v2f i) :SV_TARGET
            {
                //shadow
                float4 SHADOW_COORDS = TransformWorldToShadowCoord(i.worldPos);
                Light mainLight = GetMainLight(SHADOW_COORDS);
                half shadow = mainLight.shadowAttenuation;

                //Light
                half3 texColor=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                half3 lightDir=normalize(_MainLightPosition.xyz);
                half3 worldNormal=normalize(i.worldNormal);
                half lambert=saturate(dot(worldNormal,lightDir));
                half3 mainCol=_BaseColor*lambert;
                
                //AddLight
                half3 addColor;
                int addLightsCount = GetAdditionalLightsCount();//定义在lighting库函数的方法 返回一个额外灯光的数量
                for (int idx = 0; idx < addLightsCount; idx++)
                {
                    Light addlight = GetAdditionalLight(idx, i.worldPos);//定义在lightling库里的方法 返回一个灯光类型的数据
                    lambert=saturate(dot(worldNormal,normalize(addlight.direction)*0.5f)+0.5f);
                    addColor.rgb += lambert*addlight.color * texColor.rgb * addlight.distanceAttenuation * addlight.shadowAttenuation;
                }

                //Fresnel
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
                float dotValue = pow(1 - saturate(dot(worldNormal,viewDir)),_FresnelPow);

                half3 finalColor=addColor+lerp(0,_BaseColor,dotValue);
                return half4(finalColor,1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}