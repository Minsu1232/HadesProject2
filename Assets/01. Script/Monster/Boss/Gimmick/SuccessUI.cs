using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class SuccessUI : MonoBehaviour, ISuccessUI
{
    [Header("UI Components")]
    [SerializeField] private Image timeBarFill;
    [SerializeField] private GameObject successUI;
    [SerializeField] private TextMeshProUGUI countText;

    [Header("Animation Settings")]
    [SerializeField] private float barUpdateDuration = 0.2f;
    [SerializeField] private Color normalBarColor = Color.blue;
    [SerializeField] private Color warningBarColor = Color.red;
    [SerializeField] private float warningThreshold = 0.3f; // 30% ������ �� ���������� ����

    private int maxCount;
  

    public void InitializeSuccessUI(int maxSuccessCount)
    {
        maxCount = maxSuccessCount;
        //successUI.gameObject.SetActive(true);
        // �ʱ� UI ����
        timeBarFill.fillAmount = 1f;
        timeBarFill.color = normalBarColor;
        UpdateSuccessCount(0);

  

        // UI Ȱ��ȭ
        successUI.gameObject.SetActive(true);
    }

    public void UpdateSuccessCount(int currentSuccessCount)
    {
        // �ؽ�Ʈ ������Ʈ
        countText.text = $"{currentSuccessCount}/{maxCount}";

    
    }

    public void UpdateTimeBar(float normalizedTime)
    {
        // �ε巯�� �� ������Ʈ
        timeBarFill.DOFillAmount(normalizedTime, barUpdateDuration);

        // ��� �Ӱ谪 ������ �� ���� ����
        if (normalizedTime <= warningThreshold)
        {
            timeBarFill.DOColor(warningBarColor, barUpdateDuration);
        }
        else
        {
            timeBarFill.DOColor(normalBarColor, barUpdateDuration);
        }
    }

    public void UIOff()
    {
        successUI.gameObject.SetActive(false);
        // UI ��Ȱ��ȭ �� ��� Ʈ�� ����
     
        timeBarFill.DOKill();
        
    }

    private void OnDestroy()
    {      
        timeBarFill.DOKill();
    }
}