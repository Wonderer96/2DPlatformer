Shader "Custom/2DSolidOutline" {
    Properties {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _EdgeColor ("Edge Color", Color) = (1,1,1,1)
        _EdgeWidth ("Edge Width", Range(1,5)) = 2.0
        _AlphaThreshold ("Alpha Threshold", Range(0,0.5)) = 0.01
    }
    SubShader {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True" 
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _EdgeColor;
            float _EdgeWidth;
            float _AlphaThreshold;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // 可靠的多方向边缘检测
            float IsEdge(float2 uv) {
                float2 texel = _MainTex_TexelSize.xy * _EdgeWidth;
                float centerAlpha = tex2D(_MainTex, uv).a;
                
                // 如果中心点本身透明则直接返回
                if (centerAlpha < _AlphaThreshold) return 0;

                // 四方向+对角线采样
                float3x3 samples;
                samples[0][0] = tex2D(_MainTex, uv + float2(-texel.x, -texel.y)).a; // 左下
                samples[0][1] = tex2D(_MainTex, uv + float2(0,      -texel.y)).a;   // 下
                samples[0][2] = tex2D(_MainTex, uv + float2(texel.x, -texel.y)).a; // 右下
                samples[1][0] = tex2D(_MainTex, uv + float2(-texel.x, 0)).a;       // 左
                samples[1][1] = centerAlpha;                                       // 中
                samples[1][2] = tex2D(_MainTex, uv + float2(texel.x, 0)).a;       // 右
                samples[2][0] = tex2D(_MainTex, uv + float2(-texel.x, texel.y)).a;// 左上
                samples[2][1] = tex2D(_MainTex, uv + float2(0,      texel.y)).a;   // 上
                samples[2][2] = tex2D(_MainTex, uv + float2(texel.x, texel.y)).a; // 右上

                // 检测周围是否有透明像素
                for(int x=0; x<3; x++){
                    for(int y=0; y<3; y++){
                        if(x==1 && y==1) continue; // 跳过中心点
                        if(samples[x][y] < _AlphaThreshold) return 1;
                    }
                }
                return 0;
            }

            fixed4 frag (v2f i) : SV_Target {
                return _EdgeColor * IsEdge(i.uv);
            }
            ENDCG
        }
    }
}
