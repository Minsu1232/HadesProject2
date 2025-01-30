using UnityEngine;
using System;

public class DodgeMiniGame
{
    public enum DodgeResult
    {
        Perfect,
        Good,
        Miss
    }

    private float successWindowStart;
    private float successWindowEnd;
    private float currentProgress;
    private bool isMovingRight = true;
    private float moveSpeed = 2f;

    private float totalTime = 3f;          // �̴ϰ��� ���� �ð�
    private float remainingTime;
    private float slowMotionScale = 0.3f;  // ���ο� ��� ����

    public event Action<DodgeResult> OnDodgeResultReceived;
    public event Action OnMiniGameEnded;

    /// <summary>
    /// �̴ϰ��� ���� �� ���� ������ �⺻ ���¸� �ʱ�ȭ�Ѵ�.
    /// </summary>
    public void StartDodgeMiniGame(float difficulty = 1f)  // ���̵� �Ű����� �߰�
    {
        
        float maxWidth = Mathf.Lerp(0.4f, 0.3f, (difficulty - 1f) / 2f);
        float minWidth = Mathf.Lerp(0.3f, 0.2f, (difficulty - 1f) / 2f);
        float randomWidth = UnityEngine.Random.Range(minWidth, maxWidth);

        // ���� ��ġ�� ���̵��� ���� ����
        float safeSpace = 1f - randomWidth;
        successWindowStart = UnityEngine.Random.Range(0.2f, safeSpace - 0.2f);
        successWindowEnd = successWindowStart + randomWidth;

        // ȭ��ǥ �̵� �ӵ��� ���̵��� ���� ����
        moveSpeed = 2f + (difficulty - 1f);  // ���̵� 1~3�� ���� 2~4�� ����

        // ���� �ð��� ���̵��� ���� ����
        totalTime = Mathf.Lerp(3f, 1.5f, (difficulty - 1f) / 2f);  // ���̵� 1~3�� ���� 3~1.5�ʷ� ����

        // �⺻ �ʱ�ȭ
        currentProgress = 0f;
        isMovingRight = true;
        remainingTime = totalTime;

        // ���ο� ��� ����
        Time.timeScale = slowMotionScale;
        AudioListener.pause = true;

        Debug.Log($"DodgeMiniGame Started - Difficulty: {difficulty}, " +
                  $"Window Size: {randomWidth}, " +
                  $"Move Speed: {moveSpeed}, " +
                  $"Total Time: {totalTime}");
    }

    /// <summary>
    /// ����ڰ� �Է��� ���� ��, ȭ��ǥ�� ��ġ(currentProgress)�� �������� �����Ѵ�.
    /// </summary>
    public DodgeResult ProcessInput(float inputTiming)
    {
        DodgeResult result;

        // inputTiming(0~1)�� ���� ���� ���� �ִ��� Ȯ��
        bool isInSuccessWindow = inputTiming >= successWindowStart && inputTiming <= successWindowEnd;

        if (isInSuccessWindow)
        {
            float center = (successWindowStart + successWindowEnd) / 2f;
            float distanceFromCenter = Mathf.Abs(inputTiming - center);

            // �߽ɿ��� �󸶳� ��������� ���� Perfect / Good ����
            result = distanceFromCenter < 0.05f ? DodgeResult.Perfect : DodgeResult.Good;
        }
        else
        {
            result = DodgeResult.Miss;
        }

        // ��� �̺�Ʈ �� �̴ϰ��� ����
        OnDodgeResultReceived?.Invoke(result);
        EndMiniGame(result != DodgeResult.Miss);

        return result;
    }

    /// <summary>
    /// �� �����Ӹ��� ȭ��ǥ�� �̵���Ű��, ���� �ð��� üũ�Ѵ�.
    /// </summary>
    /// <param name="deltaTime">��� �ð�(��)</param>
    /// <returns>������ ��� ���� ���̸� true, ����Ǹ� false</returns>
    public bool Update(float deltaTime)
    {
        // ȭ��ǥ �̵�
        if (isMovingRight)
        {
            currentProgress += moveSpeed * deltaTime;
            if (currentProgress >= 1f)
            {
                currentProgress = 1f;
                isMovingRight = false;
            }
        }
        else
        {
            currentProgress -= moveSpeed * deltaTime;
            if (currentProgress <= 0f)
            {
                currentProgress = 0f;
                isMovingRight = true;
            }
        }

        // ���� �ð� ����(���ο� ��� ���� ���)
        remainingTime -= (deltaTime / slowMotionScale);

        // ���� �ð� �ʰ� ��, �̴ϰ��� ���� ó��
        if (remainingTime <= 0)
        {
            EndMiniGame(false);
            return false;
        }

        return true;
    }

    /// <summary>
    /// �̴ϰ��� ���� ó��(Ÿ�ӽ����� ����, ����� ������ �簳 ��).
    /// </summary>
    public void EndMiniGame(bool success)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        OnMiniGameEnded?.Invoke();
    }

    // -------------------------------------------------
    // Getters
    // -------------------------------------------------
    public float GetCurrentProgress() => currentProgress;          // 0~1
    public float GetSuccessWindowStart() => successWindowStart;    // 0~1
    public float GetSuccessWindowEnd() => successWindowEnd;        // 0~1
    public float GetRemainingTimeNormalized() => Mathf.Clamp01(remainingTime / totalTime);
    public float GetSlowMotionScale() => slowMotionScale;
}
