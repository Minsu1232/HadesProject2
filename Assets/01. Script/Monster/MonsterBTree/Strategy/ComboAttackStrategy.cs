using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboAttackStrategy : BasePhysicalAttackStrategy
{
    public override PhysicalAttackType AttackType => PhysicalAttackType.Combo;
    // Start is called before the first frame update
    public override void Attack(Transform transform, Transform target, IMonsterClass monsterData)
    {
        if (!CanAttack(Vector3.Distance(transform.position, target.position), monsterData))
            return;

        StartAttack();
        FaceTarget(transform, target);
    }
}
