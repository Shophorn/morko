Shader "Morko/Characters"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _SpecularColor ("Specular, Smoothness on alpha", 2D) = "black" {}
        [Normal]_NormalMap("Normal map", 2D) = "bump"{}
     
        [HDR]_EmissionColor ("Emission", color) = (0,0,0,0)
        _EmissionPower ("Emission Power", Float) = 0.1

        [Toggle]_IsMetallic("Is Metallic (overrides specular)", Float) = 0
        _MetallicSmooth("Metallic Smoothness(only with Is Metallic", Range (0, 1)) = 0
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
        #pragma surface surf StandardSpecular fullforwardshadows alphatest:_AlphaCutoff nofog

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _SpecularColor;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
        };


        half _EmissionPower;
        fixed4 _Color;

        float4 _EmissionColor;
        sampler2D _VisibilityMask;

        // Todo(Leo): Super poor metallic setting, make proper with keyword enum for example
        float _IsMetallic;
        float _MetallicSmooth;
        sampler2D _NormalMap;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandardSpecular o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 color = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = color.rgb;

            float4 specular = tex2D(_SpecularColor, IN.uv_MainTex);

            o.Specular = lerp(specular.rgb, color.rgb, _IsMetallic);
            o.Smoothness = lerp(specular.a, _MetallicSmooth, _IsMetallic);
            o.Emission = _EmissionColor;

            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));



            float2 maskUv = IN.screenPos.xy / IN.screenPos.w;
            float visibility = tex2D(_VisibilityMask, maskUv).r;
            o.Alpha = visibility;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
