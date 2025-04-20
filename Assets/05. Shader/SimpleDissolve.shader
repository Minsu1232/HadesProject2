Shader"Custom/SimpleDissolveUnlit"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        
        [Space(10)]
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _NoiseScale ("Noise Scale", Range(1, 50)) = 20
        _FlameSpeed ("Flame Speed", Range(0, 10)) = 5.0
        
        _BurnColor1 ("Burn Color 1", Color) = (1, 0.2, 0, 1)
        _BurnColor2 ("Burn Color 2", Color) = (1, 0.7, 0, 1)
        _BurnSize ("Burn Edge Size", Range(0, 0.3)) = 0.15
        _EmissionStrength ("Emission Strength", Range(0, 10)) = 3.0
    }
    
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
LOD 100
        Blend
SrcAlpha OneMinusSrcAlpha

ZWrite On

Cull Back
        
        Pass
        {
Name"Unlit"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
struct Attributes
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;
};
            
struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;
    float fogFactor : TEXCOORD1;
};
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
float4 _MainTex_ST;
half4 _Color;
float _DissolveAmount;
float _NoiseScale;
float _FlameSpeed;
half4 _BurnColor1;
half4 _BurnColor2;
float _BurnSize;
float _EmissionStrength;
            CBUFFER_END
            
            // 노이즈 함수들
float2 hash22(float2 p)
{
    p = float2(dot(p, float2(127.1, 311.7)),
                          dot(p, float2(269.5, 183.3)));
    return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
}
            
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
            
Varyings vert(Attributes input)
{
    Varyings output = (Varyings) 0;
                
    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    output.positionCS = vertexInput.positionCS;
    output.uv = TRANSFORM_TEX(input.uv, _MainTex);
    output.color = input.color * _Color;
    output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
    return output;
}
            
half4 frag(Varyings input) : SV_Target
{
                // 디졸브 노이즈 계산
    float2 noiseCoord = input.uv * _NoiseScale;
    noiseCoord += _Time.y * _FlameSpeed * 0.1;
    float noiseValue = fbm(noiseCoord);
                
                // 디졸브 임계값 계산
    float dissolveThreshold = _DissolveAmount;
                
                // 디졸브 클리핑
    clip(noiseValue - dissolveThreshold);
                
                // 불타는 테두리 효과 계산
    float burnEdge = 1.0 - saturate((noiseValue - dissolveThreshold) / _BurnSize);
                
                // 기본 텍스처 샘플링
    half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    half4 finalColor = texColor * input.color;
                
                // 불타는 테두리 효과 적용
    if (burnEdge > 0.01)
    {
                    // 불타는 효과 색상 그라데이션
        half4 burnColor = lerp(_BurnColor1, _BurnColor2, pow(burnEdge, 0.5));
                    
                    // 테두리가 있는 영역에 불타는 효과 적용
        finalColor.rgb = lerp(finalColor.rgb, burnColor.rgb, burnEdge * step(0.01, burnEdge));
                    
                    // 불타는 테두리에 발광 효과 추가
        finalColor.rgb += burnColor.rgb * _EmissionStrength * burnEdge;
    }
                
                // 포그 적용
    finalColor.rgb = MixFog(finalColor.rgb, input.fogFactor);
                
    return finalColor;
}
            ENDHLSL
        }
    }
    
Fallback"Universal Render Pipeline/Unlit"
}