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
        // Ÿ�ӽ����� ����
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.1f;

        // CameraShakeManager�� ���� ��鸲 ȿ��
        CameraShakeManager.TriggerShake(strength, duration);

        // ��Ʈ���� ����
        yield return new WaitForSecondsRealtime(duration * 0.1f);  // ���� �ð� �������� ���

        // Ÿ�ӽ����� ����
        Time.timeScale = originalTimeScale;
    }
}