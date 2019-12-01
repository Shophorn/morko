Shader "Custom/VerticalFogIntersection"
{
	Properties
	{
	   _Color("Main Color", Color) = (1, 1, 1, .5)
	   _IntersectionThresholdMax("Intersection Threshold Max", float) = 1
	   _NoiseOffset("Noise Offset", float) = 1
	   _NoiseMorph("Noise Morph",float) = 1
	   _NoiseStr("Noise Strength", float) = 0.5
	   _NoiseScale("Noise Scale", float) = 0.5
	}
		SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent"  }

		Pass
		{
		   Blend SrcAlpha OneMinusSrcAlpha
		   ZWrite Off
		   CGPROGRAM
		   #pragma vertex vert
		   #pragma fragment frag
		   #pragma multi_compile_fog
		   #include "UnityCG.cginc"

		   struct appdata
		   {
			   float3 worldPos : TEXCOORD1;
			   float4 scrPos : TEXCOORD0;
			   float4 vertex : POSITION;
		   };

		   struct v2f
		   {
			   float3 worldPos : TEXCOORD1;
			   float4 scrPos : TEXCOORD0;
			   UNITY_FOG_COORDS(0)
			   float4 vertex : SV_POSITION;

			   
		   };

		   sampler2D _CameraDepthTexture;
		   float4 _Color;
		   float4 _IntersectionColor;
		   float _IntersectionThresholdMax;
		   float _NoiseStr;
		   float _NoiseScale;
		   float _NoiseMorph;

		   //ADDITIONS ADDITIONS ADDITIONS BELOW THIS
		   float _NoiseOffset;
		   float hash(float n)
		   {
			   return frac(sin(n)*43758.5453);
		   }

		   float noise(float3 x)
		   {
			   // The noise function returns a value in the range -1.0f -> 1.0f

			   float3 p = floor(x);
			   float3 f = frac(x);

			   f = f * f*(3.0 - 2.0*f);
			   float n = p.x + p.y*57.0 + 113.0*p.z;

			   return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
				   lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
				   lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
					   lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
		   }
		   //ADDITIONS ADDITIONS ADDITIONS ABOVE THIS

		   v2f vert(appdata v)
		   {
			   v2f o;
			   o.worldPos = mul(unity_ObjectToWorld, v.vertex);
			   o.vertex = UnityObjectToClipPos(v.vertex);
			   o.scrPos = ComputeScreenPos(o.vertex);
			   UNITY_TRANSFER_FOG(o,o.vertex);
			   return o;
		   }


			float4 frag(v2f i) : SV_TARGET
			{
			   float depth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)));
			   float diff = saturate(_IntersectionThresholdMax * (depth - i.scrPos.w));

			   float4 col = lerp(fixed4(_Color.rgb, 0.0), _Color, diff * diff * diff * (diff * (6 * diff - 15) + 10));
			   col = float4(col.x, col.y, col.z, col.w);
			   UNITY_APPLY_FOG(i.fogCoord, col);
			   col *= ((noise(float3((i.worldPos.x) * _NoiseScale + (_NoiseOffset/10), i.worldPos.y * _NoiseScale+ (_NoiseMorph/10), i.worldPos.z * _NoiseScale + (_NoiseOffset / 10)))) + 1)*_NoiseStr;
			   return col;
			}

			ENDCG
		}
	}
}