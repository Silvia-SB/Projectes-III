Shader "Custom/UI/BowChargeBar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorA ("Base Color", Color) = (0.55, 0.45, 0.35, 1)
        _ColorB ("Charged Color", Color) = (1.0, 0.85, 0.55, 1)
        _PulseStrength ("Pulse Strength", Range(0, 1)) = 0.35
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 4
        _ShineStrength ("Shine Strength", Range(0, 2)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _ColorA;
            float4 _ColorB;
            float _PulseStrength;
            float _PulseSpeed;
            float _ShineStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float pulse = (sin(_Time.y * _PulseSpeed) * 0.5 + 0.5) * _PulseStrength;

                float4 col = lerp(_ColorA, _ColorB, pulse);

                float shine = smoothstep(0.65, 1.0, i.uv.y);
                col.rgb += shine * _ShineStrength;

                float4 tex = tex2D(_MainTex, i.uv);
                col.a *= tex.a * i.color.a;

                return col;
            }

            ENDHLSL
        }
    }
}