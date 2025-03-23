Shader"Custom/IconGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0, 1)) = 0.5
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

Cull Off

Lighting Off

ZWrite Off

Blend 
SrcAlpha 
OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"
            
struct appdata
{
    float4 vertex : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float4 vertex : SV_POSITION;
    fixed4 color : COLOR;
    float2 uv : TEXCOORD0;
};
            
sampler2D _MainTex;
float4 _GlowColor;
float _GlowIntensity;
            
v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    o.color = v.color;
    return o;
}
            
fixed4 frag(v2f i) : SV_Target
{
    fixed4 col = tex2D(_MainTex, i.uv);
                
                // 알파 값이 있는 부분에만 글로우 효과 적용
    fixed4 glow = _GlowColor * _GlowIntensity * col.a;
                
                // 원본 색상과 글로우 효과 합치기
    col.rgb = lerp(col.rgb, glow.rgb, _GlowIntensity * 0.5);
                
                // 외곽선 강화
    float alpha = col.a;
    col.a = alpha;
                
    return col;
}
            ENDCG
        }
    }
}