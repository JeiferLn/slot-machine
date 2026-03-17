Shader "Custom/UI/GaussianBlurMobile"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0,5)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always

        CGINCLUDE
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        half _BlurSize;

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            float4 color : COLOR;
        };

        struct v2f
        {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
            fixed4 color : COLOR;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            o.color = v.color;
            return o;
        }

        half4 GaussianBlur(float2 uv, float2 direction)
        {
            half4 col = tex2D(_MainTex, uv) * 0.227027;

            col += tex2D(_MainTex, uv + direction * 1.384615) * 0.316216;
            col += tex2D(_MainTex, uv - direction * 1.384615) * 0.316216;

            col += tex2D(_MainTex, uv + direction * 3.230769) * 0.070270;
            col += tex2D(_MainTex, uv - direction * 3.230769) * 0.070270;

            return col;
        }

        ENDCG

        Pass
        {
            Name "HorizontalBlur"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            half4 frag(v2f i) : SV_Target
            {
                float2 dir = float2(_MainTex_TexelSize.x * _BlurSize, 0);
                return GaussianBlur(i.uv, dir) * i.color;
            }

            ENDCG
        }

        Pass
        {
            Name "VerticalBlur"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            half4 frag(v2f i) : SV_Target
            {
                float2 dir = float2(0, _MainTex_TexelSize.y * _BlurSize);
                return GaussianBlur(i.uv, dir) * i.color;
            }

            ENDCG
        }
    }
}