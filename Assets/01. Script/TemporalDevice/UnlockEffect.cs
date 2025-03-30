using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnlockEffect : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private Image deviceIcon;
    [SerializeField] private Image runeImage;
    [SerializeField] private TextMeshProUGUI unlockText;

    [Header("불타는 효과 설정")]
    [SerializeField] private Material burnMaterial;
    [SerializeField] private float burnSpeed = 0.7f;
    [SerializeField] private float burnSize = 0.15f;
    [SerializeField] private Color burnColor1 = new Color(1f, 0f, 0f);
    [SerializeField] private Color burnColor2 = new Color(1f, 0.7f, 0f);
    [SerializeField] private float noiseScale = 20f;
    [SerializeField] private float flameNoiseScale = 30f;
    [SerializeField] private float flameSpeed = 5f;
    [SerializeField] private float glowIntensity = 2f;
    [SerializeField] private float rotationSpeed = 30f;

    [Header("사운드")]
    [SerializeField] private AudioSource unlockSound;
    [SerializeField] private AudioSource burnSound;

    private Material burnMaterialInstance;
    private bool isEffectPlaying = false;

    private void Awake()
    {
        // 초기 상태 설정
        gameObject.SetActive(false);
    }

    // 효과 실행
    public void PlayEffect(string deviceName, Sprite icon)
    {
        if (isEffectPlaying) return;

        gameObject.SetActive(true);
        isEffectPlaying = true;

        // 초기 상태 설정
        if (backgroundPanel != null)
            backgroundPanel.color = new Color(0, 0, 0, 0);

        if (deviceIcon != null)
        {
            deviceIcon.sprite = icon;
            deviceIcon.rectTransform.localScale = Vector3.zero;
            deviceIcon.color = new Color(1, 1, 1, 0);
        }

        if (unlockText != null)
        {
            unlockText.text = deviceName + " 해금!";
            unlockText.alpha = 0;
        }

        // 룬 이미지 준비
        PrepareRuneImage();

        // 효과 코루틴 시작
        StartCoroutine(UnlockEffectSequence());
    }

    private void PrepareRuneImage()
    {
        if (runeImage == null || burnMaterial == null) return;

        // 머티리얼 인스턴스 생성
        burnMaterialInstance = new Material(burnMaterial);
        runeImage.material = burnMaterialInstance;

        // 프로시저럴 셰이더 파라미터 설정
        burnMaterialInstance.SetFloat("_DissolveAmount", 0);
        burnMaterialInstance.SetFloat("_BurnSize", burnSize);
        burnMaterialInstance.SetColor("_BurnColor1", burnColor1);
        burnMaterialInstance.SetColor("_BurnColor2", burnColor2);
        burnMaterialInstance.SetFloat("_NoiseScale", noiseScale);
        burnMaterialInstance.SetFloat("_FlameNoiseScale", flameNoiseScale);
        burnMaterialInstance.SetFloat("_FlameSpeed", flameSpeed);
        burnMaterialInstance.SetFloat("_EmissionIntensity", glowIntensity);

        // 룬 이미지 초기 설정
        runeImage.color = Color.white;
        runeImage.transform.localScale = Vector3.one;
        runeImage.gameObject.SetActive(true);
    }

    private IEnumerator UnlockEffectSequence()
    {
        // 사운드 재생
        if (unlockSound != null)
            unlockSound.Play();

        // 배경 페이드 인
        float elapsed = 0f;
        while (elapsed < 0.6f)
        {
            float t = elapsed / 0.6f;

            if (backgroundPanel != null)
                backgroundPanel.color = new Color(0, 0, 0, Mathf.Lerp(0, 0.8f, t));

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 룬 회전 및 발광 효과 시작
        StartCoroutine(RuneGlowEffect());

        // 룬 표시 대기
        yield return new WaitForSeconds(1.0f);

        // 불타는 효과 재생
        if (burnSound != null)
            burnSound.Play();

        // 룬 불타는 효과 시작
        StartCoroutine(RuneBurningEffect());

        // 타오르는 동안 대기
        yield return new WaitForSeconds(1.5f);

        // 아이콘 페이드 인 + 확대
        elapsed = 0f;
        while (elapsed < 0.8f)
        {
            float t = elapsed / 0.8f;
            float scale = Mathf.SmoothStep(0, 1f, t);
            float alpha = Mathf.SmoothStep(0, 1f, t);

            if (deviceIcon != null)
            {
                deviceIcon.rectTransform.localScale = new Vector3(scale, scale, scale);
                deviceIcon.color = new Color(1, 1, 1, alpha);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 텍스트 페이드 인
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            float t = elapsed / 0.5f;

            if (unlockText != null)
                unlockText.alpha = Mathf.Lerp(0, 1, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 효과 지속 대기
        yield return new WaitForSeconds(2.0f);

        // 모든 요소 페이드 아웃
        elapsed = 0f;
        while (elapsed < 1.0f)
        {
            float t = elapsed / 1.0f;

            if (backgroundPanel != null)
                backgroundPanel.color = new Color(0, 0, 0, Mathf.Lerp(0.8f, 0, t));

            if (deviceIcon != null)
                deviceIcon.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, t));

            if (unlockText != null)
                unlockText.alpha = Mathf.Lerp(1, 0, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 효과 완료 및 비활성화
        isEffectPlaying = false;
        gameObject.SetActive(false);
    }

    private IEnumerator RuneGlowEffect()
    {
        if (runeImage == null || burnMaterialInstance == null) yield break;

        float elapsed = 0;
        float duration = 3.0f;

        while (elapsed < duration && runeImage.gameObject.activeSelf)
        {
            // 회전
            runeImage.rectTransform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            // 빛나는 효과 (펄스)
            float pulseAmount = (Mathf.Sin(elapsed * 3f) * 0.5f + 0.5f) * glowIntensity;
            burnMaterialInstance.SetFloat("_EmissionIntensity", pulseAmount + 1.5f);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator RuneBurningEffect()
    {
        if (runeImage == null || burnMaterialInstance == null) yield break;

        float dissolveAmount = 0;

        // 점점 더 빠르게 타오르는 효과
        while (dissolveAmount < 1)
        {
            // 비선형적인 속도로 타오르게 (처음엔 천천히, 나중엔 빠르게)
            float speed = burnSpeed * (0.1f + dissolveAmount);
            dissolveAmount += Time.deltaTime * speed;

            // 디졸브 값 설정
            burnMaterialInstance.SetFloat("_DissolveAmount", dissolveAmount);

            // 타오를수록 불 크기 증가
            float currentBurnSize = burnSize * (1f + dissolveAmount * 0.5f);
            burnMaterialInstance.SetFloat("_BurnSize", currentBurnSize);

            yield return null;
        }

        // 완전히 사라지면 비활성화
        runeImage.gameObject.SetActive(false);
    }
}