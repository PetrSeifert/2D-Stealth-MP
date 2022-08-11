Shader "Unlit/FogOfWar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AlphaTex ("Alpha", 2D) = "white" {}
        _OriginalTex ("Original", 2D) = "white" {}
        [ShowAsVector2] _CamPosition("Camera Position", Vector) = (0, 0, 0, 0)
        _Quality("Quality", Int) = 1
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

            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _AlphaTex;
            float4 _AlphaTex_ST;
            sampler2D _OriginalTex;
            float4 _OriginalTex_ST;
            float2 _CamPosition;
            int _Quality;

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
            
            //float InverseLerp(float a, float b, float v) { return (v-a)/(b-a); }
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
                o.vertex = UnityObjectToClipPos(v.vertex); // = v.vertex
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(UNITY_MATRIX_M, float4(v.vertex.xyz, 1));
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                //return float4(0,1,0,1);
                //float alpha = saturate(lerp(0, 1, length(i.uv * 2 - 1)));
                //float alpha = saturate(InverseLerp(0.25, 0.5, length(i.uv * 2 - 1)));
                // sample the texture
                ///fixed4 col = tex2D(_MainTex, i.uv);
                ///return col;
                //float alpha = saturate(alphaA+alphaG) //omezit součet alpha meshe a alpha gradientu na rozmezí 0-1
                //return (i.worldPos, 1);



                float4 main = tex2D(_MainTex, frac(i.uv + _CamPosition));



                //float alpha = 1 - tex2D(_AlphaTex, i.worldPos.xy).w;
                float alpha = 1 - BlurOneChannel(_AlphaTex, i.worldPos.xy, 3, float2(4/480, 4/270), _Quality);

                //return float4(main.rgb, 1 - alpha.w);
                //return tex2D(_AlphaTex, i.worldPos.xy);
                float4 newTex = float4(main.rgb, alpha);

                float4 oldTex = tex2D(_OriginalTex, i.worldPos.xy);
                return float4(newTex.rgb * alpha + oldTex.rgb * (1 - alpha), 1);
            }

            ENDCG
        }
    }
}
