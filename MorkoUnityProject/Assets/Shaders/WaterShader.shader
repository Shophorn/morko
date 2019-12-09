Shader "Custom/WaterShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Bump", 2D) = "white" {}
		_Hmap("HeightMap",2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_Parallax("Parallax",float) = 1
		_NoiseOffset("Noise Offset", float) = 1
	   _NoiseMorph("Noise Morph",float) = 1
	   _NoiseStr("Noise Strength", float) = 0.5
	   _NoiseScale("Noise Scale", float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
		#pragma shader_feature _NORMAL_MAP
		#pragma shader_feature _PARALLAX_MAP

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
			float3 viewDir;
        };



        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		sampler2D _BumpMap;
		sampler2D _Hmap;

		float _NoiseStr;
		float _NoiseScale;
		float _NoiseMorph;
		float _NoiseOffset;
		float _Parallax;


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

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata_full v) 
		{
			v.vertex.xyz += v.normal* 10 * ((noise(float3((v.vertex.x) * _NoiseScale + (_NoiseOffset / 10), 1 * _NoiseScale + (_NoiseMorph / 10), v.vertex.y * _NoiseScale + (_NoiseOffset / 10)))) + 1)*_NoiseStr;
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			IN.uv_MainTex += pow(ParallaxOffset(tex2D(_Hmap, IN.uv_MainTex).r, _Parallax, IN.viewDir.xyz),2);
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			;
			
			float2 uv2 = IN.uv_MainTex;
			uv2.x += +_NoiseOffset;
			uv2 *= _NoiseScale;
			o.Normal = UnpackNormal(tex2D(_BumpMap, uv2) *_NoiseStr * ((noise(float3((IN.uv_MainTex.x) * _NoiseScale + (_NoiseOffset / 10), 1 * _NoiseScale + (_NoiseMorph / 10), IN.uv_MainTex.y * _NoiseScale + (_NoiseOffset / 10)))) + 1)*_NoiseStr);
			o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
