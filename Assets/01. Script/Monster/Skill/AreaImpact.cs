// 일반화된 영역 생성 효과
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
            Debug.Log("독독독");
            damageArea.Initialize(damage, duration, radius);
        }
    }
}