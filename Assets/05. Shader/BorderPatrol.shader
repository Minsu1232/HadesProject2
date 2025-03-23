Shader"Custom/BorderPatrol"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _PatrolColor ("Patrol Color", Color) = (1,1,1,1)
        _PatrolSpeed ("Patrol Speed", Range(0.1, 10)) = 0.8
        _PatrolWidth ("Patrol Width", Range(0.01, 0.5)) = 0.4
        _OutlineWidth ("Outline Width", Range(0, 0.3)) = 0.2
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _PatrolGlow ("Patrol Glow", Range(1, 5)) = 3.0
        _FlashSpeed ("Flash Speed", Range(0, 10)) = 2.0
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
    float2 screenPos : TEXCOORD1;
};
            
sampler2D _MainTex;
float4 _MainTex_TexelSize;
float4 _PatrolColor;
float _PatrolSpeed;
float _PatrolWidth;
float _OutlineWidth;
float4 _OutlineColor;
float _PatrolGlow;
float _FlashSpeed;
            
v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    o.color = v.color;
    o.screenPos = ComputeScreenPos(o.vertex);
    return o;
}
            
            // 외곽선 검출 함수
float detectEdge(sampler2D tex, float2 uv, float2 texelSize, float thickness)
{
    float2 thicknessStep = texelSize * thickness * 30.0;
                
                // 원본 알파
    float alpha = tex2D(tex, uv).a;
                
                // 주변 샘플링
    float up = tex2D(tex, uv + float2(0, thicknessStep.y)).a;
    float down = tex2D(tex, uv - float2(0, thicknessStep.y)).a;
    float right = tex2D(tex, uv + float2(thicknessStep.x, 0)).a;
    float left = tex2D(tex, uv - float2(thicknessStep.x, 0)).a;
                
    float upLeft = tex2D(tex, uv + float2(-thicknessStep.x, thicknessStep.y)).a;
    float upRight = tex2D(tex, uv + float2(thicknessStep.x, thicknessStep.y)).a;
    float downLeft = tex2D(tex, uv + float2(-thicknessStep.x, -thicknessStep.y)).a;
    float downRight = tex2D(tex, uv + float2(thicknessStep.x, -thicknessStep.y)).a;
                
                // 외곽선 계산 (현재 픽셀이 투명하고 주변에 불투명한 픽셀이 있는 경우)
    float outline = max(max(max(up, down), max(right, left)),
                                   max(max(upLeft, upRight), max(downLeft, downRight))) - alpha;
                
                // 내부 영역 (현재 픽셀이 불투명한 경우)
    float innerArea = alpha;
                
                // 외곽선 마스크 반환
    return outline;
}
            
fixed4 frag(v2f i) : SV_Target
{
    fixed4 col = tex2D(_MainTex, i.uv);
    float2 texelSize = _MainTex_TexelSize.xy;
                
                // 외곽선 검출
    float outline = detectEdge(_MainTex, i.uv, texelSize, _OutlineWidth);
                
                // 패트롤 효과 (시간에 따라 움직이는 강조 효과)
    float time = _Time.y * _PatrolSpeed;
    float patrolPhase = frac(time); // 0~1 범위로 순환
                
                // UV 좌표를 원형 좌표로 변환
    float2 centeredUV = i.uv - 0.5;
    float angle = atan2(centeredUV.y, centeredUV.x); // -PI ~ PI 범위
    angle = (angle + 3.14159) / 6.28318; // 0~1 범위로 정규화
                
                // 각도 기반 패트롤 효과 (외곽선을 따라 움직임)
    float angleDiff = abs(angle - patrolPhase);
    angleDiff = min(angleDiff, 1.0 - angleDiff); // 최단 거리 계산
    float patrolValue = step(angleDiff, _PatrolWidth) * outline;
                
                // 깜빡임 효과 추가
    float flash = 0.75 + 0.25 * sin(_Time.y * _FlashSpeed);
                
                // 외곽선 색상 계산
    fixed4 outlineColor = _OutlineColor * outline;
                
                // 패트롤 효과 색상에 발광 적용
    fixed4 patrolColor = _PatrolColor * patrolValue * _PatrolGlow * flash;
                
                // 최종 색상 계산
                // 1. 원본 이미지
                // 2. 외곽선 적용
                // 3. 패트롤 효과 적용
    col.rgb = lerp(col.rgb, outlineColor.rgb, outline * _OutlineColor.a);
    col.rgb = lerp(col.rgb, patrolColor.rgb, patrolValue * _PatrolColor.a);
                
                // 알파값은 원본 알파와 효과 알파 중 최대값
    col.a = max(col.a, max(outline * _OutlineColor.a, patrolValue * _PatrolColor.a));
                
    return col;
}
            ENDCG
        }
    }
}