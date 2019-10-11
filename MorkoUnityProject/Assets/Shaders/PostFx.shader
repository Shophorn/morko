Shader "My/PostFx"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_AlphaMask ("AlphaMask", 2D) = "black" {}
		_EdgeBlur ("Blur, Range",Range(0.0,1.0)) = 1.0
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
			/*Gaussian Blur stuff-----------------------------------------------------------------------------------*/
			
			float normpdf(float x, float sigma)
			{
				return 0.39894*exp(-0.5*x*x / (sigma*sigma)) / sigma;
			}
			//this is the blur function... pass in standard col derived from tex2d(_MainTex,i.uv)
			half4 blur(sampler2D tex, float2 uv, float blurAmount) {
				//get our base color...
				half4 col = tex2D(tex, uv);
				//total width/height of our blur "grid":
				const int mSize = 11;
				//this gives the number of times we'll iterate our blur on each side 
				//(up,down,left,right) of our uv coordinate;
				//NOTE that this needs to be a const or you'll get errors about unrolling for loops
				const int iter = (mSize - 1) / 2;
				//run loops to do the equivalent of what's written out line by line above
				//(number of blur iterations can be easily sized up and down this way)
				for (int i = -iter; i <= iter; ++i) {
					for (int j = -iter; j <= iter; ++j) {
						col += tex2D(tex, float2(uv.x + i * blurAmount, uv.y + j * blurAmount)) * normpdf(float(i), 7);
					}
				}
					//return blurred color
				return (col / mSize);
			}
			/*------------------------------------------------------------------------------------------------------*/

            sampler2D _MainTex;
            sampler2D _AlphaMask;
			float _Saturation;
			float _BrightnessFactor;
			float _EdgeBlur;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 original = tex2D(_MainTex, i.uv);
				fixed4 mask = tex2D(_AlphaMask, i.uv);
				fixed lum = saturate(Luminance(original.rgb) * _BrightnessFactor);
				fixed4 output;
				output.rgb = lerp(original.rgb, fixed3(lum, lum, lum), 1-blur(_AlphaMask, i.uv, _EdgeBlur).r);
				output.a = original.a;
  

                return output;
            }
            ENDCG
        }
    }
}
