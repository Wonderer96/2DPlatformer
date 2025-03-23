// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/TransparentGlowGradient"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1) // ������ɫ
        _GlowColor ("Glow Color", Color) = (1,1,0,1) // ������ɫ
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.0 // ����ǿ��
        _Transparency ("Transparency", Range(0, 1)) = 1.0 // ����͸����
        _Height ("Gradient Height", Range(0.1, 10)) = 1.0 // ����͸���ȵ�����Χ�ĸ߶�
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // ͸�����
            ZWrite Off // �ر����д�룬�ʺ�͸������
            Cull Off // ˫����Ⱦ

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

            // ���㺯��
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // ����ռ�λ��
                return o;
            }

            float4 _Color;
            float4 _GlowColor;
            float _GlowIntensity;
            float _Transparency;
            float _Height;

            // ƬԪ����
            fixed4 frag (v2f i) : SV_Target
            {
                // ��������ľֲ�y�������͸�����ݶȣ���Χ��0 - 1��
                float gradient = saturate(i.worldPos.y / _Height);

                // ��ϻ�����ɫ��͸����
                fixed4 baseColor = _Color;
                baseColor.a = gradient * _Transparency;

                // ����Ч��
                fixed4 glow = _GlowColor * _GlowIntensity;

                return baseColor + glow;
            }
            ENDCG
        }
    }
}


