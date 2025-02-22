Shader"Custom/VoronoiVignetteWarpStretched"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VignetteColor ("Vignette Color", Color) = (1,0,0,1)
        _VignetteIntensity ("Vignette Intensity", Range(0,1)) = 0.5

        // Voronoi ����
        _NoiseScale ("Noise Scale", Range(1,100)) = 20
        _VeinSize ("Vein Size (Cell Thickness)", Range(0.0,1.0)) = 0.3

        // �����ڸ� ���Ʈ ����
        _EdgeFadeStart ("Edge Fade Start", Range(0.0,1.0)) = 0.3
        _EdgeFadeEnd ("Edge Fade End", Range(0.0,1.0)) = 0.7

        // �ƹ� ȿ��
        _PulseAmplitude ("Pulse Amplitude", Range(0.0,0.2)) = 0.05
        _PulseSpeed ("Pulse Speed", Range(0.0,5.0)) = 2.0

        // ������ ����(�ְ�) ����
        _WarpFrequency ("Warp Frequency", Range(0.0,10.0)) = 5.0
        _WarpAmplitude ("Warp Amplitude", Range(0.0,1.0)) = 0.3
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

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
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
};

sampler2D _MainTex;

float4 _VignetteColor;
float _VignetteIntensity;

float _NoiseScale;
float _VeinSize;
float _EdgeFadeStart;
float _EdgeFadeEnd;

float _PulseAmplitude;
float _PulseSpeed;

float _WarpFrequency;
float _WarpAmplitude;

            // ������ ������ �� ��ġ�� �����ϱ� ���� �Լ�
float2 random2(float2 p)
{
    return frac(sin(float2(
                    dot(p, float2(127.1, 311.7)),
                    dot(p, float2(269.5, 183.3))
                )) * 43758.5453);
}

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}

fixed4 frag(v2f i) : SV_Target
{
                // ȭ�� �߽ɿ����� �Ÿ�(���Ʈ��)
    float2 coords = i.uv - 0.5;
    float len = length(coords);

                // �ƹ� ȿ��(�ƹ��� 0�̸� ����)
    float pulsate = _PulseAmplitude * sin(_Time.y * _PulseSpeed);

                // UV ������ ����
    float2 scaledUV = i.uv * _NoiseScale + pulsate;

                // -----------------------------
                // ������ ����(Domain Warping) �κ�
                // sin/cos �Լ��� ����� UV�� �߰��� ������
                // -----------------------------
    float warpValX = sin(scaledUV.y * _WarpFrequency + _Time.y);
    float warpValY = cos(scaledUV.x * _WarpFrequency + _Time.y);
    float2 warp = float2(warpValX, warpValY) * _WarpAmplitude;

                // UV ��ǥ�� ��Ʋ����
    scaledUV += warp;

                // Voronoi �Ÿ� ���
    float minDist = 1.0;
    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 neighbor = float2(x, y);
            float2 cell = floor(scaledUV) + neighbor;
            float2 rand = random2(cell);
            float2 pointPos = cell + rand; // �� ���� ������ ��
            float2 diff = pointPos - scaledUV;
            float dist = length(diff);
            minDist = min(minDist, dist);
        }
    }

                // minDist�� �������� '����'�� ����� ����
    float veins = smoothstep(0.0, _VeinSize, minDist);
    float voronoiMask = 1.0 - veins;

                // ȭ�� �����ڸ��� ������ ���Ʈ ȿ���� ���������� ���̵�
    float edgeFade = smoothstep(_EdgeFadeStart, _EdgeFadeEnd, len);

                // ���� ���� = Voronoi ���� ���� * �����ڸ� ���̵� * ����
    float alpha = voronoiMask * edgeFade * _VignetteIntensity;

                // ���� ����
    return float4(_VignetteColor.rgb, alpha);
}
            ENDCG
        }
    }
}