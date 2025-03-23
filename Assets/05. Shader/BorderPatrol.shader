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
            
            // �ܰ��� ���� �Լ�
float detectEdge(sampler2D tex, float2 uv, float2 texelSize, float thickness)
{
    float2 thicknessStep = texelSize * thickness * 30.0;
                
                // ���� ����
    float alpha = tex2D(tex, uv).a;
                
                // �ֺ� ���ø�
    float up = tex2D(tex, uv + float2(0, thicknessStep.y)).a;
    float down = tex2D(tex, uv - float2(0, thicknessStep.y)).a;
    float right = tex2D(tex, uv + float2(thicknessStep.x, 0)).a;
    float left = tex2D(tex, uv - float2(thicknessStep.x, 0)).a;
                
    float upLeft = tex2D(tex, uv + float2(-thicknessStep.x, thicknessStep.y)).a;
    float upRight = tex2D(tex, uv + float2(thicknessStep.x, thicknessStep.y)).a;
    float downLeft = tex2D(tex, uv + float2(-thicknessStep.x, -thicknessStep.y)).a;
    float downRight = tex2D(tex, uv + float2(thicknessStep.x, -thicknessStep.y)).a;
                
                // �ܰ��� ��� (���� �ȼ��� �����ϰ� �ֺ��� �������� �ȼ��� �ִ� ���)
    float outline = max(max(max(up, down), max(right, left)),
                                   max(max(upLeft, upRight), max(downLeft, downRight))) - alpha;
                
                // ���� ���� (���� �ȼ��� �������� ���)
    float innerArea = alpha;
                
                // �ܰ��� ����ũ ��ȯ
    return outline;
}
            
fixed4 frag(v2f i) : SV_Target
{
    fixed4 col = tex2D(_MainTex, i.uv);
    float2 texelSize = _MainTex_TexelSize.xy;
                
                // �ܰ��� ����
    float outline = detectEdge(_MainTex, i.uv, texelSize, _OutlineWidth);
                
                // ��Ʈ�� ȿ�� (�ð��� ���� �����̴� ���� ȿ��)
    float time = _Time.y * _PatrolSpeed;
    float patrolPhase = frac(time); // 0~1 ������ ��ȯ
                
                // UV ��ǥ�� ���� ��ǥ�� ��ȯ
    float2 centeredUV = i.uv - 0.5;
    float angle = atan2(centeredUV.y, centeredUV.x); // -PI ~ PI ����
    angle = (angle + 3.14159) / 6.28318; // 0~1 ������ ����ȭ
                
                // ���� ��� ��Ʈ�� ȿ�� (�ܰ����� ���� ������)
    float angleDiff = abs(angle - patrolPhase);
    angleDiff = min(angleDiff, 1.0 - angleDiff); // �ִ� �Ÿ� ���
    float patrolValue = step(angleDiff, _PatrolWidth) * outline;
                
                // ������ ȿ�� �߰�
    float flash = 0.75 + 0.25 * sin(_Time.y * _FlashSpeed);
                
                // �ܰ��� ���� ���
    fixed4 outlineColor = _OutlineColor * outline;
                
                // ��Ʈ�� ȿ�� ���� �߱� ����
    fixed4 patrolColor = _PatrolColor * patrolValue * _PatrolGlow * flash;
                
                // ���� ���� ���
                // 1. ���� �̹���
                // 2. �ܰ��� ����
                // 3. ��Ʈ�� ȿ�� ����
    col.rgb = lerp(col.rgb, outlineColor.rgb, outline * _OutlineColor.a);
    col.rgb = lerp(col.rgb, patrolColor.rgb, patrolValue * _PatrolColor.a);
                
                // ���İ��� ���� ���Ŀ� ȿ�� ���� �� �ִ밪
    col.a = max(col.a, max(outline * _OutlineColor.a, patrolValue * _PatrolColor.a));
                
    return col;
}
            ENDCG
        }
    }
}