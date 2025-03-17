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
            
            // ���� �׸��� �Լ�
float drawLineSegment(float2 uv, float angle, float thickness, float length)
{
    float2 dir = float2(cos(angle), sin(angle));
    float2 perp = float2(dir.y, -dir.x);
                
                // ���� �� ������ �Ÿ� ���
    float dist = abs(dot(uv, perp));
                
                // ���� ���� ����
    float projLength = dot(uv, dir);
    float lengthMask = step(0, projLength) * step(projLength, length);
                
                // �ε巯�� �����ڸ��� ��
    float lineValue = 1 - smoothstep(thickness, thickness + 0.005, dist);
                
    return lineValue * lengthMask;
}
            
            // ���(����)�� �׸��� �Լ�
float dash(float2 uv, float angle, float thickness, float innerRadius, float outerRadius)
{
    float2 dir = float2(cos(angle), sin(angle));
    float len = length(uv);
                
                // ����� �������� ���� ����
    float proj = dot(uv, dir);
    float perp = abs(dot(uv, float2(dir.y, -dir.x)));
                
                // ������ ���� ���� �ִ��� Ȯ��
    float radiusMask = step(innerRadius, len) * step(len, outerRadius);
                
                // �β� ���� �ִ��� Ȯ��
    float thicknessMask = step(perp, thickness);
                
    return radiusMask * thicknessMask;
}
            
            // ���� �׸��� �Լ�
float circle(float2 uv, float radius, float thickness)
{
    float len = length(uv);
    float ring = 1 - smoothstep(radius - thickness, radius, len) - (1 - smoothstep(radius, radius + thickness, len));
    return ring;
}
            
            // �۷ο� ȿ�� �Լ�
float glow(float shape, float intensity, float width)
{
    return shape * intensity * (1.0 - smoothstep(width, width * 2.0, shape));
}
            
fixed4 frag(v2f i) : SV_Target
{
                // UV ��ǥ�� -1���� 1 ���̷� ��ȯ
    float2 uv = i.uv * 2.0 - 1.0;
                
                // ȭ�� ������ �°� ���� (UI ��Ҵ� ��κ� ���簢�� ������ ����)
    float len = length(uv);
                
                // �޽� ȿ�� (�ð��� ���� ũ�Ⱑ �ణ ����)
    float pulse = sin(_Time.y * _PulseSpeed) * _PulseAmount;
    float adjustedRadius = _ClockRadius + pulse;
                
                // �ð� ��� (�� ���� ȸ���� ���� �ܼ�ȭ)
    float time = _Time.y * _Speed;
    float angleMain = fmod(time, 6.28318) - 1.57079; // -90������ ���� (12�� ����)
    float angleSecond = fmod(time * 2.5, 6.28318) - 1.57079; // ���κ��� ������ ȸ��
                
                // ���� �׵θ� (���� ����)
    float circleEffect = 0;
    if (_ShowCircle > 0.5)
    {
                    // �̼��ϰ� �����̴� �߰� �޽�
        float microPulse = sin(_Time.y * _PulseSpeed * 2.0) * (_PulseAmount * 0.3);
        circleEffect = circle(uv, adjustedRadius + microPulse, _Thickness * 0.3);
    }
                
                // ���� �ٴ� (�׻� ǥ��)
    float mainHand = drawLineSegment(uv, angleMain, _Thickness, _MainHandLength * adjustedRadius);
                
                // �߰� �ٴ� (���� ����)
    float secondHand = 0;
    if (_ShowSecondHand > 0.5)
    {
        secondHand = drawLineSegment(uv, angleSecond, _Thickness * 0.7, _SecondHandLength * adjustedRadius);
    }
                
                // ������� �߽��� - �ణ �� ȭ���ϰ�
    float centerSize = _Thickness * 2.0 + sin(_Time.y * _PulseSpeed) * _Thickness * 0.5;
    float center = 1 - smoothstep(centerSize, centerSize + 0.01, length(uv));
                
                // ��� ��� ����
    float decoration = saturate(circleEffect + mainHand + secondHand + center);
                
                // �۷ο� ȿ�� �߰� (�� ���� ���)
    float glowSize = _Thickness * (3.0 + sin(_Time.y * _PulseSpeed * 0.8) * 1.0);
    float decorationGlow = glow(decoration, _GlowIntensity, glowSize);
                
                // ���� ��� - ���� ����� ���� ���� ����
    fixed4 mainColor = _Color * i.color;
    fixed4 secColor = _SecondColor * i.color;
                
                // �ð��� ���� ���� ��ȭ (������)
    float colorBlend = (sin(_Time.y * _PulseSpeed * 0.2) + 1.0) * 0.5;
    fixed4 finalColor = lerp(mainColor, secColor, colorBlend);
                
                // ���� ���� (��ü ������ ����)
    finalColor.a *= saturate(decoration + decorationGlow) * _Opacity;
                
                // �ؽ��Ŀ� ����
    fixed4 texColor = tex2D(_MainTex, i.uv);
    return finalColor * texColor;
}
            ENDCG
        }
    }
    
Fallback"UI/Default"
}