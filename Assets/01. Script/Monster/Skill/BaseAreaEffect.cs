using DG.Tweening;
using System.Collections;
using UnityEngine;

public abstract class BaseAreaEffect : MonoBehaviour, IDamageArea
{
    protected float damage;
    protected float duration;
    protected float radius;
    protected float tickRate = 1f; // 초당 데미지 적용 간격
    private Tween damageTween; // DOTween 트윈 객체
    public virtual void Initialize(float damage, float duration, float radius)
    {
        this.damage = damage;
        this.duration = duration;
        this.radius = radius;
        damageTween = DOTween.To(() => 0f, x => { }, 1f, tickRate)
         .SetLoops(Mathf.CeilToInt(duration / tickRate)) // duration 동안 반복
         .OnStepComplete(() => ApplyAreaDamage()) // 각 루프가 끝날 때마다 호출
         .OnComplete(() => Destroy(gameObject)); // 모든 루프가 끝난 후 파괴
    }

    // Coroutine을 사용하여 주기적으로 데미지를 적용

    private void OnDestroy()
    {
        damageTween?.Kill(); // 트윈 종료 및 리소스 해제
    }
    protected abstract void ApplyAreaDamage();
}