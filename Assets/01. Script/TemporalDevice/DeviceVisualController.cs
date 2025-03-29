using UnityEngine;

// 시간 장치의 시각적 효과를 제어하는 클래스
public class DeviceVisualController : MonoBehaviour
{
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color emissionColor = new Color(0.5f, 0.5f, 1f, 1f);
    [SerializeField] private float emissionIntensity = 2f;

    [SerializeField] private ParticleSystem[] activeParticles;
    [SerializeField] private Light[] activeLights;
    [SerializeField] private AudioSource activationSound;
    [SerializeField] private GameObject[] activeGameObjects;

    [SerializeField] private Renderer[] deviceRenderers;

    [SerializeField] bool isActive = false;

    private void Start()
    {
        // 렌더러를 자동으로 찾지 않았다면 찾기
        if (deviceRenderers == null || deviceRenderers.Length == 0)
        {
            deviceRenderers = GetComponentsInChildren<Renderer>();
        }

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

        // 초기 상태 설정
        SetActiveState(isActive);
    }

    // 장치의 활성 상태 설정
    public void SetActiveState(bool active)
    {
        isActive = active;

        // 렌더러 색상 및 이미션 설정
        foreach (var renderer in deviceRenderers)
        {
            foreach (Material mat in renderer.materials)
            {
                // 기본 색상 변경
                mat.color = active ? activeColor : inactiveColor;

                // 이미션 설정
                if (mat.HasProperty("_EmissionColor"))
                {
                    if (active)
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
                    }
                    else
                    {
                        mat.DisableKeyword("_EMISSION");
                    }
                }
            }
        }

        // 파티클 시스템 제어
        foreach (var ps in activeParticles)
        {
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
            light.enabled = active;
        }

        // 활성화 전용 게임오브젝트 제어
        foreach (var go in activeGameObjects)
        {
            go.SetActive(active);
        }

        // 활성화 사운드 재생
        if (active && activationSound != null)
        {
            activationSound.Play();
        }
    }

    // 활성화 애니메이션 재생 (외부에서 호출 가능)
    public void PlayActivationAnimation()
    {
        if (!isActive) return;

        // 추가 활성화 애니메이션 재생
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