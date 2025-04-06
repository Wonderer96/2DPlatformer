Shader "Custom/FogOfWar" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _ExploredTex ("Explored Texture", 2D) = "black" {}
        _PlayerPos ("Player Position", Vector) = (0,0,0,0)
        _RevealRadius ("Reveal Radius", Float) = 5
    }

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

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
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _ExploredTex;
            float4 _PlayerPos;
            float _RevealRadius;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // �������λ�õ���ǰ���صľ���
                float2 diff = i.worldPos.xz - _PlayerPos.xy;
                float dist = length(diff);
                
                // ��̽���������
                float explored = tex2D(_ExploredTex, i.uv).r;
                
                // ����ɼ���
                float visibility = saturate(explored + step(dist, _RevealRadius));
                
                // ������ɫ����ɫ����
                return fixed4(0, 0, 0, 1 - visibility);
            }
            ENDCG
        }
    }
}
