using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeviceVisualController : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color emissionColor = new Color(0.5f, 0.5f, 1f, 1f);
    [SerializeField] private float emissionIntensity = 2f;

    [Header("별자리 효과")]
    [SerializeField] private Transform starsTransform; // Stars 오브젝트
    [SerializeField] private Transform lineTransform; // Line 오브젝트
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float lineBrightness = 2f; // 선 밝기 조절

    [Header("효과 컴포넌트")]
    [SerializeField] private ParticleSystem[] activeParticles;
    [SerializeField] private Light[] activeLights;
    [SerializeField] private AudioSource activationSound;
    [SerializeField] private GameObject[] activeGameObjects;

    [SerializeField] private Renderer[] deviceRenderers;

    [SerializeField] bool isActive = false;

    private Coroutine animationCoroutine;
    private Renderer starsRenderer;
    private Renderer lineRenderer;
    private Vector3 originalStarsScale;
    private Vector3 originalLineScale;

    private void Start()
    {
        TemporalDeviceManager.Instance.OnVillageEnter();
        // Stars와 Line 오브젝트 찾기
        if (starsTransform == null || lineTransform == null)
        {
            foreach (Transform child in transform)
            {
                if (child.name.Equals("Stars"))
                {
                    starsTransform = child;
                    starsRenderer = child.GetComponent<Renderer>();
                }
                else if (child.name.Equals("Line"))
                {
                    lineTransform = child;
                    lineRenderer = child.GetComponent<Renderer>();
                }
            }
        }

        // 렌더러가 직접 할당되지 않았다면 가져오기
        if (starsRenderer == null && starsTransform != null)
            starsRenderer = starsTransform.GetComponent<Renderer>();

        if (lineRenderer == null && lineTransform != null)
            lineRenderer = lineTransform.GetComponent<Renderer>();

        // 파티클 시스템이 없다면 찾기
        if (activeParticles == null || activeParticles.Length == 0)
        {
            activeParticles = GetComponentsInChildren<ParticleSystem>();
        }

        // 라이트가 없다면 찾기
        if (activeLights == null || activeLights.Length == 0)
        {
            activeLights = GetComponentsInChildren<Light>();
        }

        // 디바이스 렌더러가 설정되지 않았다면 모든 렌더러 찾기
        if (deviceRenderers == null || deviceRenderers.Length == 0)
        {
            deviceRenderers = GetComponentsInChildren<Renderer>();
        }

        // 원래 스케일 저장 및 로그 추가
        if (starsTransform != null)
        {
            originalStarsScale = starsTransform.localScale;
            Debug.Log($"Stars 원래 스케일: {originalStarsScale}");

            // 스케일이 0이면 기본값으로 설정
            if (originalStarsScale.magnitude < 0.001f)
            {
                Debug.LogWarning("Stars 원래 스케일이 0에 가까워 기본값 설정");
                originalStarsScale = Vector3.one;
            }
        }

        if (lineTransform != null)
        {
            originalLineScale = lineTransform.localScale;
            Debug.Log($"Line 원래 스케일: {originalLineScale}");

            // 스케일이 0이면 기본값으로 설정
            if (originalLineScale.magnitude < 0.001f)
            {
                Debug.LogWarning("Line 원래 스케일이 0에 가까워 기본값 설정");
                originalLineScale = Vector3.one;
            }
        }

        // 초기 상태 설정
        SetActiveState(isActive);


    }

    public void SetActiveState(bool active)
    {
        isActive = active;

        // 게임오브젝트가 비활성화 상태면 애니메이션을 시작하지 않음
        if (!gameObject.activeInHierarchy) return;

        // 이전 애니메이션 코루틴 중지
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        // 렌더러 효과 적용
        ApplyRendererEffects();

        // 파티클 시스템 제어
        foreach (var ps in activeParticles)
        {
            if (ps == null) continue;

            if (active)
            {
                if (!ps.isPlaying) ps.Play();
            }
            else
            {
                if (ps.isPlaying) ps.Stop();
            }
        }

        // 라이트 제어
        foreach (var light in activeLights)
        {
            if (light == null) continue;
            light.enabled = active;
        }

        // 활성화 전용 게임오브젝트 제어
        foreach (var go in activeGameObjects)
        {
            if (go == null) continue;
            go.SetActive(active);
        }

        // 활성화 사운드 재생
        if (active && activationSound != null)
        {
            activationSound.Play();
        }

        // 애니메이션 시작
        if (active)
        {
            animationCoroutine = StartCoroutine(AnimateStarsAndLines());
        }
        else
        {
            // 비활성화 시 초기 상태로 복원
            ResetObjectsState();
        }
    }

    // 렌더러 효과 적용
    private void ApplyRendererEffects()
    {
        foreach (Renderer renderer in deviceRenderers)
        {
            if (renderer == null) continue;

            foreach (Material mat in renderer.materials)
            {
                // 기본 색상 변경
                mat.color = isActive ? activeColor : inactiveColor;

                // 발광 효과 설정
                if (mat.HasProperty("_EmissionColor"))
                {
                    if (isActive)
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
                    }
                    else
                    {
                        mat.DisableKeyword("_EMISSION");
                    }
                }

                // FlatKit 셰이더 특화 설정
                if (mat.HasProperty("_RimEnabled"))
                {
                    mat.SetInt("_RimEnabled", isActive ? 1 : 0);

                    if (isActive && mat.HasProperty("_FlatRimColor"))
                    {
                        mat.SetColor("_FlatRimColor", emissionColor * 1.5f);
                        mat.SetFloat("_FlatRimSize", 0.6f);
                        mat.SetFloat("_FlatRimEdgeSmoothness", 0.3f);
                    }
                }

                // 스펙큘러 효과
                if (mat.HasProperty("_SpecularEnabled"))
                {
                    mat.SetInt("_SpecularEnabled", isActive ? 1 : 0);

                    if (isActive && mat.HasProperty("_FlatSpecularColor"))
                    {
                        mat.SetColor("_FlatSpecularColor", emissionColor);
                        mat.SetFloat("_FlatSpecularSize", 0.5f);
                        mat.SetFloat("_FlatSpecularEdgeSmoothness", 0.3f);
                    }
                }
            }
        }
    }

    // 별들과 라인 애니메이션
    // 별들과 라인 애니메이션
    private IEnumerator AnimateStarsAndLines()
    {
        if (starsTransform == null && lineTransform == null) yield break;

        // 최상위 Transform 저장 (별자리 전체)
        Transform constellationTransform = transform;
        Vector3 originalConstellationScale = constellationTransform.localScale;

        float time = 0f;

        while (isActive && gameObject.activeInHierarchy)
        {
            time += Time.deltaTime;

            // 전체 별자리에 대한 단일 맥동 효과
            float pulseFactor = (Mathf.Sin(time * pulseSpeed) * 0.05f) + 1.0f; // 맥동 폭 줄임

            // 전체 별자리 스케일 애니메이션
            constellationTransform.localScale = originalConstellationScale * pulseFactor;

            // 별자리 전체 회전 (기존 별 회전 속도와 동일하게)
            constellationTransform.Rotate(Vector3.up, rotationSpeed * 0.5f * Time.deltaTime);

            // 머티리얼 발광 효과 업데이트
            float brightness = 0.8f + 0.4f * (Mathf.Sin(time * 1.2f) * 0.5f + 0.5f);

            if (starsRenderer != null && starsRenderer.material.HasProperty("_EmissionColor"))
            {
                starsRenderer.material.SetColor("_EmissionColor", emissionColor * emissionIntensity * brightness);
            }

            if (lineRenderer != null && lineRenderer.material.HasProperty("_EmissionColor"))
            {
                lineRenderer.material.SetColor("_EmissionColor", emissionColor * lineBrightness * brightness);
            }

            yield return null;
        }

        // 게임오브젝트가 비활성화되면 초기 상태로 복원
        constellationTransform.localScale = originalConstellationScale;

        // 회전도 초기화가 필요할 수 있음
        if (!isActive)
        {
            constellationTransform.rotation = Quaternion.identity;
        }
    }

    // 초기 상태로 복원
    private void ResetObjectsState()
    {
        // Stars 오브젝트 복원
        if (starsTransform != null)
        {
            starsTransform.localScale = originalStarsScale;
            starsTransform.rotation = Quaternion.identity;
        }

        // Line 오브젝트 복원
        if (lineTransform != null)
        {
            lineTransform.localScale = originalLineScale;
        }
    }

    // 활성화 애니메이션 재생 (외부 호출용)
    public void PlayActivationAnimation()
    {
        if (!isActive) return;

        // 활성화 애니메이션 재생
        Animation anim = GetComponent<Animation>();
        if (anim != null && anim.clip != null)
        {
            anim.Play();
        }

        // 활성화 사운드 재생
        if (activationSound != null)
        {
            activationSound.Play();
        }
    }
}