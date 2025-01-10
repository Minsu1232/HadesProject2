// 老馆拳等 康开 积己 瓤苞
using UnityEngine;

public class AreaImpact : IProjectileImpact
{
    private GameObject areaEffectPrefab;
    private float duration;
    private float radius;

    public AreaImpact(GameObject areaEffectPrefab, float duration, float radius)
    {
        this.areaEffectPrefab = areaEffectPrefab;
        this.duration = duration;
        this.radius = radius;
    }

    public void OnImpact(Vector3 impactPosition, float damage)
    {
        if (areaEffectPrefab == null) return;

        GameObject areaEffect = Object.Instantiate(areaEffectPrefab, impactPosition, Quaternion.identity);
        if (areaEffect.TryGetComponent<IDamageArea>(out var damageArea))
        {
            damageArea.Initialize(damage, duration, radius);
        }
    }
}