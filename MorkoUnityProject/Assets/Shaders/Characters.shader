Shader "Morko/Characters"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _EmissionPower ("Emission Power", Float) = 0.1

        [HideInInspector]
        _VisibilityMask ("HIDDEN VISIBILITY MASK", 2D) = "white" {}

        _AlphaCutoff("Alpha Cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        // Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        Tags { "RenderType"="Opaque" }
        LOD 200

        // Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alphatest:_AlphaCutoff nofog

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
        };

        half _Smoothness;
        half _Metallic;
        half _EmissionPower;
        fixed4 _Color;


        sampler2D _VisibilityMask;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

            o.Metallic = 0;
            o.Smoothness = _Smoothness;

            float2 maskUv = IN.screenPos.xy / IN.screenPos.w;
            float visibility = tex2D(_VisibilityMask, maskUv).r;
            o.Alpha = visibility;
            // o.Albedo = float3(1,0,0);
            // o.Emission = visibility.rrr;//o.Albedo * 0.01;//_EmissionPower;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
