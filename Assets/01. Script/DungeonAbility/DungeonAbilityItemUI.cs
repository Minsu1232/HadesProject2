using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class DungeonAbilityItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 요소")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image abilityIconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI levelAndRarityText; // 레벨과 희귀도를 함께 표시

    [Header("애니메이션 설정")]
    [SerializeField] private float hoverScaleAmount = 1.05f;
    [SerializeField] private float hoverScaleDuration = 0.2f;
    [SerializeField] private float pulseDuration = 1.5f;
    [SerializeField] private float pulseScaleAmount = 1.02f;

    [Header("효과음")]
    [SerializeField] private AudioClip hoverSound;

    private DungeonAbility ability;
    private AudioSource audioSource;
    private Vector3 originalScale;
    private Tween currentHoverTween;
    private Tween pulseTween;

    private void Awake()
    {
        // 오디오 소스 컴포넌트 가져오거나 생성
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        // 펄스 애니메이션 시작 (희귀도가 높은 아이템만)
        StartPulseEffect();
    }

    private void OnDisable()
    {
        // 모든 애니메이션 정리
        if (currentHoverTween != null && currentHoverTween.IsActive())
            currentHoverTween.Kill();

        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();
    }

    // 능력 정보 설정
    public void SetupAbility(DungeonAbility ability, Color rarityColor)
    {
        this.ability = ability;

        // 기본 정보 설정
        nameText.text = ability.name;
        descriptionText.text = ability.description;

        // 레벨과 희귀도 함께 표시
        levelAndRarityText.text = $"Lv.{ability.level} ({ability.rarity})";
        levelAndRarityText.color = rarityColor;

        // 텍스트 아웃라인 설정
        levelAndRarityText.outlineWidth = 0.2f;
        levelAndRarityText.outlineColor = new Color(0.1f, 0.1f, 0.1f, 1f); // 검은색 아웃라인

        // 배경 색상 설정 (더 연한 희귀도 색상)
        if (backgroundImage != null)
        {
            Color bgColor = rarityColor;
            bgColor.a = 0.8f; // 더 투명하게 설정
            backgroundImage.color = bgColor;
        }

        // 아이콘 설정 (있는 경우에만)
        if (ability.icon != null && abilityIconImage != null)
        {
            abilityIconImage.sprite = ability.icon;
            abilityIconImage.enabled = true;
        }
        else if (abilityIconImage != null)
        {
            // 아이콘이 없는 경우 기본 아이콘 또는 비활성화
            abilityIconImage.enabled = false;
        }

        // 희귀도가 높을수록 특별한 효과 적용
        ApplyRarityEffects(ability.rarity);

        Debug.Log($"어빌리티 UI 설정: {ability.name}, 레벨: {ability.level}, 희귀도: {ability.rarity}");
    }

    // 희귀도에 따른 특별 효과 적용
    private void ApplyRarityEffects(Rarity rarity)
    {
        // 희귀도가 높을수록 더 강한 효과 적용
        switch (rarity)
        {
            case Rarity.Epic:
            case Rarity.Legendary:
                // 펄스 효과 활성화
                StartPulseEffect();
                break;
        }
    }

    // 펄스 애니메이션 시작
    private void StartPulseEffect()
    {
        // 능력이 없는 경우 실행하지 않음
        if (ability == null)
            return;

        // 에픽 이상 희귀도만 펄스 효과 적용
        if (ability.rarity == Rarity.Epic || ability.rarity == Rarity.Legendary)
        {
            if (pulseTween != null && pulseTween.IsActive())
                pulseTween.Kill();

            // 펄스 애니메이션 생성
            pulseTween = transform.DOScale(originalScale * pulseScaleAmount, pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    // 마우스 호버 시 호출
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 호버 사운드
        if (hoverSound != null && audioSource != null)
            audioSource.PlayOneShot(hoverSound);

        // 호버 효과 (크기 확대)
        if (currentHoverTween != null && currentHoverTween.IsActive())
            currentHoverTween.Kill();

        currentHoverTween = transform.DOScale(originalScale * hoverScaleAmount, hoverScaleDuration)
            .SetEase(Ease.OutBack);

        // 배경 강조
        if (backgroundImage != null)
        {
            Color currentColor = backgroundImage.color;
            Color brighterColor = currentColor;
            brighterColor.a = 0.4f; // 더 선명하게
            backgroundImage.DOColor(brighterColor, hoverScaleDuration);
        }
    }

    // 마우스 호버 종료 시 호출
    public void OnPointerExit(PointerEventData eventData)
    {
        // 원래 크기로 복귀
        if (currentHoverTween != null && currentHoverTween.IsActive())
            currentHoverTween.Kill();

        currentHoverTween = transform.DOScale(originalScale, hoverScaleDuration)
            .SetEase(Ease.OutQuad);

        // 배경 원래대로
        if (backgroundImage != null && ability != null)
        {
            // 원래 색상으로 복원 (알파값 조정)
            Color currentColor = backgroundImage.color;
            currentColor.a = 0.2f;
            backgroundImage.DOColor(currentColor, hoverScaleDuration);
        }

        // 필요하면 펄스 효과 다시 시작
        StartPulseEffect();
    }
}