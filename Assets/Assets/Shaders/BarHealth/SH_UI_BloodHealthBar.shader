Shader "Custom/UI/BloodHealthBar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorA ("Dark Blood", Color) = (0.35, 0.02, 0.02, 1)
        _ColorB ("Fresh Blood", Color) = (0.85, 0.04, 0.02, 1)
        _Speed ("Flow Speed", Range(0, 5)) = 1.2
        _WaveStrength ("Wave Strength", Range(0, 1)) = 0.25
        _ShineStrength ("Shine Strength", Range(0, 2)) = 0.6
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
            float _Speed;
            float _WaveStrength;
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
                float wave =
                    sin((i.uv.x * 18.0) + (_Time.y * _Speed)) * 0.5 + 0.5;

                float vertical =
                    smoothstep(0.0, 1.0, i.uv.y);

                float mixValue = saturate(wave * _WaveStrength + vertical * 0.35);

                float4 col = lerp(_ColorA, _ColorB, mixValue);

                float shine = smoothstep(0.68, 0.95, i.uv.y);
                col.rgb += shine * _ShineStrength * float3(0.8, 0.15, 0.05);

                float4 tex = tex2D(_MainTex, i.uv);
                col.a *= tex.a * i.color.a;

                return col;
            }

            ENDHLSL
        }
    }
}