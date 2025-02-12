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
    private float moveSpeed = 1f;

    private float totalTime = 3f;          // 미니게임 제한 시간
    private float remainingTime;
    private float slowMotionScale = 0.3f;  // (UI 연출을 위한 슬로우 모션 비율 - 로직에는 사용하지 않음)

    public event Action<DodgeResult> OnDodgeResultReceived;
    public event Action OnMiniGameEnded;

    /// <summary>
    /// 미니게임 시작 시 성공 구간과 기본 상태를 초기화한다.
    /// 성공 구간은 항상 중앙(0.5)를 기준으로 설정되며, 난이도에 따라 너비가 좁아진다.
    /// </summary>
    public void StartDodgeMiniGame(float difficulty = 1f)
    {
        // 난이도에 따라 성공 구간의 너비 범위를 결정
        // 난이도 1에서는 너비가 상대적으로 넓고, 난이도 3에서는 좁게 설정됨
        float maxWidth = Mathf.Lerp(0.4f, 0.3f, (difficulty - 1f) / 2f);
        float minWidth = Mathf.Lerp(0.3f, 0.2f, (difficulty - 1f) / 2f);
        // 랜덤하게 너비를 선택 (이 부분을 고정값으로도 설정할 수 있음)
        float windowWidth = UnityEngine.Random.Range(minWidth, maxWidth);

        // 성공 구간을 항상 중앙에 위치하도록 설정 (중앙 0.5 기준)
        successWindowStart = 0.5f - windowWidth / 2f;
        successWindowEnd = 0.5f + windowWidth / 2f;

        // 화살표 이동 속도와 제한 시간 설정 (난이도에 따라 조정)
        moveSpeed = 1f + (difficulty - 1f);  // 난이도 1~3에 따라 2~4로 조정
        totalTime = Mathf.Lerp(3f, 1.5f, (difficulty - 1f) / 2f);  // 난이도 1~3에 따라 3~1.5초로 조정

        // 기본 상태 초기화
        currentProgress = 0f;
        isMovingRight = true;
        remainingTime = totalTime;

        // 슬로우 모션 효과 적용 (UI 연출용, 로직 업데이트는 unscaledDeltaTime 사용)
        Time.timeScale = slowMotionScale;
        AudioListener.pause = true;

        Debug.Log($"DodgeMiniGame Started - Difficulty: {difficulty}, " +
                  $"Window: {successWindowStart:F2} ~ {successWindowEnd:F2}, " +
                  $"Move Speed: {moveSpeed}, Total Time: {totalTime}");
    }

    /// <summary>
    /// 사용자가 입력을 했을 때, 현재 진행 상황과 성공 구간을 바탕으로 판정한다.
    /// </summary>
    public DodgeResult ProcessInput(float inputTiming)
    {
        // 입력 직전에 현재 상태를 로그로 출력해 UI와 로직이 일치하는지 확인
        Debug.Log($"[Input Process] InputTiming: {inputTiming:F2}, CurrentProgress: {currentProgress:F2}, " +
                  $"SuccessWindow: {successWindowStart:F2} ~ {successWindowEnd:F2}");

        DodgeResult result;
        bool isInSuccessWindow = inputTiming >= successWindowStart && inputTiming <= successWindowEnd;

        if (isInSuccessWindow)
        {
            float center = (successWindowStart + successWindowEnd) / 2f;
            float distanceFromCenter = Mathf.Abs(inputTiming - center);
            result = distanceFromCenter < 0.05f ? DodgeResult.Perfect : DodgeResult.Good;
        }
        else
        {
            result = DodgeResult.Miss;
        }

        OnDodgeResultReceived?.Invoke(result);
        EndMiniGame(result != DodgeResult.Miss);

        return result;
    }

    /// <summary>
    /// 매 프레임마다 화살표를 이동시키고, 제한 시간을 체크한다.
    /// 업데이트 시 Time.unscaledDeltaTime을 사용하여 timeScale 변경에 관계없이 일관된 시간 계산을 한다.
    /// </summary>
    public bool Update()
    {
        // 모든 로직 업데이트에 unscaledDeltaTime을 사용
        float unscaledDeltaTime = Time.unscaledDeltaTime;

        // 화살표 이동
        if (isMovingRight)
        {
            currentProgress += moveSpeed * unscaledDeltaTime;
            if (currentProgress >= 1f)
            {
                currentProgress = 1f;
                isMovingRight = false;
            }
        }
        else
        {
            currentProgress -= moveSpeed * unscaledDeltaTime;
            if (currentProgress <= 0f)
            {
                currentProgress = 0f;
                isMovingRight = true;
            }
        }

        // 제한 시간 감소 (unscaledDeltaTime 사용)
        remainingTime -= unscaledDeltaTime;

        if (remainingTime <= 0)
        {
            EndMiniGame(false);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 미니게임 종료 처리 (타임스케일 복귀, 오디오 리스너 재개 등).
    /// </summary>
    public void EndMiniGame(bool success)
    {
        // 연출이 끝났으므로 timeScale과 오디오 상태를 원복한다.
        Time.timeScale = 1f;
        AudioListener.pause = false;
        OnMiniGameEnded?.Invoke();
    }

    // -------------------------------------------------
    // Getters
    // -------------------------------------------------
    public float GetCurrentProgress() => currentProgress;          // 0 ~ 1
    public float GetSuccessWindowStart() => successWindowStart;    // 0 ~ 1
    public float GetSuccessWindowEnd() => successWindowEnd;        // 0 ~ 1
    public float GetRemainingTimeNormalized() => Mathf.Clamp01(remainingTime / totalTime);
    public float GetSlowMotionScale() => slowMotionScale;
}
