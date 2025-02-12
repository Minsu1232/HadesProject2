using UnityEngine;

public class BasicAttackStrategy : BasePhysicalAttackStrategy
{
    public override PhysicalAttackType AttackType => PhysicalAttackType.Basic;

    public BasicAttackStrategy()
    {
        
    }

    public override void Attack(Transform transform, Transform target, IMonsterClass monsterData)
    {
        if (!CanAttack(Vector3.Distance(transform.position, target.position), monsterData))
            return;

        StartAttack();
        FaceTarget(transform, target);
    }

    public override bool CanAttack(float distanceToTarget, IMonsterClass monsterData)
    {
        return base.CanAttack(distanceToTarget, monsterData);
    }
}