Shader "Unlit/MapComposite"
{
    Properties
    {
        _MainTex ("Map Texture", 2D) = "white" {}
        _Mask ("Fog Mask", 2D) = "white" {}
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _Mask;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mapColor = tex2D(_MainTex, i.uv);
                fixed4 fogColor = tex2D(_Mask, i.uv);
                
                // 使用迷雾的alpha通道控制可见度
                return lerp(mapColor, fogColor, fogColor.a);
            }
            ENDCG
        }
    }
}
