Shader"Custom/WarningIndicatorRectangle"
{
    Properties 
    {
        _FillAmount ("Fill Amount", Range(0, 1)) = 0
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _FillColor ("Fill Color", Color) = (1,0,0,0.5)
        _BaseFillColor ("Base Fill Color", Color) = (1,0.5,0.5,0.2)
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

    Comp Always
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
float4 _BaseFillColor;
            
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
                // Adjust UV to control fill direction from center
    float2 centeredUV = abs(i.uv - 0.5) * 2;
                
                // Calculate horizontal fill
    float horizontalFill = step(centeredUV.x, _FillAmount);
                
// 아웃라인 로직 수정
    float outlineThickness = 0.01; // 고정 두께
    float outlineX = step(1.0 - outlineThickness, centeredUV.x) * step(centeredUV.x, 1.0);
    float outlineY = step(1.0 - outlineThickness, centeredUV.y) * step(centeredUV.y, 1.0);
    float outline = max(outlineX, outlineY);
                // Combine base fill and fill color
    float4 col = outline * _OutlineColor +
                             (1.0 - horizontalFill) * _BaseFillColor +
                             horizontalFill * _FillColor;
                
                // Alpha masking
    col.a *= step(max(centeredUV.x, centeredUV.y), 1.0);
                
    return col;
}
            ENDCG
        }
    }
}