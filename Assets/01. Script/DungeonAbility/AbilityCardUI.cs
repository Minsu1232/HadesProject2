using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class AbilityCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("ī�� ���� ���")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image abilityIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject levelIndicator;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image rarityBorder;  // ������ �̹��� (�׵θ�)

    [Header("���̴� ����")]
    [SerializeField] private Material edgeGlowMaterial;  // EdgeGlow ���̴� ��Ƽ����
    [SerializeField] private Material iconGlowMaterial; // �����ܿ� �۷ο� ���̴� ��Ƽ���� (BorderPatrol ���̴��� ����� ��Ƽ����)

    [Header("��͵� ��Ÿ��")]
    [SerializeField]
    private Color[] rarityColors = new Color[] {
        new Color(1f, 1f, 1f),       // Common - ���
        new Color(0.3f, 0.7f, 0.3f), // Uncommon - ���
        new Color(0.3f, 0.3f, 0.9f), // Rare - �Ķ���
        new Color(0.7f, 0.3f, 0.9f), // Epic - �����
        new Color(1.0f, 0.8f, 0.0f)  // Legendary - �ݻ�
    };

    // �ܰ��� ���� (��͵� �⺻ ���󿡼� �ణ �� ���)
    [SerializeField]
    private Color[] outlineColors = new Color[] {
        new Color(1f, 1f, 1f),       // Common - ���
        new Color(0.4f, 0.9f, 0.4f), // Uncommon - ���� ���
        new Color(0.4f, 0.4f, 1f),   // Rare - ���� �Ķ���
        new Color(0.9f, 0.4f, 1f),   // Epic - ���� �����
        new Color(1f, 0.9f, 0.2f)    // Legendary - ���� �ݻ�
    };

    // ��Ʈ�� ȿ�� ���� (��͵��� ���� �Ǵ� ���Ǵ� ��)
    [SerializeField]
    private Color[] patrolColors = new Color[] {
        new Color(0.9f, 0.9f, 0.9f), // Common - ���� ����
        new Color(1f, 0.5f, 0.5f),   // Uncommon - ���� ������ (����� ����)
        new Color(1f, 0.7f, 0.3f),   // Rare - Ȳ�ݻ� (�Ķ����� ����)
        new Color(0.5f, 1f, 0.5f),   // Epic - ���λ� (������� ����)
        new Color(0.3f, 0.7f, 1f)    // Legendary - �ϴû� (�ݻ� ���)
    };

    [Header("�ִϸ��̼� ����")]
    [SerializeField] private float hoverScaleAmount = 1.1f;
    [SerializeField] private float hoverScaleDuration = 0.2f;

    [Header("��͵��� ���̴� ����")]
    [SerializeField] private float[] glowPowerValues = { 1f, 2f, 3f, 4f, 5f };
    [SerializeField] private float[] glowWidthValues = { 2f, 3f, 4f, 5f, 7f };
    [SerializeField] private float[] pulseSpeedValues = { 0f, 0.5f, 1f, 1.5f, 2f };
    [SerializeField] private float[] pulseAmountValues = { 0f, 0.1f, 0.2f, 0.3f, 0.4f };

    // BorderPatrol ���̴� ����
    [SerializeField] private float[] patrolGlowValues = { 1f, 2f, 3f, 4f, 5f };
    [SerializeField] private float[] flashSpeedValues = { 0f, 0.5f, 1f, 1.5f, 2f };

    [Header("���� ȿ��")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip selectSound;

    private DungeonAbility ability;
    private System.Action<DungeonAbility> onSelected;
    private AudioSource audioSource;
    private Vector3 originalScale;
    private bool isHovering = false;
    private Tween currentScaleTween;
    private Material instancedMaterial; // ���� �ν��Ͻ��� ������ ��Ƽ����
    private Material iconMaterial; // �����ܿ� ��Ƽ���� �ν��Ͻ�

    private void Awake()
    {
        // ����� �ҽ� ������Ʈ �������ų� ����
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        originalScale = transform.localScale;

        // ��ư ������Ʈ�� �ִ��� Ȯ���ϰ� ������ �߰�
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }

        // ���̴� ��Ƽ���� �ʱ�ȭ
        SetupEdgeGlowMaterial();
    }

    private void SetupEdgeGlowMaterial()
    {
        if (edgeGlowMaterial != null && rarityBorder != null)
        {
            // ��Ƽ���� �ν��Ͻ� ����
            instancedMaterial = new Material(edgeGlowMaterial);

            // �׵θ� �̹����� ��Ƽ���� ����
            rarityBorder.material = instancedMaterial;
        }
    }

    private void OnEnable()
    {
        // ī�尡 Ȱ��ȭ�� ���� ���̴� �� ������Ʈ
        if (ability != null && instancedMaterial != null)
        {
            UpdateShaderValues((int)ability.rarity);
        }
    }

    private void OnDisable()
    {
        // �ִϸ��̼� �ߴ�
        if (currentScaleTween != null && currentScaleTween.IsActive())
            currentScaleTween.Kill();

        // �ν��Ͻ� ��Ƽ���� ����
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

        // �⺻ ���� ����
        nameText.text = ability.name;

        // ������ �� ��� - DungeonAbility���� ���� ����
        float levelUpValue = ability.effectValue * ability.levelMultiplier;

        // ���� ���� ������ ���� �߰�
        string formattedDesc = ability.description.TrimEnd('.', ' ', '\n', '\r') + $" (������ +{levelUpValue:0.#}).";
        descriptionText.text = formattedDesc;

        if (ability.icon != null)
            abilityIcon.sprite = ability.icon;

        // ����� �α� �߰�
        Debug.Log($"ī�� �¾�: �̸�={ability.name}, ��͵�={ability.rarity}, ��͵��ε���={(int)ability.rarity}");

        int rarityIndex = (int)ability.rarity;

        // ���̴� �� ������Ʈ
        UpdateShaderValues(rarityIndex);

        // �����ܿ� BorderPatrol ���̴� ����
        ApplyBorderPatrolShader(rarityIndex);

        // ���� ǥ��
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

    // BorderPatrol ���̴� ����
    private void ApplyBorderPatrolShader(int rarityIndex)
    {
        if (ability.icon != null && iconGlowMaterial != null)
        {
            // ���� ��Ƽ���� ����
            if (iconMaterial != null)
            {
                Destroy(iconMaterial);
            }

            // �� ��Ƽ���� �ν��Ͻ� ����
            iconMaterial = new Material(iconGlowMaterial);
            abilityIcon.material = iconMaterial;

            // �⺻ ���� (��� ��͵��� �������� ��)
            iconMaterial.SetFloat("_PatrolSpeed", 0.2f);      // �⺻ 0.2 �ӵ�
            iconMaterial.SetFloat("_PatrolWidth", 0.05f);     // �⺻ 0.05 �ʺ�
            iconMaterial.SetFloat("_OutlineWidth", 0.1f);     // �⺻ 0.1 �ܰ��� �ʺ�
            iconMaterial.SetFloat("_PatrolGlow", 3f);         // �⺻ 3x �߱� ����
            iconMaterial.SetFloat("_FlashSpeed", 0f);         // �⺻ ������ ����

            // ��͵��� �ٸ� ����
            // 1. ���� ����
            iconMaterial.SetColor("_OutlineColor", outlineColors[rarityIndex]);
            iconMaterial.SetColor("_PatrolColor", patrolColors[rarityIndex]);

            // 2. ��͵��� ���� ȿ�� ��ȭ
            if (rarityIndex >= 1) // ��� �̻�
            {
                iconMaterial.SetFloat("_PatrolWidth", 0.05f + (rarityIndex * 0.02f)); // ��͵��� �������� ȿ�� �� ����
            }

            if (rarityIndex >= 2) // ��� �̻�
            {
                iconMaterial.SetFloat("_PatrolGlow", 3f + rarityIndex); // ��͵��� �������� �߱� ��ȭ
            }

            if (rarityIndex >= 3) // ���� �̻�
            {
                iconMaterial.SetFloat("_FlashSpeed", 0.5f * rarityIndex); // ���� �̻󿡼� ������ �߰�
            }

            if (rarityIndex == 4) // ���� ���
            {
                iconMaterial.SetFloat("_PatrolSpeed", 0.5f); // ���� ����� �� ���� ������
                iconMaterial.SetFloat("_PatrolWidth", 0.15f); // �� ���� ȿ��
                iconMaterial.SetFloat("_FlashSpeed", 2f); // ���� ������
            }
        }
    }

    // ���̴� �� ������Ʈ
    private void UpdateShaderValues(int rarityIndex)
    {
        if (instancedMaterial == null || rarityIndex < 0 || rarityIndex >= rarityColors.Length)
            return;

        // ��͵��� ���� ���� ����
        Color glowColor = rarityColors[rarityIndex];
        instancedMaterial.SetColor("_GlowColor", glowColor);

        // ��͵��� ���� ���̴� �Ӽ� ����
        if (rarityIndex < glowPowerValues.Length)
            instancedMaterial.SetFloat("_GlowPower", glowPowerValues[rarityIndex]);

        if (rarityIndex < glowWidthValues.Length)
            instancedMaterial.SetFloat("_GlowWidth", glowWidthValues[rarityIndex]);

        if (rarityIndex < pulseSpeedValues.Length)
            instancedMaterial.SetFloat("_PulseSpeed", pulseSpeedValues[rarityIndex]);

        if (rarityIndex < pulseAmountValues.Length)
            instancedMaterial.SetFloat("_PulseAmount", pulseAmountValues[rarityIndex]);

        // ���� �ƿ��� �� ���� (�����ڸ� ���� �ΰ���)
        instancedMaterial.SetFloat("_AlphaCutoff", 0.1f);
    }

    // IPointerEnterHandler ����
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnter();
    }

    // IPointerExitHandler ����
    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExit();
    }

    // ���콺 ȣ�� �� ȣ��
    public void OnPointerEnter()
    {
        isHovering = true;

        // ȣ�� ����
        if (hoverSound != null && audioSource != null)
            audioSource.PlayOneShot(hoverSound);

        // ī�� Ȯ��
        if (currentScaleTween != null && currentScaleTween.IsActive())
            currentScaleTween.Kill();

        currentScaleTween = transform.DOScale(originalScale * hoverScaleAmount, hoverScaleDuration)
            .SetEase(Ease.OutBack);

        // ���̴� ȿ�� ��ȭ
        if (instancedMaterial != null)
        {
            // ���� �� ��������
            float currentGlowPower = instancedMaterial.GetFloat("_GlowPower");
            float currentGlowWidth = instancedMaterial.GetFloat("_GlowWidth");
            float currentPulseSpeed = instancedMaterial.GetFloat("_PulseSpeed");

            // ȣ�� �� ��ȭ�� ������ ����
            instancedMaterial.SetFloat("_GlowPower", currentGlowPower * 1.5f);
            instancedMaterial.SetFloat("_GlowWidth", currentGlowWidth * 1.2f);
            instancedMaterial.SetFloat("_PulseSpeed", currentPulseSpeed * 1.5f);
            instancedMaterial.SetFloat("_PulseAmount", 0.5f); // �׻� �޽� ȿ�� Ȱ��ȭ
        }

        // ������ ȿ�� ��ȭ
        if (iconMaterial != null)
        {
            // ȣ�� �� ȿ�� ��ȭ
            float currentPatrolWidth = iconMaterial.GetFloat("_PatrolWidth");
            float currentPatrolGlow = iconMaterial.GetFloat("_PatrolGlow");

            iconMaterial.SetFloat("_PatrolWidth", currentPatrolWidth * 1.5f);
            iconMaterial.SetFloat("_PatrolGlow", currentPatrolGlow * 1.3f);

            // ������ �߰� �Ǵ� ��ȭ
            float currentFlashSpeed = iconMaterial.GetFloat("_FlashSpeed");
            if (currentFlashSpeed > 0)
                iconMaterial.SetFloat("_FlashSpeed", currentFlashSpeed * 1.5f);
            else
                iconMaterial.SetFloat("_FlashSpeed", 1f); // �⺻ ������ �߰�
        }
    }

    // ���콺 ȣ�� ���� �� ȣ��
    public void OnPointerExit()
    {
        isHovering = false;

        // ���� ũ��� ����
        if (currentScaleTween != null && currentScaleTween.IsActive())
            currentScaleTween.Kill();

        currentScaleTween = transform.DOScale(originalScale, hoverScaleDuration)
            .SetEase(Ease.OutQuad);

        // ���̴� ȿ�� �������
        if (instancedMaterial != null && ability != null)
        {
            int rarityIndex = (int)ability.rarity;
            UpdateShaderValues(rarityIndex);
        }

        // ������ ȿ�� �������
        if (iconMaterial != null && ability != null)
        {
            int rarityIndex = (int)ability.rarity;
            ApplyBorderPatrolShader(rarityIndex);
        }
    }

    // ī�� Ŭ�� �� ȣ��
    public void OnClick()
    {
        // ���� ����
        if (selectSound != null && audioSource != null)
            audioSource.PlayOneShot(selectSound);

        // ī�� ��ġ ȿ��
        transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);

        // ���̴� ȿ�� ��ȭ (������)
        if (instancedMaterial != null)
        {
            Color originalColor = instancedMaterial.GetColor("_GlowColor");
            Color flashColor = Color.white;

            // ������ ������
            Sequence glowFlash = DOTween.Sequence();

            // �۷ο� ���� ������
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

            // �۷ο� ���� ����
            float currentGlowPower = instancedMaterial.GetFloat("_GlowPower");
            instancedMaterial.SetFloat("_GlowPower", currentGlowPower * 2f);

            // ������ ����
            glowFlash.Play();

            // 0.5�� �� ���� ������ ����
            DOVirtual.DelayedCall(0.5f, () => {
                UpdateShaderValues((int)ability.rarity);
            });
        }

        // ������ ȿ�� ������
        if (iconMaterial != null)
        {
            // Ŭ�� �� ȿ�� ��ȭ
            float originalWidth = iconMaterial.GetFloat("_PatrolWidth");
            float originalGlow = iconMaterial.GetFloat("_PatrolGlow");

            // ��ȭ�� ȿ��
            iconMaterial.SetFloat("_PatrolWidth", originalWidth * 2f);
            iconMaterial.SetFloat("_PatrolGlow", originalGlow * 2f);
            iconMaterial.SetFloat("_FlashSpeed", 3f); // ���� ������

            // ���� ȿ���� ����
            DOVirtual.DelayedCall(0.3f, () => {
                if (iconMaterial != null && ability != null)
                {
                    ApplyBorderPatrolShader((int)ability.rarity);
                }
            });
        }

        // ���� �̺�Ʈ �߻� (�ణ �������� �ð� ȿ�� Ȯ�� ����)
        DOVirtual.DelayedCall(0.3f, () => {
            onSelected?.Invoke(ability);
        });
    }
}