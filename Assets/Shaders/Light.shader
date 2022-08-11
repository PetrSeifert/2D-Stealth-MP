Shader "Unlit/Light"
{
    Properties
    {
        _MainColor ("Color", Color) = (0, 0, 0, 0)
        _Multiplier ("Multiplier", Range(0.0, 10.0)) = 1
        _Size ("Size", Range(0.0, 1.0)) = 0
        //_b ("b", Range(0.0, 5.0)) = 1
        _Softness ("Softness", Range(1.0, 10.0)) = 1
        _Pixelation ("Pixelation", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Opaque" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _MainColor;
            float _Multiplier;
            float _Size;
            //float _b;
            float _Softness;
            float _Pixelation;

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

            float inverseLerp(float a, float b, float v) { return (v-a)/(b-a); }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float alpha;
                if(_Pixelation != 0)
                {
                    float2 roundedUV = float2(ceil(i.uv.x * _Pixelation) / _Pixelation, ceil(i.uv.y * _Pixelation) / _Pixelation);
                    alpha = 1 - length(roundedUV * 2 - 1);
                }
                else alpha = 1 - length(i.uv * 2 - 1);

                return float4(_MainColor.xyz, saturate(inverseLerp(pow(1 - _Size, _Softness), 1, pow(alpha, _Softness)) * _Multiplier));
            }
            ENDCG
        }
    }
}
