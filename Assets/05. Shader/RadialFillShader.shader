Shader"Custom/RadialFillShader"
{
  Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FillAmount ("Fill Amount", Range(0,1)) = 0
        _FillColor ("Fill Color", Color) = (1,1,1,1)
        // 스프라이트의 피봇 (기본값은 중앙)
        _Pivot ("Pivot", Vector) = (0.5, 0.5, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
Blend SrcAlpha
OneMinusSrcAlpha
            ZWrite
Off
            Cull
Off
            AlphaToMask
On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;
float _FillAmount;
float4 _FillColor;
float4 _Pivot;

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

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
                // 스프라이트 아틀라스 대응: _MainTex_ST 적용
    float2 uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
    o.uv = uv;
    return o;
}

float PI = 3.14159265;
            // 피봇(_Pivot) 정보를 이용해 중심 기준 좌표 계산
float GetRadialValue(float2 uv)
{
    float2 centered = uv - _Pivot.xy;
    float angle = atan2(centered.y, centered.x);
    angle = (angle + PI) / (2.0 * PI);
    return angle;
}

fixed4 frag(v2f i) : SV_Target
{
    fixed4 texCol = tex2D(_MainTex, i.uv);
    float radial = GetRadialValue(i.uv);
                // _FillAmount에 따라 마스크 결정
    float mask = step(radial, _FillAmount);
    texCol.rgb = lerp(texCol.rgb, _FillColor.rgb, mask);
    texCol.a *= mask;
    return texCol;
}
            ENDCG
        }
    }
}
