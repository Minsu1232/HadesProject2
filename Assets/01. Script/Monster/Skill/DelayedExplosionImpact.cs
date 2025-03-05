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

    // �������̽� �޼��� �̸��� �°� ����
    public void OnImpact(Vector3 impactPosition, float damage)
    {
        // ���� ������ ó���� ��ü ����
        GameObject delayedExplosion = new GameObject("DelayedExplosion");
        delayedExplosion.transform.position = impactPosition;

        // ���� ���� ������Ʈ �߰�
        ExplosionController controller = delayedExplosion.AddComponent<ExplosionController>();
        controller.Initialize(safeZoneRadius, dangerRadius, explosionDelay, isRingShaped, explosionEffect, damage);
    }
}