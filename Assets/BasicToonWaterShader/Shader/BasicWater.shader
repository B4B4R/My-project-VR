Shader "Custom/BasicWaterShader_VR_ObjectFoam"
{
    Properties
    {
        _Color("Background Color", Color) = (0.1, 0.4, 0.8, 0.8)
        _TextureColor("Texture Color", Color) = (1, 1, 1, 1)
        _MainTex("Water Texture", 2D) = "white" {}
        _WaveSpeed("Wave Speed", Float) = 0.5
        _WaveStrength("Wave Strength", Range(0, 0.1)) = 0.01
        _WaveAmount("Wave Amount", Float) = 0.1
        _WaveFrequency("Wave Frequency", Float) = 1
        _TextureDistortion("Texture Distortion", Range(0, 1)) = 0.5
        _TransparencySpeed("Transparency Animation Speed", Float) = 1.0
        _TransparencyStrength("Transparency Strength", Range(0, 1)) = 0.5
        _FoamColor("Foam Color", Color) = (1, 1, 1, 1)
        _FoamAmount("Foam Amount", Range(0, 1)) = 0.5
        _FoamCutoff("Foam Cutoff", Range(0, 1)) = 0.5
        _FoamSpeed("Foam Speed", Float) = 0.1
        _FoamNoiseScale("Foam Noise Scale", Float) = 20
        _FoamProximityRange("Foam Proximity Range", Float) = 1.5
    }

        SubShader
        {
            Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
            LOD 100

            CGPROGRAM
            #pragma surface surf Lambert alpha:fade
            #pragma target 3.0
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            fixed4 _Color, _TextureColor, _FoamColor;
            float _WaveSpeed, _WaveStrength, _WaveAmount, _WaveFrequency;
            float _TextureDistortion;
            float _TransparencySpeed, _TransparencyStrength;
            float _FoamAmount, _FoamCutoff, _FoamSpeed, _FoamNoiseScale;
            float _FoamProximityRange;

            struct Input
            {
                float2 uv_MainTex;
                float3 worldPos;
            };

            float2 random2(float2 st)
            {
                st = float2(dot(st, float2(127.1, 311.7)),
                            dot(st, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(st) * 43758.5453123);
            }

            float gradientNoise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);
                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(
                    lerp(dot(random2(i + float2(0.0,0.0)), f - float2(0.0,0.0)),
                         dot(random2(i + float2(1.0,0.0)), f - float2(1.0,0.0)), u.x),
                    lerp(dot(random2(i + float2(0.0,1.0)), f - float2(0.0,1.0)),
                         dot(random2(i + float2(1.0,1.0)), f - float2(1.0,1.0)), u.x),
                    u.y);
            }

            void surf(Input IN, inout SurfaceOutput o)
            {
                float2 uv = IN.uv_MainTex;

                // Waves
                float2 waveOffset = float2(
                    gradientNoise(uv * _WaveFrequency + _Time.y * _WaveSpeed),
                    gradientNoise(uv * _WaveFrequency * 1.2 + _Time.y * _WaveSpeed * 1.1)
                ) * _WaveAmount;

                float2 distortedUV = uv + waveOffset * _WaveStrength * _TextureDistortion;

                fixed4 c = tex2D(_MainTex, distortedUV);
                c = lerp(tex2D(_MainTex, uv), c, _TextureDistortion);
                c *= _TextureColor;

                float transparencyPulse = (sin(_Time.y * _TransparencySpeed) + 1) * 0.5;
                float textureTransparency = lerp(1, transparencyPulse, _TransparencyStrength);

                // Foam noise
                float2 foamUV = IN.worldPos.xz * _FoamNoiseScale + _Time.y * _FoamSpeed;
                float foamNoise = gradientNoise(foamUV);
                float foam = smoothstep(_FoamCutoff, 1, foamNoise * _FoamAmount);

                // Proximity foam approximation
                // On parcourt un point de référence (ex: y=0) pour simuler objets proches
                // Vous pouvez passer la position d’un objet via script en shader global (_ObjectPos)
                float3 objectPos = float3(0,0,0); // mettre à jour via SetGlobalVector depuis script
                float dist = distance(float3(IN.worldPos.x, 0, IN.worldPos.z), float3(objectPos.x, 0, objectPos.z));
                float proximityFactor = saturate(1 - dist / _FoamProximityRange);

                foam = saturate(foam + proximityFactor);

                // Combine
                fixed3 finalColor = lerp(_Color.rgb, c.rgb, c.a * textureTransparency);
                finalColor = lerp(finalColor, _FoamColor.rgb, foam);

                o.Albedo = finalColor;
                o.Alpha = lerp(_Color.a, c.a * _TextureColor.a, c.a * textureTransparency);
                o.Normal = float3(0, 0, 1);
            }

            ENDCG
        }

            FallBack "Transparent/VertexLit"
}
