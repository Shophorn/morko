Shader "My/PostFx"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Saturation ("Desaturation",Range(0.0,1.0)) = 1.0
		_BrightnessFactor ("Brightness Factor",Range(0.0,2.0))= 1.0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			float _Saturation;
			float _BrightnessFactor;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 original = tex2D(_MainTex, i.uv);
				fixed lum = saturate(Luminance(original.rgb) * _BrightnessFactor);
				fixed4 output;
				output.rgb = lerp(original.rgb, fixed3(lum, lum, lum), _Saturation);
				output.a = original.a;
  

                return output;
            }
            ENDCG
        }
    }
}
