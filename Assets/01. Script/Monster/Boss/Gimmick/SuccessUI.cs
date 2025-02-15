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
    [SerializeField] private float warningThreshold = 0.3f; // 30% 이하일 때 경고색상으로 변경

    private int maxCount;
  

    public void InitializeSuccessUI(int maxSuccessCount)
    {
        maxCount = maxSuccessCount;
        //successUI.gameObject.SetActive(true);
        // 초기 UI 세팅
        timeBarFill.fillAmount = 1f;
        timeBarFill.color = normalBarColor;
        UpdateSuccessCount(0);

  

        // UI 활성화
        successUI.gameObject.SetActive(true);
    }

    public void UpdateSuccessCount(int currentSuccessCount)
    {
        // 텍스트 업데이트
        countText.text = $"{currentSuccessCount}/{maxCount}";

    
    }

    public void UpdateTimeBar(float normalizedTime)
    {
        // 부드러운 바 업데이트
        timeBarFill.DOFillAmount(normalizedTime, barUpdateDuration);

        // 경고 임계값 이하일 때 색상 변경
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
        // UI 비활성화 시 모든 트윈 정리
     
        timeBarFill.DOKill();
        
    }

    private void OnDestroy()
    {      
        timeBarFill.DOKill();
    }
}