Shader "Custom/EdgeSparkle"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1,1,1,1)

        _RimColor ("Glow Color", Color) = (0.5,1,2,1)
        _RimPower ("Rim Width", Range(0.1,10)) = 2.5
        _GlowIntensity ("Glow Intensity", Range(0,10)) = 3.0

        _NoiseTex ("Noise (optional)", 2D) = "white" {}
        _SparkleScale ("Sparkle Scale", Range(0.01,5)) = 0.5
        _SparkleSpeed ("Sparkle Speed", Range(0,10)) = 2.0
        _SparkleThreshold ("Sparkle Threshold", Range(0,1)) = 0.7
    }

    SubShader
    {
        Tags
        { 
            "RenderType"="Transparent"
            "Queue"="Overlay"
        }

        // This makes it draw regardless of depth
        ZWrite Off
        ZTest Always
        Blend SrcAlpha One     // additive blending (glow style)
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;

            float4 _Color;
            float4 _RimColor;
            float _RimPower;
            float _GlowIntensity;
            float _SparkleScale;
            float _SparkleSpeed;
            float _SparkleThreshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 N = normalize(i.worldNormal);
                float3 V = normalize(_WorldSpaceCameraPos - i.worldPos);

                // Fresnel rim
                float rim = pow(1.0 - saturate(dot(N, V)), _RimPower);

                // Sparkle noise
                float2 nUV = i.worldPos.xz * _SparkleScale + _Time.y * _SparkleSpeed;
                float noise = tex2D(_NoiseTex, nUV).r;
                noise = lerp(noise, hash21(nUV), 0.5);

                float sparkle = step(_SparkleThreshold, noise);

                // Final glow intensity
                float glow = rim * (_GlowIntensity * (1 + sparkle));

                // Base color
                fixed4 baseCol = tex2D(_MainTex, i.uv) * _Color;

                // Additive emissive glow
                fixed4 col = baseCol + _RimColor * glow;
                col.a = saturate(glow); // control transparency via glow
                return col;
            }
            ENDCG
        }
    }
}
