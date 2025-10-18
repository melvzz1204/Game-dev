Shader "Custom/HiddenGlowOutline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0, 1, 1, 1)
        _OutlineThickness("Outline Thickness", Range(0.0, 0.05)) = 0.015
        _GlowIntensity("Glow Intensity", Range(0, 5)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Overlay" }
        ZTest Always
        ZWrite Off
        Cull Front
        Blend SrcAlpha One

        Pass
        {
            Name "HiddenGlowOutline"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float _OutlineThickness;
            float4 _OutlineColor;
            float _GlowIntensity;

            v2f vert(appdata v)
            {
                v2f o;
                float3 norm = normalize(v.normal);
                v.vertex.xyz += norm * _OutlineThickness;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(_OutlineColor.rgb * _GlowIntensity, _OutlineColor.a);
            }
            ENDCG
        }
    }

    FallBack Off
}
