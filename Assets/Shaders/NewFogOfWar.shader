Shader "Unlit/NewFogOfWar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DataTex ("Alpha", 2D) = "white" {}
        [ShowAsVector2] _CamPosition("Camera Position", Vector) = (0, 0, 0, 0)
        [ShowAsVector2] _Deviation("Camera Position", Vector) = (0, 0, 0, 0)
        //_Quality("Quality", Int) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull off //double sided
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _DataTex;
            float4 _DataTex_ST;
            float2 _CamPosition;
            float2 _Deviation;
            //int _Quality;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD01;
            };
            
            float BlurOneChannel(sampler2D tex, float2 uv, int channel, float2 offset, int quality = 1)
            {
                float sum = 0;
                float count = 0;
                if(channel < 0 || channel > 3) channel = 0;
                /*if(quality < 1) quality = 1;
                int side = quality * 2 + 1;
                for(int i = 0; i < side; i++)
                {
                    for(int j = 0; j < side; j++)
                    {
                        float intensity = 2 / (abs(i * j) + 2);
                        sum += tex2D(tex, uv + float2((i - quality) * offset.x, (j - quality) * offset.y))[channel] * intensity;
                        count += intensity;
                }
                return sum / count;*/
                if(quality <= 0) return tex2D(tex, uv)[channel];
                for(int i = 0; i < quality; i++)
                {
                    sum += tex2D(tex, uv + float2(i * offset.x, 0 * offset.y))[channel];
                    sum += tex2D(tex, uv + float2(-i * offset.x, 0 * offset.y))[channel];
                    sum += tex2D(tex, uv + float2(0 * offset.x, i * offset.y))[channel];
                    sum += tex2D(tex, uv + float2(0 * offset.x, -i * offset.y))[channel];
                    sum += tex2D(tex, uv + float2(i * offset.x, i * offset.y))[channel];
                    sum += tex2D(tex, uv + float2(i * offset.x, -i * offset.y))[channel];
                    sum += tex2D(tex, uv + float2(-i * offset.x, i * offset.y))[channel];
                    sum += tex2D(tex, uv + float2(-i * offset.x, -i * offset.y))[channel];
                }
                return sum / quality / 8;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(UNITY_MATRIX_M, float4(v.vertex.xyz, 1));
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 main = tex2D(_MainTex, frac(i.uv / 16 + _CamPosition));
                //float4 notmain = tex2D(_DataTex, i.uv / float2(480, 270) + ((_CamPosition - floor(_CamPosition) + float2(0.5, 0)) / float2(32, 19)));   //_Deviation
                float4 notmain = tex2D(_DataTex, i.uv / float2(480, 270) + ((_Deviation + float2(0.25, -0.625)) / float2(32.5, 19.5)));
                return float4(main.rgb, notmain.a);
            }
            ENDCG
        }
    }
}