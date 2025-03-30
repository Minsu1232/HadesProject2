Shader"Custom/ProceduralBurnEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        
        _BurnColor1 ("Burn Color 1", Color) = (1, 0.2, 0, 1)
        _BurnColor2 ("Burn Color 2", Color) = (1, 0.7, 0, 1)
        _EmissionColor ("Emission Color", Color) = (1, 0.6, 0.3, 1)
        _BurnSize ("Burn Size", Range(0, 0.3)) = 0.15
        _EmissionIntensity ("Emission Intensity", Range(0, 5)) = 2.0
        
        _NoiseScale ("Noise Scale", Range(1, 50)) = 20
        _FlameSpeed ("Flame Speed", Range(0, 10)) = 5.0
        _FlameNoiseScale ("Flame Noise Scale", Range(1, 50)) = 30
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
Blend 
SrcAlpha
OneMinusSrcAlpha
        ColorMask[_ColorMask]

        Pass
        {
Name"Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

#include "UnityCG.cginc"
#include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

struct appdata_t
{
    float4 vertex : POSITION;
    float4 color : COLOR;
    float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex : SV_POSITION;
    fixed4 color : COLOR;
    float2 texcoord : TEXCOORD0;
    float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
};

sampler2D _MainTex;
fixed4 _Color;
fixed4 _BurnColor1;
fixed4 _BurnColor2;
fixed4 _EmissionColor;
float _BurnSize;
float _DissolveAmount;
float _EmissionIntensity;
float _FlameSpeed;
float _NoiseScale;
float _FlameNoiseScale;
fixed4 _TextureSampleAdd;
float4 _ClipRect;
float4 _MainTex_ST;

            // 2D 해시 함수
float2 hash22(float2 p)
{
    p = float2(dot(p, float2(127.1, 311.7)),
                           dot(p, float2(269.5, 183.3)));
    return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
}

            // 단순화된 Perlin Noise
float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);

    float2 a = hash22(i);
    float2 b = hash22(i + float2(1.0, 0.0));
    float2 c = hash22(i + float2(0.0, 1.0));
    float2 d = hash22(i + float2(1.0, 1.0));

    float x1 = lerp(dot(a, f - float2(0.0, 0.0)), dot(b, f - float2(1.0, 0.0)), u.x);
    float x2 = lerp(dot(c, f - float2(0.0, 1.0)), dot(d, f - float2(1.0, 1.0)), u.x);
    return 0.5 + 0.5 * lerp(x1, x2, u.y);
}

            // 분수 브라운 운동 (Fractal Brownian Motion) - 여러 노이즈를 중첩시켜 자연스러운 패턴 생성
float fbm(float2 p)
{
    float f = 0.0;
    f += 0.5000 * noise(p);
    p *= 2.02;
    f += 0.2500 * noise(p);
    p *= 2.03;
    f += 0.1250 * noise(p);
    p *= 2.01;
    f += 0.0625 * noise(p);
    return f / 0.9375;
}

            // 불꽃 패턴을 위한 노이즈 함수
float fireNoise(float2 p)
{
    float noise1 = fbm(p);
    float noise2 = fbm(p + float2(5.2, 1.3));
                
                // 움직이는 불꽃 효과
    float2 q = float2(noise1, noise2) - 0.5;
    float noise3 = fbm(p + q * _Time.y * 0.3 * _FlameSpeed);
                
                // 방향성 있는 불꽃 (위로 올라가는 효과)
    p.y *= 1.5;
    float finalNoise = fbm(p + float2(noise3, _Time.y * 0.1 * _FlameSpeed));
                
    return finalNoise;
}

v2f vert(appdata_t v)
{
    v2f OUT;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
    OUT.worldPosition = v.vertex;
    OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

    OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                
    OUT.color = v.color * _Color;
    return OUT;
}

fixed4 frag(v2f IN) : SV_Target
{
    half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                
                // 프로시저럴 노이즈 생성
    float2 noiseCoord = IN.texcoord * _NoiseScale;
    float2 flameNoiseCoord = IN.texcoord * _FlameNoiseScale;
                
                // 기본 노이즈 (디졸브 효과용)
    float basicNoise = fbm(noiseCoord);
                
                // 불꽃 패턴 노이즈 (움직이는 효과)
    float flamePattern = fireNoise(flameNoiseCoord);
                
                // 두 노이즈 혼합
    float noise = lerp(basicNoise, flamePattern, 0.4);
                
                // 디졸브 임계값
    float dissolveThreshold = _DissolveAmount;
                
                // 불타는 가장자리 효과 임계값
    float burnLow = saturate(dissolveThreshold - _BurnSize);
    float burnHigh = dissolveThreshold;
                
                // 불타는 효과 강도
    float burnEffect = smoothstep(burnLow, burnHigh, noise);
                
                // 가장자리 색상 계산 (불꽃 색상 그라데이션)
    float fireGradient = saturate(pow(burnEffect * 1.5, 0.5));
    half4 burnColor = lerp(_BurnColor1, _BurnColor2, fireGradient);
                
                // 빛나는 효과 추가
    float emissionMask = pow(burnEffect, 2.0) * _EmissionIntensity;
    burnColor.rgb += _EmissionColor.rgb * emissionMask;
                
                // 최종 색상 계산
    float burnMask = step(noise, dissolveThreshold) * burnEffect;
    color.rgb = lerp(color.rgb, burnColor.rgb, burnMask);
                
                // 알파값 조정 (디졸브 효과)
    color.a *= step(dissolveThreshold, noise);
                
                // 불꽃 경계선 강화
    color.a = lerp(color.a, color.a * 1.2, burnMask);
                
#ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
#endif

#ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
#endif

    return color;
}
        ENDCG
        }
    }
}