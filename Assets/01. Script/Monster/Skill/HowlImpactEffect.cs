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
        Debug.Log("�ڡڡ� Howl Impact Effect �ߵ� - ȿ�� ����! �ڡڡ�");

        // ���� �� �÷��̾� ����
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
                    Debug.Log($"���¢������ Essence {essenceAmount} ����!");

                    // �÷��̾�� ������ ����
                    var playerClass = GameInitializer.Instance.GetPlayerClass();
                    playerClass.TakeDamage((int)damage);
                    return;
                }
            }
        }
    }
}
