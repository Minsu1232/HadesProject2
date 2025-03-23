using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class AbilityCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("카드 구성 요소")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image abilityIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject levelIndicator;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image rarityBorder;  // 프레임 이미지 (테두리)

    [Header("쉐이더 설정")]
    [SerializeField] private Material edgeGlowMaterial;  // EdgeGlow 쉐이더 머티리얼
    [SerializeField] private Material iconGlowMaterial; // 아이콘용 글로우 쉐이더 머티리얼 (BorderPatrol 쉐이더를 사용한 머티리얼)

    [Header("희귀도 스타일")]
    [SerializeField]
    private Color[] rarityColors = new Color[] {
        new Color(1f, 1f, 1f),       // Common - 흰색
        new Color(0.3f, 0.7f, 0.3f), // Uncommon - 녹색
        new Color(0.3f, 0.3f, 0.9f), // Rare - 파란색
        new Color(0.7f, 0.3f, 0.9f), // Epic - 보라색
        new Color(1.0f, 0.8f, 0.0f)  // Legendary - 금색
    };

    // 외곽선 색상 (희귀도 기본 색상에서 약간 더 밝게)
    [SerializeField]
    private Color[] outlineColors = new Color[] {
        new Color(1f, 1f, 1f),       // Common - 흰색
        new Color(0.4f, 0.9f, 0.4f), // Uncommon - 밝은 녹색
        new Color(0.4f, 0.4f, 1f),   // Rare - 밝은 파란색
        new Color(0.9f, 0.4f, 1f),   // Epic - 밝은 보라색
        new Color(1f, 0.9f, 0.2f)    // Legendary - 밝은 금색
    };

    // 패트롤 효과 색상 (희귀도별 보색 또는 대비되는 색)
    [SerializeField]
    private Color[] patrolColors = new Color[] {
        new Color(0.9f, 0.9f, 0.9f), // Common - 밝은 은색
        new Color(1f, 0.5f, 0.5f),   // Uncommon - 연한 빨강색 (녹색의 보색)
        new Color(1f, 0.7f, 0.3f),   // Rare - 황금색 (파란색의 보색)
        new Color(0.5f, 1f, 0.5f),   // Epic - 연두색 (보라색의 보색)
        new Color(0.3f, 0.7f, 1f)    // Legendary - 하늘색 (금색 대비)
    };

    [Header("애니메이션 설정")]
    [SerializeField] private float hoverScaleAmount = 1.1f;
    [SerializeField] private float hoverScaleDuration = 0.2f;

    [Header("희귀도별 쉐이더 설정")]
    [SerializeField] private float[] glowPowerValues = { 1f, 2f, 3f, 4f, 5f };
    [SerializeField] private float[] glowWidthValues = { 2f, 3f, 4f, 5f, 7f };
    [SerializeField] private float[] pulseSpeedValues = { 0f, 0.5f, 1f, 1.5f, 2f };
    [SerializeField] private float[] pulseAmountValues = { 0f, 0.1f, 0.2f, 0.3f, 0.4f };

    // BorderPatrol 쉐이더 설정
    [SerializeField] private float[] patrolGlowValues = { 1f, 2f, 3f, 4f, 5f };
    [SerializeField] private float[] flashSpeedValues = { 0f, 0.5f, 1f, 1.5f, 2f };

    [Header("사운드 효과")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip selectSound;

    private DungeonAbility ability;
    private System.Action<DungeonAbility> onSelected;
    private AudioSource audioSource;
    private Vector3 originalScale;
    private bool isHovering = false;
    private Tween currentScaleTween;
    private Material instancedMaterial; // 개별 인스턴스에 적용할 머티리얼
    private Material iconMaterial; // 아이콘용 머티리얼 인스턴스

    private void Awake()
    {
        // 오디오 소스 컴포넌트 가져오거나 생성
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        originalScale = transform.localScale;

        // 버튼 컴포넌트가 있는지 확인하고 없으면 추가
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }

        // 쉐이더 머티리얼 초기화
        SetupEdgeGlowMaterial();
    }

    private void SetupEdgeGlowMaterial()
    {
        if (edgeGlowMaterial != null && rarityBorder != null)
        {
            // 머티리얼 인스턴스 생성
            instancedMaterial = new Material(edgeGlowMaterial);

            // 테두리 이미지에 머티리얼 적용
            rarityBorder.material = instancedMaterial;
        }
    }

    private void OnEnable()
    {
        // 카드가 활성화될 때만 쉐이더 값 업데이트
        if (ability != null && instancedMaterial != null)
        {
            UpdateShaderValues((int)ability.rarity);
        }
    }

    private void OnDisable()
    {
        // 애니메이션 중단
        if (currentScaleTween != null && currentScaleTween.IsActive())
            currentScaleTween.Kill();

        // 인스턴스 머티리얼 정리
        if (instancedMaterial != null)
        {
            Destroy(instancedMaterial);
            instancedMaterial = null;
        }

        if (iconMaterial != null)
        {
            Destroy(iconMaterial);
            iconMaterial = null;
        }
    }

    public void Setup(DungeonAbility ability, System.Action<DungeonAbility> onSelected)
    {
        this.ability = ability;
        this.onSelected = onSelected;

        // 기본 정보 설정
        nameText.text = ability.name;

        // 레벨업 값 계산 - DungeonAbility에서 직접 접근
        float levelUpValue = ability.effectValue * ability.levelMultiplier;

        // 설명 끝에 레벨업 정보 추가
        string formattedDesc = ability.description.TrimEnd('.', ' ', '\n', '\r') + $" (레벨당 +{levelUpValue:0.#}).";
        descriptionText.text = formattedDesc;

        if (ability.icon != null)
            abilityIcon.sprite = ability.icon;

        // 디버그 로그 추가
        Debug.Log($"카드 셋업: 이름={ability.name}, 희귀도={ability.rarity}, 희귀도인덱스={(int)ability.rarity}");

        int rarityIndex = (int)ability.rarity;

        // 쉐이더 값 업데이트
        UpdateShaderValues(rarityIndex);

        // 아이콘에 BorderPatrol 쉐이더 적용
        ApplyBorderPatrolShader(rarityIndex);

        // 레벨 표시
        if (ability.level > 1)
        {
            levelIndicator.SetActive(true);
            levelText.text = $"Lv.{ability.level}";
        }
        else
        {
            levelIndicator.SetActive(false);
        }
    }

    // BorderPatrol 쉐이더 적용
    private void ApplyBorderPatrolShader(int rarityIndex)
    {
        if (ability.icon != null && iconGlowMaterial != null)
        {
            // 이전 머티리얼 정리
            if (iconMaterial != null)
            {
                Destroy(iconMaterial);
            }

            // 새 머티리얼 인스턴스 생성
            iconMaterial = new Material(iconGlowMaterial);
            abilityIcon.material = iconMaterial;

            // 기본 설정 (모든 희귀도에 공통적인 값)
            iconMaterial.SetFloat("_PatrolSpeed", 0.2f);      // 기본 0.2 속도
            iconMaterial.SetFloat("_PatrolWidth", 0.05f);     // 기본 0.05 너비
            iconMaterial.SetFloat("_OutlineWidth", 0.1f);     // 기본 0.1 외곽선 너비
            iconMaterial.SetFloat("_PatrolGlow", 3f);         // 기본 3x 발광 강도
            iconMaterial.SetFloat("_FlashSpeed", 0f);         // 기본 깜빡임 없음

            // 희귀도별 다른 설정
            // 1. 색상 설정
            iconMaterial.SetColor("_OutlineColor", outlineColors[rarityIndex]);
            iconMaterial.SetColor("_PatrolColor", patrolColors[rarityIndex]);

            // 2. 희귀도에 따른 효과 강화
            if (rarityIndex >= 1) // 고급 이상
            {
                iconMaterial.SetFloat("_PatrolWidth", 0.05f + (rarityIndex * 0.02f)); // 희귀도가 높을수록 효과 폭 증가
            }

            if (rarityIndex >= 2) // 희귀 이상
            {
                iconMaterial.SetFloat("_PatrolGlow", 3f + rarityIndex); // 희귀도가 높을수록 발광 강화
            }

            if (rarityIndex >= 3) // 에픽 이상
            {
                iconMaterial.SetFloat("_FlashSpeed", 0.5f * rarityIndex); // 에픽 이상에서 깜빡임 추가
            }

            if (rarityIndex == 4) // 전설 등급
            {
                iconMaterial.SetFloat("_PatrolSpeed", 0.5f); // 전설 등급은 더 빠른 움직임
                iconMaterial.SetFloat("_PatrolWidth", 0.15f); // 더 넓은 효과
                iconMaterial.SetFloat("_FlashSpeed", 2f); // 빠른 깜빡임
            }
        }
    }

    // 쉐이더 값 업데이트
    private void UpdateShaderValues(int rarityIndex)
    {
        if (instancedMaterial == null || rarityIndex < 0 || rarityIndex >= rarityColors.Length)
            return;

        // 희귀도에 따른 색상 설정
        Color glowColor = rarityColors[rarityIndex];
        instancedMaterial.SetColor("_GlowColor", glowColor);

        // 희귀도에 따른 쉐이더 속성 설정
        if (rarityIndex < glowPowerValues.Length)
            instancedMaterial.SetFloat("_GlowPower", glowPowerValues[rarityIndex]);

        if (rarityIndex < glowWidthValues.Length)
            instancedMaterial.SetFloat("_GlowWidth", glowWidthValues[rarityIndex]);

        if (rarityIndex < pulseSpeedValues.Length)
            instancedMaterial.SetFloat("_PulseSpeed", pulseSpeedValues[rarityIndex]);

        if (rarityIndex < pulseAmountValues.Length)
            instancedMaterial.SetFloat("_PulseAmount", pulseAmountValues[rarityIndex]);

        // 알파 컷오프 값 설정 (가장자리 감지 민감도)
        instancedMaterial.SetFloat("_AlphaCutoff", 0.1f);
    }

    // IPointerEnterHandler 구현
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnter();
    }

    // IPointerExitHandler 구현
    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExit();
    }

    // 마우스 호버 시 호출
    public void OnPointerEnter()
    {
        isHovering = true;

        // 호버 사운드
        if (hoverSound != null && audioSource != null)
            audioSource.PlayOneShot(hoverSound);

        // 카드 확대
        if (currentScaleTween != null && currentScaleTween.IsActive())
            currentScaleTween.Kill();

        currentScaleTween = transform.DOScale(originalScale * hoverScaleAmount, hoverScaleDuration)
            .SetEase(Ease.OutBack);

        // 쉐이더 효과 강화
        if (instancedMaterial != null)
        {
            // 현재 값 가져오기
            float currentGlowPower = instancedMaterial.GetFloat("_GlowPower");
            float currentGlowWidth = instancedMaterial.GetFloat("_GlowWidth");
            float currentPulseSpeed = instancedMaterial.GetFloat("_PulseSpeed");

            // 호버 시 강화된 값으로 설정
            instancedMaterial.SetFloat("_GlowPower", currentGlowPower * 1.5f);
            instancedMaterial.SetFloat("_GlowWidth", currentGlowWidth * 1.2f);
            instancedMaterial.SetFloat("_PulseSpeed", currentPulseSpeed * 1.5f);
            instancedMaterial.SetFloat("_PulseAmount", 0.5f); // 항상 펄스 효과 활성화
        }

        // 아이콘 효과 강화
        if (iconMaterial != null)
        {
            // 호버 시 효과 강화
            float currentPatrolWidth = iconMaterial.GetFloat("_PatrolWidth");
            float currentPatrolGlow = iconMaterial.GetFloat("_PatrolGlow");

            iconMaterial.SetFloat("_PatrolWidth", currentPatrolWidth * 1.5f);
            iconMaterial.SetFloat("_PatrolGlow", currentPatrolGlow * 1.3f);

            // 깜빡임 추가 또는 강화
            float currentFlashSpeed = iconMaterial.GetFloat("_FlashSpeed");
            if (currentFlashSpeed > 0)
                iconMaterial.SetFloat("_FlashSpeed", currentFlashSpeed * 1.5f);
            else
                iconMaterial.SetFloat("_FlashSpeed", 1f); // 기본 깜빡임 추가
        }
    }

    // 마우스 호버 종료 시 호출
    public void OnPointerExit()
    {
        isHovering = false;

        // 원래 크기로 복귀
        if (currentScaleTween != null && currentScaleTween.IsActive())
            currentScaleTween.Kill();

        currentScaleTween = transform.DOScale(originalScale, hoverScaleDuration)
            .SetEase(Ease.OutQuad);

        // 쉐이더 효과 원래대로
        if (instancedMaterial != null && ability != null)
        {
            int rarityIndex = (int)ability.rarity;
            UpdateShaderValues(rarityIndex);
        }

        // 아이콘 효과 원래대로
        if (iconMaterial != null && ability != null)
        {
            int rarityIndex = (int)ability.rarity;
            ApplyBorderPatrolShader(rarityIndex);
        }
    }

    // 카드 클릭 시 호출
    public void OnClick()
    {
        // 선택 사운드
        if (selectSound != null && audioSource != null)
            audioSource.PlayOneShot(selectSound);

        // 카드 펀치 효과
        transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);

        // 쉐이더 효과 강화 (깜빡임)
        if (instancedMaterial != null)
        {
            Color originalColor = instancedMaterial.GetColor("_GlowColor");
            Color flashColor = Color.white;

            // 깜빡임 시퀀스
            Sequence glowFlash = DOTween.Sequence();

            // 글로우 색상 깜빡임
            glowFlash.Append(
                DOTween.To(() => originalColor,
                    x => instancedMaterial.SetColor("_GlowColor", x),
                    flashColor, 0.1f)
            );
            glowFlash.Append(
                DOTween.To(() => flashColor,
                    x => instancedMaterial.SetColor("_GlowColor", x),
                    originalColor, 0.1f)
            );
            glowFlash.Append(
                DOTween.To(() => originalColor,
                    x => instancedMaterial.SetColor("_GlowColor", x),
                    flashColor, 0.1f)
            );
            glowFlash.Append(
                DOTween.To(() => flashColor,
                    x => instancedMaterial.SetColor("_GlowColor", x),
                    originalColor, 0.1f)
            );

            // 글로우 강도 증가
            float currentGlowPower = instancedMaterial.GetFloat("_GlowPower");
            instancedMaterial.SetFloat("_GlowPower", currentGlowPower * 2f);

            // 시퀀스 실행
            glowFlash.Play();

            // 0.5초 후 원래 값으로 복원
            DOVirtual.DelayedCall(0.5f, () => {
                UpdateShaderValues((int)ability.rarity);
            });
        }

        // 아이콘 효과 깜빡임
        if (iconMaterial != null)
        {
            // 클릭 시 효과 강화
            float originalWidth = iconMaterial.GetFloat("_PatrolWidth");
            float originalGlow = iconMaterial.GetFloat("_PatrolGlow");

            // 강화된 효과
            iconMaterial.SetFloat("_PatrolWidth", originalWidth * 2f);
            iconMaterial.SetFloat("_PatrolGlow", originalGlow * 2f);
            iconMaterial.SetFloat("_FlashSpeed", 3f); // 강한 깜빡임

            // 원래 효과로 복원
            DOVirtual.DelayedCall(0.3f, () => {
                if (iconMaterial != null && ability != null)
                {
                    ApplyBorderPatrolShader((int)ability.rarity);
                }
            });
        }

        // 선택 이벤트 발생 (약간 지연시켜 시각 효과 확인 가능)
        DOVirtual.DelayedCall(0.3f, () => {
            onSelected?.Invoke(ability);
        });
    }
}