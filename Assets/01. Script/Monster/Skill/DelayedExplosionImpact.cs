using UnityEngine;

public class DelayedExplosionImpact : IProjectileImpact
{
    private float safeZoneRadius;
    private float dangerRadius;
    private bool isRingShaped;
    private GameObject explosionEffect;
    private float explosionDelay;

    public DelayedExplosionImpact(float safeZoneRadius, float dangerRadiusMultiplier, float explosionDelay, bool isRingShaped, GameObject explosionEffect)
    {
        this.safeZoneRadius = safeZoneRadius;
        this.dangerRadius = safeZoneRadius * dangerRadiusMultiplier;
        this.explosionDelay = explosionDelay;
        this.isRingShaped = isRingShaped;
        this.explosionEffect = explosionEffect;
    }

    // 인터페이스 메서드 이름에 맞게 수정
    public void OnImpact(Vector3 impactPosition, float damage)
    {
        // 지연 폭발을 처리할 객체 생성
        GameObject delayedExplosion = new GameObject("DelayedExplosion");
        delayedExplosion.transform.position = impactPosition;

        // 지연 폭발 컴포넌트 추가
        ExplosionController controller = delayedExplosion.AddComponent<ExplosionController>();
        controller.Initialize(safeZoneRadius, dangerRadius, explosionDelay, isRingShaped, explosionEffect, damage);
    }
}