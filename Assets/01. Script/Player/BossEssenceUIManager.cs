using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class BossEssenceUIManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image essenceBarFill;
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] private TextMeshProUGUI essenceName;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.red;
    [SerializeField] private Color highStateColor = Color.magenta;  // 70% �̻�
    [SerializeField] private Color maxStateColor = Color.yellow;    // 100%
    [SerializeField] private float barUpdateDuration = 0.2f;

    private IBossEssenceSystem currentEssenceSystem;

    public void Initialize(IBossEssenceSystem essenceSystem)
    {
        currentEssenceSystem = essenceSystem;

        // �̺�Ʈ ����
        essenceSystem.OnEssenceChanged += UpdateEssenceBar;
        essenceSystem.OnEssenceStateChanged += UpdateEssenceState;
        essenceSystem.OnMaxEssenceStateChanged += UpdateMaxEssenceState;
        essenceName.text = currentEssenceSystem.BossEssenceName;
        // �ʱ� ���� ����
        essenceBarFill.fillAmount = 0f;
        essenceBarFill.color = normalColor;
        UpdatePercentageText(0);

        Debug.Log("��~��");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))  // �׽�Ʈ�� Ű
        {
            // ������ ���� �׽�Ʈ
            currentEssenceSystem.IncreaseEssence(10f);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            // ������ ���� �׽�Ʈ
            currentEssenceSystem.DecreaseEssence(10f);
        }
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