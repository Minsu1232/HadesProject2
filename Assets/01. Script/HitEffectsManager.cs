using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffectsManager : MonoBehaviour
{
    public void TriggerHitStop(float duration)
    {
        TimeScaleManager.TriggerHitStop(duration); // 전역 타임스케일 호출
    }

    public void TriggerShake(float intensity, float duration)
    {
        StartCoroutine(ShakeCoroutine(intensity, duration));
    }
   

    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        float elapsedTime = 0f;
        Vector3 originalPosition = transform.position;

        while (elapsedTime < duration)
        {
            float xOffset = Random.Range(-1f, 1f) * intensity;
            float zOffset = Random.Range(-1f, 1f) * intensity;

            transform.position = new Vector3(originalPosition.x + xOffset, originalPosition.y, originalPosition.z + zOffset);

            intensity *= 0.9f; // 흔들림 점차 감소
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition; // 원래 위치 복구
    }

  
}
