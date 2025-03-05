Shader"Custom/WarningIndicator"
{
    Properties 
    {
        _FillAmount ("Fill Amount", Range(0, 1)) = 0
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _FillColor ("Fill Color", Color) = (1,0,0,0.5)
        _BaseColor ("Base Color", Color) = (1,1,1,0.2)
        _InnerRadius ("Inner Radius", Range(0, 1)) = 0  // 내부 원 반경 비율 (0=없음, 1=전체)
        _InnerColor ("Inner Color", Color) = (0,1,0,0.5)  // 내부 색상 (안전 구역)
    }
    
    SubShader 
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
ZWrite Off

ZTest LEqual

Blend
SrcAlpha
OneMinusSrcAlpha
        Stencil
{
    Ref 1

    Comp
    Always
            Pass
    Replace

}
        
        Pass 
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"
            
float _FillAmount;
float4 _OutlineColor;
float4 _FillColor;
float4 _BaseColor;
float _InnerRadius;
float4 _InnerColor;
            
struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};
            
struct v2f
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
};
            
v2f vert(appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}
            
fixed4 frag(v2f i) : SV_Target
{
    float2 center = float2(0.5, 0.5);
    float dist = length(i.uv - center) * 2;
                
    float outline = step(0.95, dist) * step(dist, 1.0);
    float fill = step(dist, _FillAmount);
    float baseArea = step(dist, 1.0);
                
                // 내부 원 (안전 구역)
   // 내부 원 (안전 구역)
    float innerArea = step(dist, _InnerRadius * 2.0); // 직경을 고려하여 2를 곱함
    float donutArea = baseArea - innerArea; // 외부원 - 내부원 = 도넛 영역
                
                // 조합: 기본 배경 + 외곽선 + 채우기 (도넛 영역만) + 내부 원
    float4 col = _BaseColor * baseArea +
                             outline * _OutlineColor +
                             (fill * donutArea) * _FillColor +
                             (fill * innerArea) * _InnerColor;
                
                // 알파 블렌딩
    col.a = _BaseColor.a * baseArea +
                        outline * _OutlineColor.a +
                        (fill * donutArea) * _FillColor.a +
                        (fill * innerArea) * _InnerColor.a;
                
    return col;
}
            ENDCG
        }
    }
}