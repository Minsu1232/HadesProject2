Shader"FlatKit/Stylized Surface With Outline Dissolve"
{
    Properties
    {
        [MainColor] _BaseColor ("Color", Color) = (1,1,1,1)

        [Space(10)]
        [KeywordEnum(None, Single, Steps, Curve)]_CelPrimaryMode("Cel Shading Mode", Float) = 1
        _ColorDim ("[_CELPRIMARYMODE_SINGLE]Color Shaded", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _ColorDimSteps ("[_CELPRIMARYMODE_STEPS]Color Shaded", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _ColorDimCurve ("[_CELPRIMARYMODE_CURVE]Color Shaded", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _SelfShadingSize ("[_CELPRIMARYMODE_SINGLE]Self Shading Size", Range(0, 1)) = 0.5
        _ShadowEdgeSize ("[_CELPRIMARYMODE_SINGLE]Shadow Edge Size", Range(0, 0.5)) = 0.05
        _Flatness ("[_CELPRIMARYMODE_SINGLE]Localized Shading", Range(0, 1)) = 1.0

        [IntRange]_CelNumSteps ("[_CELPRIMARYMODE_STEPS]Number Of Steps", Range(1, 10)) = 3.0
        _CelStepTexture ("[_CELPRIMARYMODE_STEPS][LAST_PROP_STEPS]Cel steps", 2D) = "black" {}
        _CelCurveTexture ("[_CELPRIMARYMODE_CURVE][LAST_PROP_CURVE]Ramp", 2D) = "black" {}

        [Space(10)]
        [Toggle(DR_CEL_EXTRA_ON)] _CelExtraEnabled("Enable Extra Cel Layer", Int) = 0
        _ColorDimExtra ("[DR_CEL_EXTRA_ON]Color Shaded", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _SelfShadingSizeExtra ("[DR_CEL_EXTRA_ON]Self Shading Size", Range(0, 1)) = 0.6
        _ShadowEdgeSizeExtra ("[DR_CEL_EXTRA_ON]Shadow Edge Size", Range(0, 0.5)) = 0.05
        _FlatnessExtra ("[DR_CEL_EXTRA_ON]Localized Shading", Range(0, 1)) = 1.0

        [Space(10)]
        [Toggle(DR_SPECULAR_ON)] _SpecularEnabled("Enable Specular", Int) = 0
        [HDR] _FlatSpecularColor("[DR_SPECULAR_ON]Specular Color", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _FlatSpecularSize("[DR_SPECULAR_ON]Specular Size", Range(0.0, 1.0)) = 0.1
        _FlatSpecularEdgeSmoothness("[DR_SPECULAR_ON]Specular Edge Smoothness", Range(0.0, 1.0)) = 0

        [Space(10)]
        [Toggle(DR_RIM_ON)] _RimEnabled("Enable Rim", Int) = 0
        [HDR] _FlatRimColor("[DR_RIM_ON]Rim Color", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _FlatRimLightAlign("[DR_RIM_ON]Light Align", Range(0.0, 1.0)) = 0
        _FlatRimSize("[DR_RIM_ON]Rim Size", Range(0, 1)) = 0.5
        _FlatRimEdgeSmoothness("[DR_RIM_ON]Rim Edge Smoothness", Range(0, 1)) = 0.5

        [Space(10)]
        [Toggle(DR_GRADIENT_ON)] _GradientEnabled("Enable Height Gradient", Int) = 0
        [HDR] _ColorGradient("[DR_GRADIENT_ON]Gradient Color", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _GradientCenterX("[DR_GRADIENT_ON]Center X", Float) = 0
        _GradientCenterY("[DR_GRADIENT_ON]Center Y", Float) = 0
        _GradientSize("[DR_GRADIENT_ON]Size", Float) = 10.0
        _GradientAngle("[DR_GRADIENT_ON]Gradient Angle", Range(0, 360)) = 0

        [Space(10)]
        [Toggle(DR_VERTEX_COLORS_ON)] _VertexColorsEnabled("Enable Vertex Colors", Int) = 0

        _LightContribution("[FOLDOUT(Advanced Lighting){5}]Light Color Contribution", Range(0, 1)) = 0
        _LightFalloffSize("Light edge width (point / spot)", Range(0, 1)) = 0

        // Used to provide light direction to cel shading if all light in the scene is baked.
        [Space(5)]
        [Toggle(DR_ENABLE_LIGHTMAP_DIR)]_OverrideLightmapDir("Override light direction", Int) = 0
        _LightmapDirectionPitch("[DR_ENABLE_LIGHTMAP_DIR]Pitch", Range(0, 360)) = 0
        _LightmapDirectionYaw("[DR_ENABLE_LIGHTMAP_DIR]Yaw", Range(0, 360)) = 0
        [HideInInspector] _LightmapDirection("Direction", Vector) = (0, 1, 0, 0)

        [KeywordEnum(None, Multiply, Color)] _UnityShadowMode ("[FOLDOUT(Unity Built-in Shadows){4}]Mode", Float) = 0
        _UnityShadowPower("[_UNITYSHADOWMODE_MULTIPLY]Power", Range(0, 1)) = 0.2
        _UnityShadowColor("[_UNITYSHADOWMODE_COLOR]Color", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _UnityShadowSharpness("Sharpness", Range(1, 10)) = 1.0

        [MainTexture] _BaseMap("[FOLDOUT(Texture maps){6}]Albedo", 2D) = "white" {}
        [Space][KeywordEnum(Multiply, Add)]_TextureBlendingMode("[_]Blending Mode", Float) = 0
        [Space]_TextureImpact("[_]Texture Impact", Range(0, 1)) = 1.0
        [Space(20)]_BumpMap ("Normal Map", 2D) = "bump" {}
        _EmissionMap ("Emission Map", 2D) = "black" {}
        [HDR]_EmissionColor("Emission Color", Color) = (1, 1, 1, 1)

        [HideInInspector] _Cutoff ("Base Alpha cutoff", Range (0, 1)) = .5

        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0

        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0

        // --------------------- OUTLINE PROPS -----------------------
        _OutlineColor("[FOLDOUT(Outline){5}]Color", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
        _OutlineWidth("Width", Float) = 0.01
        _OutlineScale("Scale", Float) = 1.0
        _OutlineDepthOffset("Depth Offset", Range(0, 1)) = 0.0
        _CameraDistanceImpact("Camera Distance Impact", Range(0, 1)) = 0.0

        // --------------------- DISSOLVE PROPS -----------------------
        [Space(20)]
        [Toggle(DR_DISSOLVE_ON)] _DissolveEnabled("[FOLDOUT(Dissolve Effect){5}]Enable Dissolve", Int) = 0
        _DissolveAmount("[DR_DISSOLVE_ON]Dissolve Amount", Range(0, 1)) = 0
        _DissolveNoiseScale("[DR_DISSOLVE_ON]Noise Scale", Range(1, 50)) = 20
        _DissolveFlameSpeed("[DR_DISSOLVE_ON]Flame Speed", Range(0, 10)) = 5.0
        
        _BurnColor1("[DR_DISSOLVE_ON]Burn Color 1", Color) = (1, 0.2, 0, 1)
        _BurnColor2("[DR_DISSOLVE_ON]Burn Color 2", Color) = (1, 0.7, 0, 1)
        _BurnSize("[DR_DISSOLVE_ON]Burn Edge Size", Range(0, 0.3)) = 0.15
        _EmissionStrength("[DR_DISSOLVE_ON]Emission Strength", Range(0, 10)) = 3.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"
        }

        // ----------------------------- DISSOLVE PASS -----------------------------
        // URP 파이프라인에 맞게 ForwardLit 패스 추가
        Pass
        {
Name"DissolveForwardLit"
            Tags
{"LightMode" = "UniversalForward"
}

Blend[_SrcBlend] [_DstBlend]
ZWrite[_ZWrite]
Cull[_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // 키워드 정의
            #pragma shader_feature_local DR_DISSOLVE_ON
            
            // URP 키워드
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // 노이즈 함수들 추가
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

            // 주요 변수들
            CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
                
                // 디졸브 속성들
float _DissolveAmount;
float _DissolveNoiseScale;
float _DissolveFlameSpeed;
half4 _BurnColor1;
half4 _BurnColor2;
float _BurnSize;
float _EmissionStrength;
CBUFFER_END
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    float2 lightmapUV : TEXCOORD1;
    float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 normalWS : TEXCOORD2;
    float4 color : COLOR;
    float fogFactor : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

    Varyings vert(Attributes input)
    {
        Varyings output = (Varyings) 0;
                
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
        VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
        output.positionWS = vertexInput.positionWS;
        output.positionCS = vertexInput.positionCS;
        output.normalWS = normalInput.normalWS;
        output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
        output.color = input.color;
        output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
        return output;
    }

    half4 frag(Varyings input) : SV_Target
    {
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 uv = input.uv;
                
                // 디졸브 노이즈 계산
#if defined(DR_DISSOLVE_ON)
                    float2 noiseCoord = uv * _DissolveNoiseScale;
                    noiseCoord += _Time.y * _DissolveFlameSpeed * 0.1;
                    float noiseValue = fbm(noiseCoord);
                    
                    // 디졸브 임계값 계산
                    float dissolveThreshold = _DissolveAmount;
                    
                    // 디졸브 클리핑
                    clip(noiseValue - dissolveThreshold);
                    
                    // 불타는 테두리 효과 계산
                    float burnEdge = 1.0 - saturate((noiseValue - dissolveThreshold) / _BurnSize);
#endif

                // 기본 텍스처 샘플링
        half4 albedoAlpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
        half4 finalColor = albedoAlpha * _BaseColor;
                
                // 디졸브 효과 적용
#if defined(DR_DISSOLVE_ON)
                    if (burnEdge > 0.01) {
                        // 불타는 효과 색상 그라데이션
                        half4 burnColor = lerp(_BurnColor1, _BurnColor2, pow(burnEdge, 0.5));
                        
                        // 테두리가 있는 영역에 불타는 효과 적용
                        finalColor.rgb = lerp(finalColor.rgb, burnColor.rgb, burnEdge * step(0.01, burnEdge));
                        
                        // 불타는 테두리에 발광 효과 추가
                        finalColor.rgb += burnColor.rgb * _EmissionStrength * burnEdge;
                    }
#endif
                
                // 포그 적용
        finalColor.rgb = MixFog(finalColor.rgb, input.fogFactor);
                
        return finalColor;
    }
            ENDHLSL
}

        // 기존 아웃라인 패스 활용
        UsePass"FlatKit/Stylized Surface With Outline/OUTLINE"
        
        // 그림자 캐스팅 패스 추가
        Pass
        {
Name"ShadowCaster"
            Tags
{"LightMode" = "ShadowCaster"
}

ZWrite On

ZTest LEqual

Cull[_Cull]
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #pragma shader_feature_local DR_DISSOLVE_ON
            
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            // 노이즈 함수들 (위와 동일)
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

            CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
float _DissolveAmount;
float _DissolveNoiseScale;
float _DissolveFlameSpeed;
float _BurnSize;
            CBUFFER_END
            
struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv : TEXCOORD0;
    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
    float3 _LightDirection;

    Varyings ShadowPassVertex(Attributes input)
    {
        Varyings output = (Varyings) 0;
                
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                
        float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
        float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
        float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                
#if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
        positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

        output.positionCS = positionCS;
        return output;
    }

    half4 ShadowPassFragment(Varyings input) : SV_TARGET
    {
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
#if defined(DR_DISSOLVE_ON)
                    float2 noiseCoord = input.uv * _DissolveNoiseScale;
                    noiseCoord += _Time.y * _DissolveFlameSpeed * 0.1;
                    float noiseValue = fbm(noiseCoord);
                    
                    float dissolveThreshold = _DissolveAmount;
                    clip(noiseValue - dissolveThreshold);
#endif
                
        return 0;
    }
            ENDHLSL
}
        
        // DepthOnly 패스 추가
        Pass
        {
Name"DepthOnly"
            Tags
{"LightMode" = "DepthOnly"
}

ZWrite On

ColorMask 0

Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            
            #pragma shader_feature_local DR_DISSOLVE_ON

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            // 노이즈 함수들 (위와 동일)
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

            CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
float _DissolveAmount;
float _DissolveNoiseScale;
float _DissolveFlameSpeed;
            CBUFFER_END
            
struct Attributes
{
    float4 position : POSITION;
    float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv : TEXCOORD0;
    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

    Varyings DepthOnlyVertex(Attributes input)
    {
        Varyings output = (Varyings) 0;
                
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
        output.positionCS = TransformObjectToHClip(input.position.xyz);
        return output;
    }

    half4 DepthOnlyFragment(Varyings input) : SV_TARGET
    {
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
#if defined(DR_DISSOLVE_ON)
                    float2 noiseCoord = input.uv * _DissolveNoiseScale;
                    noiseCoord += _Time.y * _DissolveFlameSpeed * 0.1;
                    float noiseValue = fbm(noiseCoord);
                    
                    float dissolveThreshold = _DissolveAmount;
                    clip(noiseValue - dissolveThreshold);
#endif
                
        return 0;
    }
            ENDHLSL
}
    }

    Fallback"Universal Render Pipeline/Lit"

}