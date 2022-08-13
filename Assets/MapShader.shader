Shader "Unlit/MapShader"
{
    Properties
    {
        _MapTex ("MapTex", 2D) = "white" {}
        _Color1("Color1",Color)=(1,1,1,1)
        _Color2("Color2",Color)=(1,1,1,1)
        _Color3("Color3",Color)=(1,1,1,1)
        _Threshold1("Threshold1",Float)=0.3
        _Threshold2("Threshold2",Float)=0.6
    }	
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MapTex;
            float4 _MapTex_ST;
            float4 _Color1;
            float4 _Color2;
            float4 _Color3;

            float _Threshold1;
            float _Threshold2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MapTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float noise = tex2D(_MapTex, i.uv).r;
                if(noise>_Threshold1)
                    return _Color1;
                if(noise>_Threshold2)
                    return _Color2;
                return _Color3;
            }
            ENDCG
        }
    }
}
