using System.Collections;
using UnityEngine;

public class HitStopManager : Singleton<HitStopManager>
{
    public static void TriggerHitStop(float duration, float strength)
    {
        if (Instance == null)
        {
            Debug.LogError("HitStopManager 인스턴스가 없습니다.");
            return;
        }

        Instance.StartCoroutine(Instance.HitStopEffect(duration, strength));
    }

    private IEnumerator HitStopEffect(float duration, float strength)
    {
        // TimeScale 조절
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.1f;  // 강한 슬로우 모션 효과

        // 카메라 흔들림 적용
        CameraShakeManager.TriggerShake(strength, duration);

        // 히트스톱 지속
        yield return new WaitForSecondsRealtime(duration);

        // TimeScale 복구
        Time.timeScale = originalTimeScale;
    }
}