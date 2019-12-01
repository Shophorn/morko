Shader "Morko/VisionTEST"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

		_Tint("Tint", Color) = (1,1,1,1)
		_MaskTex ("Mask", 2D) = "white" {}
    }
    SubShader
    {
		Blend SrcAlpha OneMinusSrcAlpha
        Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

            };

            struct v2f
            {
				float2 uv : TEXCOORD0;
				float4 scrPos : TEXCOORD1;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
			float4 _Tint;
			sampler2D _MaskTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.scrPos = ComputeScreenPos(o.vertex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float4 screenpos = i.scrPos;
				screenpos.x /= screenpos.w;
				screenpos.y /= screenpos.w;

				float mask = tex2D(_MaskTex, screenpos.xy).r;
				fixed4 col = _Tint;
				col.a = lerp(0, 1, mask);
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
