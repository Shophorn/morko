Shader "Morko/VisionCone"
{
    Properties
    {
		_Color ("Color", color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
		BlendOp Add
		Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 vert (float4 vertex : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }

			fixed4 _Color;

            fixed4 frag (float4 v : SV_POSITION) : SV_Target
            {
				return _Color;
            }
            ENDCG
        }
    }
}
