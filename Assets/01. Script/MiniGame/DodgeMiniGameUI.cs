// ���� �̴ϰ��� UI
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
        // ���� ���̵�ƿ��� ���� ���̾��ٸ� ����
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
        }

        // ����� ���� �ؽ�Ʈ�� ���� ����
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

        // ���̵�ƿ� �ڷ�ƾ ����
        fadeOutCoroutine = StartCoroutine(FadeOutResult());
    }
    private IEnumerator FadeOutResult()
    {
        // ����� 1�ʰ� ������
        yield return new WaitForSecondsRealtime(1f);

        // 0.5�ʿ� ���� ���̵�ƿ�
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
        // �� ������ ���
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

        // ����: �������� �ǹ��� 0.5�̹Ƿ�, ���� �� ��ǥ�� width/2�� ���� �߾� ��ġ�� �����մϴ�.
        successBar.rectTransform.anchoredPosition = new Vector2(startPos + width * 0.5f, 0f);
        successBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);



       

        // Perfect ���� ����
        float center = (successStart + successEnd) / 2f;
        float perfectWidth = 0.1f;  // Perfect ���� ���� �ʺ� (��ü �� ���� 10%)
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