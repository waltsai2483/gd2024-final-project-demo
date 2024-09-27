Shader "Unlit/Attack"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (0.5, 0.5, 0.5, 1)
        _ShadeColor ("Shade Color", Color) = (0.5, 0.5, 0.5, 0.5)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="AlphaTest"
        }
        LOD 200
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGINCLUDE
        #include "UnityCG.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float4 vertex : SV_POSITION;
        };

        fixed4 _MainColor;
        fixed4 _ShadeColor;
        float4 _MainTex_ST;

        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            return o;
        }


        fixed4 frag(v2f i) : SV_Target
        {
            return _MainColor;
        }

        fixed4 fragXRay(v2f i) : SV_Target
        {
            return _ShadeColor;
        }
        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }

        Pass
        {
            ZTest Greater
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragXRay
            ENDCG
        }
    }
}