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

    private float totalTime = 3f;          // 미니게임 제한 시간
    private float remainingTime;
    private float slowMotionScale = 0.3f;  // 슬로우 모션 비율

    public event Action<DodgeResult> OnDodgeResultReceived;
    public event Action OnMiniGameEnded;

    /// <summary>
    /// 미니게임 시작 시 성공 구간과 기본 상태를 초기화한다.
    /// </summary>
    public void StartDodgeMiniGame()
    {
        // 0~1 범위 중에서 성공 구간을 잡는다. (예: 최대 0.6 폭까지)
        float randomWidth = UnityEngine.Random.Range(0.2f, 0.4f);
        successWindowStart = UnityEngine.Random.Range(0.2f, 0.8f - randomWidth);
        successWindowEnd = successWindowStart + randomWidth;

        // 화살표 초기 상태
        currentProgress = 0f;
        isMovingRight = true;
        remainingTime = totalTime;

        // 슬로우 모션 적용
        Time.timeScale = slowMotionScale;
        AudioListener.pause = true;
    }

    /// <summary>
    /// 사용자가 입력을 했을 때, 화살표의 위치(currentProgress)를 바탕으로 판정한다.
    /// </summary>
    public DodgeResult ProcessInput(float inputTiming)
    {
        DodgeResult result;

        // inputTiming(0~1)이 성공 구간 내에 있는지 확인
        bool isInSuccessWindow = inputTiming >= successWindowStart && inputTiming <= successWindowEnd;

        if (isInSuccessWindow)
        {
            float center = (successWindowStart + successWindowEnd) / 2f;
            float distanceFromCenter = Mathf.Abs(inputTiming - center);

            // 중심에서 얼마나 가까운지에 따라 Perfect / Good 판정
            result = distanceFromCenter < 0.05f ? DodgeResult.Perfect : DodgeResult.Good;
        }
        else
        {
            result = DodgeResult.Miss;
        }

        // 결과 이벤트 및 미니게임 종료
        OnDodgeResultReceived?.Invoke(result);
        EndMiniGame(result != DodgeResult.Miss);

        return result;
    }

    /// <summary>
    /// 매 프레임마다 화살표를 이동시키고, 제한 시간을 체크한다.
    /// </summary>
    /// <param name="deltaTime">경과 시간(초)</param>
    /// <returns>게임이 계속 진행 중이면 true, 종료되면 false</returns>
    public bool Update(float deltaTime)
    {
        // 화살표 이동
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

        // 제한 시간 감소(슬로우 모션 상태 고려)
        remainingTime -= (deltaTime / slowMotionScale);

        // 제한 시간 초과 시, 미니게임 실패 처리
        if (remainingTime <= 0)
        {
            EndMiniGame(false);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 미니게임 종료 처리(타임스케일 복귀, 오디오 리스너 재개 등).
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
