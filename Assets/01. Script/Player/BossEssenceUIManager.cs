using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class BossEssenceUIManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image essenceBarFill;  // Fill 이미지
    [SerializeField] private TextMeshProUGUI percentageText;  // 퍼센트 텍스트

    [Header("Bar Settings")]
    [SerializeField] private Color normalColor = Color.red;    // 기본 빨간색
    [SerializeField] private Color highStateColor = Color.magenta;  // 75% 이상
    [SerializeField] private Color maxStateColor = Color.yellow;    // 100%
    [SerializeField] private float barUpdateDuration = 0.2f;

    private IBossEssenceSystem currentEssenceSystem;

    public void Initialize(string bossName, IBossEssenceSystem essenceSystem)
    {
        currentEssenceSystem = essenceSystem;

        // 이벤트 구독
        essenceSystem.OnEssenceChanged += UpdateEssenceBar;
        essenceSystem.OnEssenceStateChanged += UpdateEssenceState;
        essenceSystem.OnMaxEssenceStateChanged += UpdateMaxEssenceState;

        // UI 초기화
        essenceBarFill.fillAmount = 0f;
        essenceBarFill.color = normalColor;
        UpdatePercentageText(0);
    }

    private void UpdateEssenceBar(float value)
    {
        float fillAmount = value / 100f;
        essenceBarFill.DOFillAmount(fillAmount, barUpdateDuration);
        UpdatePercentageText(value);
    }

    private void UpdateEssenceState()
    {
        if (currentEssenceSystem.IsInEssenceState)
        {
            essenceBarFill.DOColor(highStateColor, barUpdateDuration);
        }
        else
        {
            essenceBarFill.DOColor(normalColor, barUpdateDuration);
        }
    }

    private void UpdateMaxEssenceState()
    {
        if (currentEssenceSystem.IsMaxEssence)
        {
            essenceBarFill.DOColor(maxStateColor, barUpdateDuration);
        }
    }

    private void UpdatePercentageText(float value)
    {
        percentageText.text = $"{Mathf.Round(value)}%";
    }

    private void OnDestroy()
    {
        if (currentEssenceSystem != null)
        {
            currentEssenceSystem.OnEssenceChanged -= UpdateEssenceBar;
            currentEssenceSystem.OnEssenceStateChanged -= UpdateEssenceState;
            currentEssenceSystem.OnMaxEssenceStateChanged -= UpdateMaxEssenceState;
        }
    }
}