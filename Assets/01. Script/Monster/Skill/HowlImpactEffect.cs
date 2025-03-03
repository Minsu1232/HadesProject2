using UnityEngine;

public class HowlImpactEffect : IProjectileImpact
{
    private float damage;
    private float essenceAmount;
    private float radius;
    ICreatureStatus monsterStatus;
    public HowlImpactEffect(float damage, float essenceAmount, float radius, ICreatureStatus monsterStatus)
    {
        this.damage = damage;
        this.essenceAmount = essenceAmount;
        this.radius = radius;
        this.monsterStatus = monsterStatus;

    }

    public void OnImpact(Vector3 impactPosition, float damage)
    {
        Debug.Log("★★★ Howl Impact Effect 발동 - 효과 적용! ★★★");

        // 범위 내 플레이어 감지
        Collider[] hitColliders = Physics.OverlapSphere(
            impactPosition,
            radius,
            LayerMask.GetMask("Player")
        );

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                var bossMonster = monsterStatus.GetMonsterClass() as AlexanderBoss;
                if (bossMonster != null)
                {
                    bossMonster.InflictEssence(essenceAmount);
                    Debug.Log($"울부짖음으로 Essence {essenceAmount} 증가!");

                    // 플레이어에게 데미지 적용
                    var playerClass = GameInitializer.Instance.GetPlayerClass();
                    playerClass.TakeDamage((int)damage);
                    return;
                }
            }
        }
    }
}
