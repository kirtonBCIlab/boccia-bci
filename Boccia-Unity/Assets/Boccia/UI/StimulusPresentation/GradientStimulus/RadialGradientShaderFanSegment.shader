Shader "Custom/RadialGradientShaderFanSegment"
{
    Properties
    {
        _GradientColor ("Gradient Color", Color) = (1, 0, 0, 1)  // Red color
        _Radius ("Gradient Radius", Range(0.1, 1)) = 0.10 // How far the red extends
        _Softness ("Gradient Softness", Range(0, 1)) = 0.45 // Controls how smooth the gradient is
        _SegmentSize ("Segment Size", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _GradientColor;
            float _Radius;
            float _Softness;
            float _SegmentSize;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5); // Center of texture UV
                float dist = length(i.uv - center); // Distance from center

                float adjustedRadius = _Radius * _SegmentSize;
                float adjustedSoftness = _Softness * _SegmentSize * 0.4;
                float gradient = smoothstep(adjustedRadius - adjustedSoftness, adjustedRadius + adjustedSoftness, dist); // Blend smoothly

                return lerp(_GradientColor, fixed4(1,1,1,1), gradient); 
            }
            ENDCG
        }
    }
}
