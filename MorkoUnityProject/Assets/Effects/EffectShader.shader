Shader "Morko/EffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Color ("Color", color) = (1,1,1,1)
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend", float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend", float) = 0
        [Enum(UnityEngine.Rendering.BlendOp)]_BlendOp("Blend Operation", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100

        ZWrite off
        ZTest always 

        // Blend [_SrcBlend] [_DstBlend]
        // BlendOp [_BlendOp]

        Blend [_SrcBlend] [_DstBlend]
        BlendOp [_BlendOp]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _MODE_DEFAULT _MODE_ADDITIVE   

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

            sampler2D _MainTex;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float alpha = tex2D(_MainTex, i.uv).a;
                float4 color = float4(_Color.rgb, alpha);
                return color;
            }
            ENDCG
        }
    }
}
