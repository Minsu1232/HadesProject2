using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DodgeMiniGameTest : MonoBehaviour
{
    public Canvas canvas;
    public Image backgroundBar;    // ��� ��
    public Image successBar;       // ���� ���� �� (�ʷϻ�)
    public Image progressArrow;    // ȭ��ǥ

    public TextMeshProUGUI remainingTimeText;
    public TextMeshProUGUI resultText;

    private DodgeMiniGame miniGame;

    void Start()
    {
        miniGame = new DodgeMiniGame();
        miniGame.OnDodgeResultReceived += HandleDodgeResult;

        // �̴ϰ��� ����
        miniGame.StartDodgeMiniGame();

        // UI Ȱ��ȭ
        canvas.gameObject.SetActive(true);

        // ���� ���� UI ��ġ
        SetSuccessBarPosition();
    }

    void Update()
    {
        // �̴ϰ��� ���� ������Ʈ
        miniGame.Update(Time.deltaTime);

        // ȭ��ǥ ��ġ ����
        UpdateProgressArrow();

        // ���� �ð� �ؽ�Ʈ ����
        UpdateRemainingTime();

        // ����� ���� Ȯ��(����)
        // Debug.Log($"Progress: {miniGame.GetCurrentProgress():F2}, " +
        //           $"SuccessStart: {miniGame.GetSuccessWindowStart():F2}, " +
        //           $"SuccessEnd: {miniGame.GetSuccessWindowEnd():F2}");

        // �����̽��� �Է� �� ����
        if (Input.GetKeyDown(KeyCode.Space))
        {
            miniGame.ProcessInput(miniGame.GetCurrentProgress());
        }
    }

    /// <summary>
    /// ���� ���� �ٸ� DodgeMiniGame�� successWindowStart, successWindowEnd ���� ���� ǥ���Ѵ�.
    /// </summary>
    void SetSuccessBarPosition()
    {
        float successStart = miniGame.GetSuccessWindowStart(); // 0~1
        float successEnd = miniGame.GetSuccessWindowEnd();     // 0~1

        // ��� ���� ���� ��
        float barWidth = backgroundBar.rectTransform.rect.width;

        // ��� �ٰ� �߽� ����(pivot�� 0.5)�̶��, -barWidth/2�� ������
        float barStartPos = -barWidth * 0.5f;

        // ���� ������ ���� ��ġ�� �� ���
        float startPos = barStartPos + (successStart * barWidth);
        float width = (successEnd - successStart) * barWidth;

        // ���� �� ��ġ�� ũ�� ����
        successBar.rectTransform.anchoredPosition = new Vector2(startPos, 0f);
        successBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    /// <summary>
    /// ���� ���� ȭ��ǥ�� DodgeMiniGame�� currentProgress(0~1)�� ���� ǥ���Ѵ�.
    /// </summary>
    void UpdateProgressArrow()
    {
        float barWidth = backgroundBar.rectTransform.rect.width;
        float barStartPos = -barWidth * 0.5f;

        // currentProgress(0~1)�� barWidth�� ��ȯ
        float arrowPos = barStartPos + (miniGame.GetCurrentProgress() * barWidth);
        progressArrow.rectTransform.anchoredPosition = new Vector2(arrowPos, 0f);
    }

    /// <summary>
    /// ���� �ð� UI�� ǥ���Ѵ�.
    /// </summary>
    void UpdateRemainingTime()
    {
        remainingTimeText.text = $"Time: {miniGame.GetRemainingTimeNormalized():F2}";
    }

    /// <summary>
    /// DodgeMiniGame�� ���� ��� �̺�Ʈ
    /// </summary>
    /// <param name="result">Perfect, Good, Miss</param>
    private void HandleDodgeResult(DodgeMiniGame.DodgeResult result)
    {
        resultText.text = $"Result: {result}";
    }
}
