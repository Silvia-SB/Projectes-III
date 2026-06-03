Shader "Custom/ElectricityShader"
{
    Properties
    {
        _EmissionColor ("Emission Color", Color) = (0,1,1,1)
        _EmissionStrength ("Emission Strength", Float) = 10

        _BranchSteps ("Branch Steps", Range(2, 12)) = 7
        _BranchSpread ("Branch Angle Spread", Range(0, 3.14)) = 1.2
        _BranchChance ("Branch Chance", Range(0, 1)) = 0.35

        _MaxRays ("Max Rays", Range(1, 8)) = 3

        _Thickness ("Line Thickness", Range(0.001, 0.1)) = 0.02
        _Speed ("Speed", Float) = 1.5

        _MinInterval ("Min Interval", Float) = 0.5
        _MaxInterval ("Max Interval", Float) = 2.5

        _ColorA ("Color A", Color) = (0,1,1,1)
        _ColorB ("Color B", Color) = (1,0,1,1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _EmissionColor;
            float _EmissionStrength;

            float _BranchSteps;
            float _BranchSpread;
            float _BranchChance;

            float _MaxRays;

            float _Thickness;
            float _Speed;

            float _MinInterval;
            float _MaxInterval;

            float4 _ColorA;
            float4 _ColorB;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float2 hash2(float2 p)
            {
                float n = hash(p);
                return float2(n, hash(p + n));
            }

            float lineDist(float2 p, float2 a, float2 b)
            {
                float2 pa = p - a;
                float2 ab = b - a;

                float t = saturate(dot(pa, ab) / dot(ab, ab));
                return length(pa - ab * t);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float time = _Time.y * _Speed;

                float intensity = 0;

                //GLOBAL BURST SYSTEM (interval like before)
                float cycle = floor(time / _MaxInterval);
                float rnd = hash(float2(cycle, cycle));

                float interval = lerp(_MinInterval, _MaxInterval, rnd);
                float active = step(frac(time / interval), 0.5);

                //LIMIT NUMBER OF ROOT RAYS
                int maxRays = (int)_MaxRays;

                float3 rayColor;

                for (int root = 0; root < 8; root++)
                {

                    if (root >= maxRays) break;

                    float2 seed = float2(root * 17.3, root * 9.1);

                    float2 pos = hash2(seed + floor(time * 0.5));
                    float2 dir = normalize(hash2(seed + floor(time)) * 2 - 1);

                    float strength = 1.0;

                    int branchCap = 0;

                    //Color

                    float colorSeed = hash(seed + 33.7 + floor(time));
                    rayColor = lerp(_ColorA.rgb, _ColorB.rgb, colorSeed);

                    //MAIN BRANCH CHAIN
                    for (int i = 0; i < (int)_BranchSteps; i++)
                    {
                        float2 prev = pos;

                        float jitter = (hash(seed + i + floor(time)) - 0.5) * _BranchSpread;

                        float2x2 rot =
                        {
                            cos(jitter), -sin(jitter),
                            sin(jitter),  cos(jitter)
                        };

                        dir = mul(rot, dir);

                        pos += dir * 0.08;

                        float d = lineDist(uv, prev, pos);

                        float lightningLine = smoothstep(_Thickness, 0.0, d);

                        intensity += lightningLine * strength;

                        //BRANCH SPAWN LOGIC
                        if (branchCap < 2)
                        {
                            float branchRoll = hash(seed + i * 3.1 + floor(time));

                            if (branchRoll < _BranchChance)
                            {
                                // fake extra mini-branch
                                float2 bdir = normalize(hash2(seed + i + 99) * 2 - 1);

                                float2 bpos = pos + bdir * 0.05;

                                float d2 = lineDist(uv, pos, bpos);

                                float branchLine = smoothstep(_Thickness * 0.8, 0.0, d2);

                                intensity += branchLine * strength * 0.6;

                                branchCap++;
                            }
                        }

                        strength *= 0.75;
                    }
                }

                //FINAL BURST MASK
                float flicker = 0.5 + 0.5 * sin(time * 25.0);

                float finalMask = intensity * flicker * active;

                float3 col = rayColor * finalMask * _EmissionStrength;

                return half4(col, finalMask);
            }

            ENDHLSL
        }
    }
}