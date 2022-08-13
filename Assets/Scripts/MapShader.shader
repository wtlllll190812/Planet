Shader "Custom/MapShader"
{
    Properties
    {
        _MapTex ("MapTex", 2D) = "white" {}
        
        _MoutainColor("MoutainColor",Color)=(1,1,1,1)
        _ForestColor("ForestColor",Color)=(1,1,1,1)
        _GrassColor("GrassColor",Color)=(1,1,1,1)
        _OceanColor("OceanColor",Color)=(1,1,1,1)
        _SandColor("SandColor",Color)=(1,1,1,1)

        _ForestThreshold("ForestThreshold",Float)=0.3
        _oceanThreshold("oceanThreshold",Float)=0.6
        _MoutainThreshold("MoutainThreshold",Float)=0.9
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
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv[9] : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MapTex;
            float4 _MapTex_ST;
            float4 _MapTex_TexelSize;

            float4 _MoutainColor;
            float4 _ForestColor;
            float4 _GrassColor;
            float4 _OceanColor;
            float4 _SandColor;

            float _ForestThreshold;
            float _oceanThreshold;
            float _MoutainThreshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv[0] = v.uv+_MapTex_TexelSize.xy*half2(0,0);
                o.uv[1] = v.uv+_MapTex_TexelSize.xy*half2(1,0);
                o.uv[2] = v.uv+_MapTex_TexelSize.xy*half2(1,1);
                o.uv[3] = v.uv+_MapTex_TexelSize.xy*half2(0,1);
                o.uv[4] = v.uv+_MapTex_TexelSize.xy*half2(-1,1);
                o.uv[5] = v.uv+_MapTex_TexelSize.xy*half2(-1,0);
                o.uv[6] = v.uv+_MapTex_TexelSize.xy*half2(-1,-1);
                o.uv[7] = v.uv+_MapTex_TexelSize.xy*half2(0,-1);
                o.uv[8] = v.uv+_MapTex_TexelSize.xy*half2(1,-1);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float noise = tex2D(_MapTex, i.uv[0]).r;
                if(noise>_MoutainThreshold)
                    return _MoutainColor;
                if(noise>_ForestThreshold)
                    return _ForestColor;
                if(noise>_oceanThreshold)
                {
                    int number=0;
                    for(int index=0;index<9;index++)
                    {
                        float s=tex2D(_MapTex, i.uv[index]).r;
                        if(s<_oceanThreshold)
                            number++;
                    }
                    if(number!=0)
                        return _SandColor;
                    return _GrassColor;
                }
                return _OceanColor;
            }
            ENDCG
        }
    }
}
