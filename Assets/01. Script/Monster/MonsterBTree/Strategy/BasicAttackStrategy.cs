using UnityEngine;

public class BasicAttackStrategy : BasePhysicalAttackStrategy
{
    public override PhysicalAttackType AttackType => PhysicalAttackType.Basic;

    public override void Attack(Transform transform, Transform target, MonsterClass monsterData)
    {
        if (!CanAttack(Vector3.Distance(transform.position, target.position), monsterData))
            return;

        FaceTarget(transform, target);
        isAttackAnimation = true;
        lastAttackTime = Time.time;
        isAttacking = true;
    }
}