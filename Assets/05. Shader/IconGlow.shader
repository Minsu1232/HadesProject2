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
                
                // ���� ���� �ִ� �κп��� �۷ο� ȿ�� ����
    fixed4 glow = _GlowColor * _GlowIntensity * col.a;
                
                // ���� ����� �۷ο� ȿ�� ��ġ��
    col.rgb = lerp(col.rgb, glow.rgb, _GlowIntensity * 0.5);
                
                // �ܰ��� ��ȭ
    float alpha = col.a;
    col.a = alpha;
                
    return col;
}
            ENDCG
        }
    }
}