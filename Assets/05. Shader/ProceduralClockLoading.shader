Shader"UI/ProceduralClockDecoration"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Primary Color", Color) = (0.2,0.6,1,1)
        _SecondColor ("Secondary Color", Color) = (0.4,0.8,1,0.6)
        _Speed ("Rotation Speed", Range(-10,10)) = 2
        _Thickness ("Line Thickness", Range(0.001, 0.05)) = 0.01
        _MainHandLength ("Main Hand Length", Range(0.5, 1.0)) = 0.85
        _SecondHandLength ("Second Hand Length", Range(0.5, 1.0)) = 0.7
        _ClockRadius ("Decoration Radius", Range(0.1, 1.0)) = 0.4
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.5
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 3.0
        _PulseAmount ("Pulse Amount", Range(0, 0.5)) = 0.1
        [Toggle] _ShowSecondHand ("Show Second Hand", Float) = 1
        [Toggle] _ShowCircle ("Show Circle", Float) = 1
        _Opacity ("Overall Opacity", Range(0, 1)) = 0.8
    }
    
    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

Blend SrcAlpha
OneMinusSrcAlpha
        Cull
Off
        ZWrite
Off
        Lighting
Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
#include "UnityCG.cginc"
            
struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    fixed4 color : COLOR;
};
            
struct v2f
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
    fixed4 color : COLOR;
};
            
sampler2D _MainTex;
fixed4 _Color;
fixed4 _SecondColor;
float _Speed;
float _Thickness;
float _MainHandLength;
float _SecondHandLength;
float _ClockRadius;
float _GlowIntensity;
float _PulseSpeed;
float _PulseAmount;
float _ShowSecondHand;
float _ShowCircle;
float _Opacity;
            
v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    o.color = v.color;
    return o;
}
            
            // 선을 그리는 함수
float drawLineSegment(float2 uv, float angle, float thickness, float length)
{
    float2 dir = float2(cos(angle), sin(angle));
    float2 perp = float2(dir.y, -dir.x);
                
                // 선과 점 사이의 거리 계산
    float dist = abs(dot(uv, perp));
                
                // 선의 길이 제한
    float projLength = dot(uv, dir);
    float lengthMask = step(0, projLength) * step(projLength, length);
                
                // 부드러운 가장자리의 선
    float lineValue = 1 - smoothstep(thickness, thickness + 0.005, dist);
                
    return lineValue * lengthMask;
}
            
            // 대시(눈금)를 그리는 함수
float dash(float2 uv, float angle, float thickness, float innerRadius, float outerRadius)
{
    float2 dir = float2(cos(angle), sin(angle));
    float len = length(uv);
                
                // 대시의 방향으로 점을 투영
    float proj = dot(uv, dir);
    float perp = abs(dot(uv, float2(dir.y, -dir.x)));
                
                // 반지름 범위 내에 있는지 확인
    float radiusMask = step(innerRadius, len) * step(len, outerRadius);
                
                // 두께 내에 있는지 확인
    float thicknessMask = step(perp, thickness);
                
    return radiusMask * thicknessMask;
}
            
            // 원을 그리는 함수
float circle(float2 uv, float radius, float thickness)
{
    float len = length(uv);
    float ring = 1 - smoothstep(radius - thickness, radius, len) - (1 - smoothstep(radius, radius + thickness, len));
    return ring;
}
            
            // 글로우 효과 함수
float glow(float shape, float intensity, float width)
{
    return shape * intensity * (1.0 - smoothstep(width, width * 2.0, shape));
}
            
fixed4 frag(v2f i) : SV_Target
{
                // UV 좌표를 -1에서 1 사이로 변환
    float2 uv = i.uv * 2.0 - 1.0;
                
                // 화면 비율에 맞게 조정 (UI 요소는 대부분 정사각형 비율을 가정)
    float len = length(uv);
                
                // 펄스 효과 (시간에 따라 크기가 약간 변함)
    float pulse = sin(_Time.y * _PulseSpeed) * _PulseAmount;
    float adjustedRadius = _ClockRadius + pulse;
                
                // 시간 계산 (더 빠른 회전을 위해 단순화)
    float time = _Time.y * _Speed;
    float angleMain = fmod(time, 6.28318) - 1.57079; // -90도에서 시작 (12시 방향)
    float angleSecond = fmod(time * 2.5, 6.28318) - 1.57079; // 메인보다 빠르게 회전
                
                // 원형 테두리 (선택 사항)
    float circleEffect = 0;
    if (_ShowCircle > 0.5)
    {
                    // 미세하게 움직이는 추가 펄스
        float microPulse = sin(_Time.y * _PulseSpeed * 2.0) * (_PulseAmount * 0.3);
        circleEffect = circle(uv, adjustedRadius + microPulse, _Thickness * 0.3);
    }
                
                // 메인 바늘 (항상 표시)
    float mainHand = drawLineSegment(uv, angleMain, _Thickness, _MainHandLength * adjustedRadius);
                
                // 추가 바늘 (선택 사항)
    float secondHand = 0;
    if (_ShowSecondHand > 0.5)
    {
        secondHand = drawLineSegment(uv, angleSecond, _Thickness * 0.7, _SecondHandLength * adjustedRadius);
    }
                
                // 장식적인 중심점 - 약간 더 화려하게
    float centerSize = _Thickness * 2.0 + sin(_Time.y * _PulseSpeed) * _Thickness * 0.5;
    float center = 1 - smoothstep(centerSize, centerSize + 0.01, length(uv));
                
                // 모든 요소 결합
    float decoration = saturate(circleEffect + mainHand + secondHand + center);
                
                // 글로우 효과 추가 (더 눈에 띄게)
    float glowSize = _Thickness * (3.0 + sin(_Time.y * _PulseSpeed * 0.8) * 1.0);
    float decorationGlow = glow(decoration, _GlowIntensity, glowSize);
                
                // 색상 계산 - 메인 색상과 보조 색상 블렌딩
    fixed4 mainColor = _Color * i.color;
    fixed4 secColor = _SecondColor * i.color;
                
                // 시간에 따라 색상 변화 (선택적)
    float colorBlend = (sin(_Time.y * _PulseSpeed * 0.2) + 1.0) * 0.5;
    fixed4 finalColor = lerp(mainColor, secColor, colorBlend);
                
                // 알파 적용 (전체 불투명도 조절)
    finalColor.a *= saturate(decoration + decorationGlow) * _Opacity;
                
                // 텍스쳐와 결합
    fixed4 texColor = tex2D(_MainTex, i.uv);
    return finalColor * texColor;
}
            ENDCG
        }
    }
    
Fallback"UI/Default"
}