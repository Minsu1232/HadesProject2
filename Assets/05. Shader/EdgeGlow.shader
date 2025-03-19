Shader"UI/EdgeGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // 글로우 효과 속성
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowPower ("Glow Power", Range(0, 10)) = 2
        _GlowWidth ("Glow Width", Range(0, 20)) = 5
        _AlphaCutoff ("Alpha Cutoff", Range(0, 1)) = 0.1
        
        // 펄스 효과 속성
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 1
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.2
        
        // UI 마스크 지원
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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
        
        Stencil
        {
Ref[_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
ReadMask[_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
Cull Off

Lighting Off

ZWrite Off

ZTest[unity_GUIZTestMode]
Blend SrcAlpha
OneMinusSrcAlpha
        ColorMask[_ColorMask]
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"
#include "UnityUI.cginc"
            
struct appdata_t
{
    float4 vertex : POSITION;
    float4 color : COLOR;
    float2 texcoord : TEXCOORD0;
};
            
struct v2f
{
    float4 vertex : SV_POSITION;
    fixed4 color : COLOR;
    float2 texcoord : TEXCOORD0;
    float4 worldPosition : TEXCOORD1;
};
            
fixed4 _Color;
fixed4 _GlowColor;
float _GlowPower;
float _GlowWidth;
float _AlphaCutoff;
float _PulseSpeed;
float _PulseAmount;
float4 _ClipRect;
sampler2D _MainTex;
float4 _MainTex_TexelSize;
            
v2f vert(appdata_t IN)
{
    v2f OUT;
    OUT.worldPosition = IN.vertex;
    OUT.vertex = UnityObjectToClipPos(IN.vertex);
    OUT.texcoord = IN.texcoord;
    OUT.color = IN.color * _Color;
    return OUT;
}
            
fixed4 frag(v2f IN) : SV_Target
{
                // 기본 텍스처 색상
    half4 color = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // 알파 컷오프로 가장자리 감지 준비
    float originalAlpha = color.a;
                
                // 가장자리 감지
    float2 texelSize = _MainTex_TexelSize.xy * _GlowWidth;
    float edgeIntensity = 0;
                
                // 3x3 이웃 픽셀 확인
    for (int i = -1; i <= 1; i++)
    {
        for (int j = -1; j <= 1; j++)
        {
            if (i == 0 && j == 0)
                continue; // 자신은 건너뛰기
                        
            float2 offset = float2(i, j) * texelSize;
            float neighborAlpha = tex2D(_MainTex, IN.texcoord + offset).a;
                        
                        // 현재 픽셀이 불투명하고 이웃이 투명하면 가장자리로 간주
            if (originalAlpha > _AlphaCutoff && neighborAlpha < _AlphaCutoff)
            {
                edgeIntensity = 1.0;
                break;
            }
        }
        if (edgeIntensity > 0)
            break;
    }
                
                // 펄스 효과 (시간에 따른 강도 변화)
    float pulseValue = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
    float glowIntensity = _GlowPower * (1 + pulseValue * _PulseAmount);
                
                // 글로우 효과 적용
    float glow = edgeIntensity * glowIntensity;
                
                // 글로우 색상과 원본 색상 혼합
    color.rgb = color.rgb + (_GlowColor.rgb * glow * _GlowColor.a);
                
                // UI 마스크 적용
    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
    return color;
}
            ENDCG
        }
    }
    
Fallback"UI/Default"
}