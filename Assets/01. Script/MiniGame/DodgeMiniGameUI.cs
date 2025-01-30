// 닷지 미니게임 UI
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DodgeMiniGameUI : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image backgroundBar;
    [SerializeField] private Image successBar;
    [SerializeField] private Image progressArrow;
    [SerializeField] private TextMeshProUGUI remainingTimeText;
    [SerializeField] private TextMeshProUGUI resultText;

    private DodgeMiniGameWrapper miniGame;

    private void Awake()
    {
        var miniGameManager = FindObjectOfType<MiniGameManager>();
        if (miniGameManager != null)
        {
            miniGameManager.RegisterUI(MiniGameType.Dodge, this);
            Debug.Log("UI successfully registered for MiniGameType.Dodge.");
        }
        else
        {
            Debug.LogError("Failed to register UI. MiniGameManager is null.");
        }
    }

    public void Initialize(DodgeMiniGameWrapper game)
    {
        Debug.Log("DodgeMiniGameUI Initialize called");
        miniGame = game;
        canvas.gameObject.SetActive(true);
        // 약간의 지연을 주고 SetSuccessBarPosition 호출
        StartCoroutine(DelayedSetup());
    }
    private IEnumerator DelayedSetup()
    {
        // 한 프레임 대기
        yield return new WaitForEndOfFrame();

        SetSuccessBarPosition();
        resultText.text = "";
    }
    private void Update()
    {
        if (miniGame == null || miniGame.IsComplete) return;

        UpdateProgressArrow();
        UpdateRemainingTime();
    }

    private void SetSuccessBarPosition()
    {
        float successStart = miniGame.GetSuccessWindowStart();
        float successEnd = miniGame.GetSuccessWindowEnd();
        float barWidth = backgroundBar.rectTransform.rect.width;
        float barStartPos = -barWidth * 0.5f;
        float startPos = barStartPos + (successStart * barWidth);
        float width = (successEnd - successStart) * barWidth;

        successBar.rectTransform.anchoredPosition = new Vector2(startPos, 0f);
        successBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    private void UpdateProgressArrow()
    {
        float barWidth = backgroundBar.rectTransform.rect.width;
        float barStartPos = -barWidth * 0.5f;
        float arrowPos = barStartPos + (miniGame.GetCurrentProgress() * barWidth);
        progressArrow.rectTransform.anchoredPosition = new Vector2(arrowPos, 0f);
    }

    private void UpdateRemainingTime()
    {
        remainingTimeText.text = $"Time: {miniGame.GetRemainingTimeNormalized():F2}";
    }
}