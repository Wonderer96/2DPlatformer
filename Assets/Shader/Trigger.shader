// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/TransparentGlowGradient"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1) // 基础颜色
        _GlowColor ("Glow Color", Color) = (1,1,0,1) // 发光颜色
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.0 // 发光强度
        _Transparency ("Transparency", Range(0, 1)) = 1.0 // 整体透明度
        _Height ("Gradient Height", Range(0.1, 10)) = 1.0 // 控制透明度递增范围的高度
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // 透明混合
            ZWrite Off // 关闭深度写入，适合透明物体
            Cull Off // 双面渲染

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            // 顶点函数
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // 世界空间位置
                return o;
            }

            float4 _Color;
            float4 _GlowColor;
            float _GlowIntensity;
            float _Transparency;
            float _Height;

            // 片元函数
            fixed4 frag (v2f i) : SV_Target
            {
                // 根据物体的局部y坐标计算透明度梯度（范围：0 - 1）
                float gradient = saturate(i.worldPos.y / _Height);

                // 混合基础颜色和透明度
                fixed4 baseColor = _Color;
                baseColor.a = gradient * _Transparency;

                // 发光效果
                fixed4 glow = _GlowColor * _GlowIntensity;

                return baseColor + glow;
            }
            ENDCG
        }
    }
}


