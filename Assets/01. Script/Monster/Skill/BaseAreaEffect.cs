using DG.Tweening;
using System.Collections;
using UnityEngine;

public abstract class BaseAreaEffect : MonoBehaviour, IDamageArea
{
    protected float damage;
    protected float duration;
    protected float radius;
    protected float tickRate = 1f; // �ʴ� ������ ���� ����
    private Tween damageTween; // DOTween Ʈ�� ��ü
    public virtual void Initialize(float damage, float duration, float radius)
    {
        this.damage = damage;
        this.duration = duration;
        this.radius = radius;
        damageTween = DOTween.To(() => 0f, x => { }, 1f, tickRate)
         .SetLoops(Mathf.CeilToInt(duration / tickRate)) // duration ���� �ݺ�
         .OnStepComplete(() => ApplyAreaDamage()) // �� ������ ���� ������ ȣ��
         .OnComplete(() => Destroy(gameObject)); // ��� ������ ���� �� �ı�
    }

    // Coroutine�� ����Ͽ� �ֱ������� �������� ����

    private void OnDestroy()
    {
        damageTween?.Kill(); // Ʈ�� ���� �� ���ҽ� ����
    }
    protected abstract void ApplyAreaDamage();
}