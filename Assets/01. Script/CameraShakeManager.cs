using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeManager : Singleton<CameraShakeManager>
{
    private Vector3 shakeOffset = Vector3.zero; // ��鸲 ȿ�� ������
    private Coroutine shakeCoroutine;

    public static Vector3 GetShakeOffset()
    {
        return Instance != null ? Instance.shakeOffset : Vector3.zero;
    }

    public static void TriggerShake(float intensity, float duration)
    {
        if (Instance == null)
        {
            Debug.LogError("CameraShakeManager �ν��Ͻ��� �����ϴ�.");
            return;
        }

        // ���� ��鸲 ����
        if (Instance.shakeCoroutine != null)
        {
            Instance.StopCoroutine(Instance.shakeCoroutine);
        }

        // ���ο� ��鸲 ����
        Instance.shakeCoroutine = Instance.StartCoroutine(Instance.ShakeCoroutine(intensity, duration));
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        float elapsedTime = 0f;
        float currentIntensity = intensity;

        while (elapsedTime < duration)
        {
            // �ð��� ���� ���� ����
            float percentComplete = elapsedTime / duration;
            currentIntensity = intensity * (1f - percentComplete);

            // �߰��� �� �����Ӹ��ٵ� ����
            currentIntensity *= 0.95f;

            // ��鸲 ������ ���
            float xOffset = Random.Range(-1f, 1f) * currentIntensity;
            float yOffset = Random.Range(-1f, 1f) * currentIntensity;
            shakeOffset = new Vector3(xOffset, yOffset, 0f);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero; // ��鸲 ���� �� �ʱ�ȭ
    }
}
