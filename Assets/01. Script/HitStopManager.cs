using System.Collections;
using UnityEngine;

public class HitStopManager : Singleton<HitStopManager>
{
    public static void TriggerHitStop(float duration, float strength)
    {
        if (Instance == null)
        {
            Debug.LogError("HitStopManager �ν��Ͻ��� �����ϴ�.");
            return;
        }

        Instance.StartCoroutine(Instance.HitStopEffect(duration, strength));
    }

    private IEnumerator HitStopEffect(float duration, float strength)
    {
        // TimeScale ����
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.1f;  // ���� ���ο� ��� ȿ��

        // ī�޶� ��鸲 ����
        CameraShakeManager.TriggerShake(strength, duration);

        // ��Ʈ���� ����
        yield return new WaitForSecondsRealtime(duration);

        // TimeScale ����
        Time.timeScale = originalTimeScale;
    }
}