Shader"Custom/TrailEffect"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _FadeDistance ("Fade Distance", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
Blend
SrcAlpha
OneMinusSrcAlpha
ZWrite
Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
    float4 color : COLOR;
};

float4 _Color;
float _FadeDistance;

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
    float fade = 1 - i.uv.x;
    fade = pow(fade, _FadeDistance);
                
    float4 col = _Color * i.color;
    col.a *= fade;
                
    return col;
}
            ENDCG
        }
    }
}