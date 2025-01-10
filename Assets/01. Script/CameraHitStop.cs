using System.Collections;
using UnityEngine;

public class CameraHitStop : MonoBehaviour
{
    private static CameraHitStop instance;
    public static CameraHitStop Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CameraHitStop>();
                if (instance == null)
                {
                    GameObject go = new GameObject("CameraHitStop");
                    instance = go.AddComponent<CameraHitStop>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void HitStop(float duration, float strength)
    {
        StartCoroutine(HitStopEffect(duration, strength));
    }

    private IEnumerator HitStopEffect(float duration, float strength)
    {
        // 타임스케일 조정
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.1f;

        // CameraShakeManager를 통한 흔들림 효과
        CameraShakeManager.TriggerShake(strength, duration);

        // 히트스톱 지속
        yield return new WaitForSecondsRealtime(duration * 0.1f);  // 실제 시간 기준으로 대기

        // 타임스케일 복구
        Time.timeScale = originalTimeScale;
    }
}