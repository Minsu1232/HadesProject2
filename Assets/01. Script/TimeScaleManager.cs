using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleManager : Singleton<TimeScaleManager>
{
    private static TimeScaleManager instance;


    private static float originalTimeScale = 1f; // 기본 타임스케일
    private static int hitStopCount = 0; // 중첩된 히트스탑 수
    public static void TriggerHitStop(float duration)
    {
        if (hitStopCount == 0)
        {
            originalTimeScale = Time.timeScale; // 현재 상태 저장
        }

        hitStopCount++; // 중첩 카운트 증가
        Time.timeScale = 0.1f; // 히트스탑 실행

       Instance.StartCoroutine(Instance.HitStopCoroutine(duration));
    }

    private IEnumerator HitStopCoroutine(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);

        hitStopCount--;
        if (hitStopCount <= 0)
        {
            hitStopCount = 0; // 음수 방지
            Time.timeScale = originalTimeScale; // 원래 값 복원
        }
    }

    public static void ResetTimeScale()
    {
        hitStopCount = 0; // 모든 중첩 초기화
        Time.timeScale = originalTimeScale; // 원래 값 복원
    }
}
