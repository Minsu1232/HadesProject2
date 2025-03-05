Shader"Custom/DonutIndicator"
{
    Properties 
    {
        _FillAmount ("Fill Amount", Range(0, 1)) = 0
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _FillColor ("Fill Color", Color) = (1,0,0,0.5)
        _BaseFillColor ("Base Fill Color", Color) = (1,0.5,0.5,0.2)
        _InnerRadius ("Inner Radius", Range(0, 1)) = 0.5
    }
    
    SubShader 
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
ZWrite Off

ZTest LEqual

Blend
SrcAlpha
OneMinusSrcAlpha
        
        Pass 
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"
            
float _FillAmount;
float4 _OutlineColor;
float4 _FillColor;
float4 _BaseFillColor;
float _InnerRadius;
            
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
                
                // ���� ���� ���
    float innerEdge = _InnerRadius;
    float outerEdge = 1.0;
                
    float innerMask = step(innerEdge, dist);
    float outerMask = step(dist, outerEdge);
    float donutMask = innerMask * outerMask;
                
                // Fill ȿ�� ���
    float fillMask = step(dist, _FillAmount * outerEdge);
                
                // �ƿ����� ���
    float outlineThickness = 0.01;
    float innerOutline = abs(dist - innerEdge) < outlineThickness ? 1.0 : 0.0;
    float outerOutline = abs(dist - outerEdge) < outlineThickness ? 1.0 : 0.0;
    float outlineMask = (innerOutline + outerOutline) * donutMask;
                
                // ���� ȥ�� - �簢�� ���̴��� ������ ���
    float4 col = outlineMask * _OutlineColor +
                             donutMask * (1.0 - fillMask) * _BaseFillColor +
                             donutMask * fillMask * _FillColor;
                
                // ���İ� ����
    col.a *= donutMask;
                
                // ���� ���� ���� ��� (��ü ��)
    if (_InnerRadius < 0.01)
    {
        outlineMask = outerOutline;
        col = outlineMask * _OutlineColor +
                          outerMask * (1.0 - fillMask) * _BaseFillColor +
                          outerMask * fillMask * _FillColor;
        col.a *= outerMask;
    }
                
    return col;
}
            ENDCG
        }
    }
}