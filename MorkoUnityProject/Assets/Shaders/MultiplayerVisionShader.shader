Shader "Morko/MultiplayerVision"
{
    Properties
    {
		[HideInInspector]
        _BaseColor ("", 2D) = "white" {}
		[HideInInspector]
		_BaseDepth ("", 2D) = "black" {}
		[HideInInspector]
		_MaskColor ("", 2D) = "black" {}
		[HideInInspector]
		_MaskDepth ("",2D) = "black" {}
		[HideInInspector]
		_MultiplayerColor("",2D) = "black" {}
		[HideInInspector]
		_MultiplayerDepth("",2D) = "black" {}

		_EdgeBlur("Blur Distance",Range(0.0,0.02)) = 0.0
		_OutsideBlur("Outside Blur Scale",Range(0.0,0.02)) = 0.0
		_BlurFilterSize("Blur filter size",int) = 5
		_Saturation ("Desaturation",Range(0.0,1.0)) = 1.0
		_BrightnessFactor ("Brightness Factor",Range(0.0,1.0))= 1.0

		_NearClipPlane ("Near clip plane",float) = 0.3
		_FarClipPlane("Far clip plane",float) = 1000

    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
			Name "PrimoPass"
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
			/*By Some Hero @ unity forums---------------------------------------------------------------------------*/
			float normpdf(float x, float sigma)
			{
				return 0.39894*exp(-0.5*x*x / (sigma*sigma)) / sigma;
			}
			//this is the blur function... pass in standard col derived from tex2d(_MainTex,i.uv)
			half4 blur(sampler2D tex, float2 uv, float blurAmount) {
				//get our base color...
				half4 col = tex2D(tex, uv);
				//total width/height of our blur "grid":
				const int mSize = 6;
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
			/*------------------------------------------------------------------------------------------------------*/
			
			
            sampler2D _BaseColor;
			sampler2D _BaseDepth;
            sampler2D _MaskColor;
			sampler2D _MaskDepth;
			sampler2D _MultiplayerColor;
			sampler2D _MultiplayerDepth;
			int _BlurFilterSize;
			float _OutsideBlur;
			float _Saturation;
			float _BrightnessFactor;
			float _EdgeBlur;
			float _NearClipPlane;
			float _FarClipPlane;

			float2 BlurSampled(sampler2D maskColor, float2 uv, float blurAmount)
			{
				const int size = _BlurFilterSize;
				float2 col = tex2D(maskColor, uv).rg;
				float2 col2 = col;
				for (int y = -size; y <= size; ++y)
				{
					for (int x = -size; x <= size; ++x)
					{
						col += tex2D(maskColor, float2(uv.x + x * blurAmount, uv.y + y * blurAmount)).rg;
					}
				}
				float2 normalized = col / ((size * 2 + 1) * (size * 2 + 1));
				return float2(min(col2, normalized).r,col2.g);
			}

			float3 BlurMasked(sampler2D originalColor, float2 uv, float blurAmount, float maskColor)
			{
				const int size = _BlurFilterSize;
				float3 col = tex2D(originalColor, uv).rgb;
				float3 col2 = col;
				for (int y = -size; y <= size; ++y)
				{
					for (int x = -size; x <= size; ++x)
					{
						col += tex2D(originalColor, float2(uv.x + x * blurAmount/2 * maskColor, uv.y + y * blurAmount/2 * maskColor)).rgb;
					}
				}
				float3 normalized = col / ((size * 2 + 1) * (size * 2 + 1));
				return normalized;
			}
			struct output
			{
				float4 color : SV_Target;
				float depth : SV_Depth;
			};

			output frag (v2f i)
            {
				float baseDepth = tex2D(_BaseDepth, i.uv).r;

				float2 maskTex = BlurSampled(_MaskColor, i.uv, _EdgeBlur);

				float3 baseColor = BlurMasked(_BaseColor, i.uv, _OutsideBlur, 1-max(maskTex.r,maskTex.g));
				
				float maskFull = maskTex.g;
				float maskPartial = maskTex.r;
				float maskDepth = tex2D(_MaskDepth, i.uv).r;

				float3 morkoColor = tex2D(_MultiplayerColor, i.uv).rgb;
				float morkoDepth = tex2D(_MultiplayerDepth, i.uv).r;

				float depthTest = step(baseDepth - morkoDepth, 0);
				float depthTest2 = 1-step(baseDepth - maskDepth, 0);
				float3 inSight = lerp(baseColor, morkoColor, depthTest);

				float3 darkening = baseColor * _BrightnessFactor;
				float3 outOfSight = lerp(darkening, Luminance(darkening).rrr, _Saturation);

				float fullMask = max(maskFull * depthTest2, maskPartial);
				float4 finalColor = fixed4(lerp(inSight, outOfSight, 1 - fullMask), 1);
				output o;
				o.color = finalColor;
				o.depth = max(baseDepth,morkoDepth);
				//o.color = float4(o.depth.rrr, 1);
                return o;
            }
            ENDCG
        }
		
    }
}
