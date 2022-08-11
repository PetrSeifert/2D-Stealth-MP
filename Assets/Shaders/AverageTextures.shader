Shader "Unlit/AverageTextures"
{
    Properties
    {
        _Textures ("Texture Array", 2DArray) = "" {}
        _TexturesCount ("Texture Count", Int) = 1

    }
    SubShader
    {
        //Tags { "Queue"="Transparent" "RenderType"="Opaque" }
        //Blend SrcAlpha OneMinusSrcAlpha
        //Cull off //double sided
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma require 2darray

            #include "UnityCG.cginc"
            
            UNITY_DECLARE_TEX2DARRAY(_Textures);
            int _TexturesCount;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD01;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(UNITY_MATRIX_M, float4(v.vertex.xyz, 1));
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 result = float4(0,0,0,0);
                for(int j = 0; j < _TexturesCount; j++)
                {
                    result += float4(UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(i.worldPos.xy, j)) / _TexturesCount);
                }
                return result;
            }

            ENDCG
        }
    }
}
