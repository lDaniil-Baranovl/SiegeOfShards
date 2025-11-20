Shader "Custom/ProceduralSummonCircle"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _Radius("Radius", Range(0, 1)) = 0.45
        _EdgeSoftness("Edge Softness", Range(0.001, 0.2)) = 0.03

        _PulseSpeed("Pulse Speed", Float) = 6
        _PulseStrength("Pulse Strength", Float) = 0.40
        _MinAlpha("Min Alpha", Range(0,1)) = 0.25
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Color;
            float _Radius;
            float _EdgeSoftness;
            float _PulseSpeed;
            float _PulseStrength;
            float _MinAlpha;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * 2 - 1; // переводим в диапазон -1..1
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = length(i.uv);

                // ћ€гкий край круга
                float circle = smoothstep(_Radius, _Radius - _EdgeSoftness, dist);

                // ѕульсаци€
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;

                float alpha = lerp(_MinAlpha, 1.0, pulse);
                float brightness = lerp(1 - _PulseStrength, 1 + _PulseStrength, pulse);

                fixed4 col = _Color;
                col.rgb *= brightness;
                col.a *= circle * alpha;

                return col;
            }
            ENDCG
        }
    }
}
