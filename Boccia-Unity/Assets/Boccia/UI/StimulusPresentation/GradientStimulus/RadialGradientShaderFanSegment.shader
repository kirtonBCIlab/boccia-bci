Shader "Custom/RadialGradientShaderFanSegment"
{
    Properties
    {
        _GradientColor ("Gradient Color", Color) = (1, 0, 0, 1)  // Red color
        _Radius ("Gradient Radius", Range(0.1, 1)) = 0.16 // How far the red extends
        _Softness ("Gradient Softness", Range(0.1, 1)) = 0.45 // Controls how smooth the gradient is
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
                float gradient = smoothstep(_Radius - _Softness, _Radius + _Softness, dist); // Blend smoothly
                return lerp(_GradientColor, fixed4(1,1,1,1), gradient); // Blend red to white

                // float dist = i.uv.y; // Use the Y-axis of UVs as radial distance
                // float gradient = smoothstep(_Radius - _Softness, _Radius + _Softness, dist); // Smooth transition
                // return lerp(_GradientColor, fixed4(1,1,1,1), gradient);
            }
            ENDCG
        }
    }
}
