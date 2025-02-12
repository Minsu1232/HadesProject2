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
    [SerializeField] private Image perfectBar;
    [SerializeField] private Image progressArrow;
    [SerializeField] private TextMeshProUGUI remainingTimeText;
    [SerializeField] private TextMeshProUGUI resultText;

    private Coroutine fadeOutCoroutine;

    private DodgeMiniGameWrapper miniGame;

    private void Awake()
    {
        var miniGameManager = FindObjectOfType<MiniGameManager>();
        if (miniGameManager != null)
        {
            miniGameManager.RegisterUI(MiniGameType.Dodge, this);
           
        }
        else
        {
            Debug.LogError("Failed to register UI. MiniGameManager is null.");
        }
    }

    public void Initialize(DodgeMiniGameWrapper game)
    {
       
        miniGame = game;
        canvas.gameObject.SetActive(true);
        //miniGame.OnDodgeResultReceived += ShowResult;
        StartCoroutine(DelayedSetup());
       
    }
    private void ShowResult(DodgeMiniGame.DodgeResult result)
    {
        // 이전 페이드아웃이 진행 중이었다면 중지
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
        }

        // 결과에 따른 텍스트와 색상 설정
        switch (result)
        {
            case DodgeMiniGame.DodgeResult.Perfect:
                resultText.text = "Perfect!";
                resultText.color = Color.yellow;
                break;
            case DodgeMiniGame.DodgeResult.Good:
                resultText.text = "Good!";
                resultText.color = Color.green;
                break;
            default:
                resultText.text = "Miss!";
                resultText.color = Color.red;
                break;
        }

        // 페이드아웃 코루틴 시작
        fadeOutCoroutine = StartCoroutine(FadeOutResult());
    }
    private IEnumerator FadeOutResult()
    {
        // 결과를 1초간 보여줌
        yield return new WaitForSecondsRealtime(1f);

        // 0.5초에 걸쳐 페이드아웃
        float duration = 0.5f;
        float elapsed = 0f;
        Color startColor = resultText.color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            resultText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        resultText.text = "";
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

        // 수정: 성공바의 피벗이 0.5이므로, 왼쪽 끝 좌표에 width/2를 더해 중앙 위치를 지정합니다.
        successBar.rectTransform.anchoredPosition = new Vector2(startPos + width * 0.5f, 0f);
        successBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);



       

        // Perfect 구역 설정
        float center = (successStart + successEnd) / 2f;
        float perfectWidth = 0.1f;  // Perfect 판정 구역 너비 (전체 바 기준 10%)
        float perfectStart = center - perfectWidth / 2f;
        float perfectBarWidth = perfectWidth * barWidth;
        float perfectStartPos = barStartPos + (perfectStart * barWidth);

        perfectBar.rectTransform.anchoredPosition = new Vector2(perfectStartPos + perfectBarWidth * 0.5f, 0f);
        perfectBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, perfectBarWidth);
    }

    private void UpdateProgressArrow()
    {
        float barWidth = backgroundBar.rectTransform.rect.width;
        float barStartPos = -barWidth * 0.5f;
        float progress = miniGame.GetCurrentProgress();
        float arrowPos = barStartPos + (progress * barWidth);
        progressArrow.rectTransform.anchoredPosition = new Vector2(arrowPos, progressArrow.rectTransform.anchoredPosition.y);
       
    }

    private void UpdateRemainingTime()
    {
        remainingTimeText.text = $"Time: {miniGame.GetRemainingTimeNormalized():F2}";
    }
    private void OnDestroy()
    {
        if (miniGame != null)
        {
            //miniGame.OnDodgeResultReceived -= ShowResult;
        }
    }
}