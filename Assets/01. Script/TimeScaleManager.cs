using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleManager : Singleton<TimeScaleManager>
{
    private static TimeScaleManager instance;


    private static float originalTimeScale = 1f; // �⺻ Ÿ�ӽ�����
    private static int hitStopCount = 0; // ��ø�� ��Ʈ��ž ��
    public static void TriggerHitStop(float duration)
    {
        if (hitStopCount == 0)
        {
            originalTimeScale = Time.timeScale; // ���� ���� ����
        }

        hitStopCount++; // ��ø ī��Ʈ ����
        Time.timeScale = 0.1f; // ��Ʈ��ž ����

       Instance.StartCoroutine(Instance.HitStopCoroutine(duration));
    }

    private IEnumerator HitStopCoroutine(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);

        hitStopCount--;
        if (hitStopCount <= 0)
        {
            hitStopCount = 0; // ���� ����
            Time.timeScale = originalTimeScale; // ���� �� ����
        }
    }

    public static void ResetTimeScale()
    {
        hitStopCount = 0; // ��� ��ø �ʱ�ȭ
        Time.timeScale = originalTimeScale; // ���� �� ����
    }
}
