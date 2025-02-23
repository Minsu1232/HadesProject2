Shader"Custom/WarningIndicator"
{
    Properties 
    {
        _FillAmount ("Fill Amount", Range(0, 1)) = 0
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _FillColor ("Fill Color", Color) = (1,0,0,0.5)
        _BaseColor ("Base Color", Color) = (1,1,1,0.2)  // New base color property
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
float4 _BaseColor; // New base color variable
            
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
                
                // Combine base color, fill color, and outline
    float4 col = _BaseColor * baseArea + // Base color with fixed transparency
                             outline * _OutlineColor +
                             fill * _FillColor;
                
                // Ensure proper alpha blending
    col.a = _BaseColor.a * baseArea +
                        outline * _OutlineColor.a +
                        fill * _FillColor.a;
                
    return col;
}
            ENDCG
        }
    }
}