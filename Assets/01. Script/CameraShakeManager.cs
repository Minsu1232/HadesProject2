using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeManager : Singleton<CameraShakeManager>
{
    private Vector3 shakeOffset = Vector3.zero; // 흔들림 효과 오프셋
    private Coroutine shakeCoroutine;

    public static Vector3 GetShakeOffset()
    {
        return Instance != null ? Instance.shakeOffset : Vector3.zero;
    }

    public static void TriggerShake(float intensity, float duration)
    {
        if (Instance == null)
        {
            Debug.LogError("CameraShakeManager 인스턴스가 없습니다.");
            return;
        }

        // 기존 흔들림 종료
        if (Instance.shakeCoroutine != null)
        {
            Instance.StopCoroutine(Instance.shakeCoroutine);
        }

        // 새로운 흔들림 시작
        Instance.shakeCoroutine = Instance.StartCoroutine(Instance.ShakeCoroutine(intensity, duration));
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        float elapsedTime = 0f;
        float currentIntensity = intensity;

        while (elapsedTime < duration)
        {
            // 시간에 따라 강도 감소
            float percentComplete = elapsedTime / duration;
            currentIntensity = intensity * (1f - percentComplete);

            // 추가로 매 프레임마다도 감소
            currentIntensity *= 0.95f;

            // 흔들림 오프셋 계산
            float xOffset = Random.Range(-1f, 1f) * currentIntensity;
            float yOffset = Random.Range(-1f, 1f) * currentIntensity;
            shakeOffset = new Vector3(xOffset, yOffset, 0f);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero; // 흔들림 종료 후 초기화
    }
}
