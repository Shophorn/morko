Shader "Morko/VideoPlayerVignette"
{
    Properties
    {
        [HideInInspector]
        _MainTex ("Texture", 2D) = "white" {}
        _Tint("Tint", color) = (1,1,1,1)
        [KeywordEnum(Material, UIComponent, Both)] _TintSource("Tint Color Source", int) = 0
        
        _Distance ("Distance", Range(0, 1)) =  0.5
        _Intensity ("Intensity", Float) = 1
        _VignetteColor ("Vignette Color", color) = (0,0,0,1)
        _DesaturationEffect ("Desaturation", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _TINTSOURCE_MATERIAL _TINTSOURCE_UICOMPONENT _TINTSOURCE_BOTH

            #include "UnityCG.cginc"

            #define INV_SQRT2 0.7071067811865475

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float3 _Tint;

            float _Distance;
            float _Intensity;
            float3 _VignetteColor;

            float _DesaturationEffect;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);

            #if _TINTSOURCE_MATERIAL || _TINTSOURCE_BOTH
                color.rgb *= _Tint;
            #endif

            #if _TINTSOURCE_UICOMPONENT || _TINTSOURCE_BOTH
                color.rgb *= i.color;
            #endif

                float3 desaturated = Luminance(color.rgb);
                // 1−(1−A)×(1−B)
                float3 one3 = float3(1,1,1);

                desaturated = one3 - (one3 - desaturated) * (one3 - desaturated);
                desaturated = one3 - (one3 - desaturated) * (one3 - desaturated);
                color.rgb = lerp(color.rgb, desaturated, _DesaturationEffect);

                // color.rgb = Luminance(color.rgb);

                float2 toCenter = 2.0 * i.uv - 1.0;
                float dist = length(toCenter);
                dist *= INV_SQRT2; // To map distorted uv-space circle of radius 1 to radius sqrt(2) so that it reaches the corners of [0...1] uv-space
                float factor = pow(1.0 - dist.r, _Intensity);

                color.rgb *= factor;


                return color;
            }
            ENDCG
        }
    }
}
