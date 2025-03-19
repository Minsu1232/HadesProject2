Shader"UI/EdgeGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // �۷ο� ȿ�� �Ӽ�
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowPower ("Glow Power", Range(0, 10)) = 2
        _GlowWidth ("Glow Width", Range(0, 20)) = 5
        _AlphaCutoff ("Alpha Cutoff", Range(0, 1)) = 0.1
        
        // �޽� ȿ�� �Ӽ�
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 1
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.2
        
        // UI ����ũ ����
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
                // �⺻ �ؽ�ó ����
    half4 color = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // ���� �ƿ����� �����ڸ� ���� �غ�
    float originalAlpha = color.a;
                
                // �����ڸ� ����
    float2 texelSize = _MainTex_TexelSize.xy * _GlowWidth;
    float edgeIntensity = 0;
                
                // 3x3 �̿� �ȼ� Ȯ��
    for (int i = -1; i <= 1; i++)
    {
        for (int j = -1; j <= 1; j++)
        {
            if (i == 0 && j == 0)
                continue; // �ڽ��� �ǳʶٱ�
                        
            float2 offset = float2(i, j) * texelSize;
            float neighborAlpha = tex2D(_MainTex, IN.texcoord + offset).a;
                        
                        // ���� �ȼ��� �������ϰ� �̿��� �����ϸ� �����ڸ��� ����
            if (originalAlpha > _AlphaCutoff && neighborAlpha < _AlphaCutoff)
            {
                edgeIntensity = 1.0;
                break;
            }
        }
        if (edgeIntensity > 0)
            break;
    }
                
                // �޽� ȿ�� (�ð��� ���� ���� ��ȭ)
    float pulseValue = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
    float glowIntensity = _GlowPower * (1 + pulseValue * _PulseAmount);
                
                // �۷ο� ȿ�� ����
    float glow = edgeIntensity * glowIntensity;
                
                // �۷ο� ����� ���� ���� ȥ��
    color.rgb = color.rgb + (_GlowColor.rgb * glow * _GlowColor.a);
                
                // UI ����ũ ����
    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
    return color;
}
            ENDCG
        }
    }
    
Fallback"UI/Default"
}