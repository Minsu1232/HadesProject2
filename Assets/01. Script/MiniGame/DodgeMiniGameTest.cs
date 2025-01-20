using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DodgeMiniGameTest : MonoBehaviour
{
    public Canvas canvas;
    public Image backgroundBar;    // 배경 바
    public Image successBar;       // 성공 구간 바 (초록색)
    public Image progressArrow;    // 화살표

    public TextMeshProUGUI remainingTimeText;
    public TextMeshProUGUI resultText;

    private DodgeMiniGame miniGame;

    void Start()
    {
        miniGame = new DodgeMiniGame();
        miniGame.OnDodgeResultReceived += HandleDodgeResult;

        // 미니게임 시작
        miniGame.StartDodgeMiniGame();

        // UI 활성화
        canvas.gameObject.SetActive(true);

        // 성공 구간 UI 배치
        SetSuccessBarPosition();
    }

    void Update()
    {
        // 미니게임 로직 업데이트
        miniGame.Update(Time.deltaTime);

        // 화살표 위치 갱신
        UpdateProgressArrow();

        // 남은 시간 텍스트 갱신
        UpdateRemainingTime();

        // 디버그 정보 확인(선택)
        // Debug.Log($"Progress: {miniGame.GetCurrentProgress():F2}, " +
        //           $"SuccessStart: {miniGame.GetSuccessWindowStart():F2}, " +
        //           $"SuccessEnd: {miniGame.GetSuccessWindowEnd():F2}");

        // 스페이스바 입력 시 판정
        if (Input.GetKeyDown(KeyCode.Space))
        {
            miniGame.ProcessInput(miniGame.GetCurrentProgress());
        }
    }

    /// <summary>
    /// 성공 구간 바를 DodgeMiniGame의 successWindowStart, successWindowEnd 값에 맞춰 표시한다.
    /// </summary>
    void SetSuccessBarPosition()
    {
        float successStart = miniGame.GetSuccessWindowStart(); // 0~1
        float successEnd = miniGame.GetSuccessWindowEnd();     // 0~1

        // 배경 바의 실제 폭
        float barWidth = backgroundBar.rectTransform.rect.width;

        // 배경 바가 중심 기준(pivot이 0.5)이라면, -barWidth/2가 시작점
        float barStartPos = -barWidth * 0.5f;

        // 성공 구간의 시작 위치와 폭 계산
        float startPos = barStartPos + (successStart * barWidth);
        float width = (successEnd - successStart) * barWidth;

        // 성공 바 위치와 크기 적용
        successBar.rectTransform.anchoredPosition = new Vector2(startPos, 0f);
        successBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    /// <summary>
    /// 진행 중인 화살표를 DodgeMiniGame의 currentProgress(0~1)에 따라 표시한다.
    /// </summary>
    void UpdateProgressArrow()
    {
        float barWidth = backgroundBar.rectTransform.rect.width;
        float barStartPos = -barWidth * 0.5f;

        // currentProgress(0~1)를 barWidth로 변환
        float arrowPos = barStartPos + (miniGame.GetCurrentProgress() * barWidth);
        progressArrow.rectTransform.anchoredPosition = new Vector2(arrowPos, 0f);
    }

    /// <summary>
    /// 남은 시간 UI를 표시한다.
    /// </summary>
    void UpdateRemainingTime()
    {
        remainingTimeText.text = $"Time: {miniGame.GetRemainingTimeNormalized():F2}";
    }

    /// <summary>
    /// DodgeMiniGame의 판정 결과 이벤트
    /// </summary>
    /// <param name="result">Perfect, Good, Miss</param>
    private void HandleDodgeResult(DodgeMiniGame.DodgeResult result)
    {
        resultText.text = $"Result: {result}";
    }
}
