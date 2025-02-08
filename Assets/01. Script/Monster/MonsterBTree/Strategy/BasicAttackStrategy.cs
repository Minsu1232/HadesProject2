using DG.Tweening;
using GSpawn_Pro;
using UnityEngine;

public class BasicAttackStrategy : BasePhysicalAttackStrategy
{
    public override PhysicalAttackType AttackType => PhysicalAttackType.Basic;

    public BasicAttackStrategy() { lastAttackTime = Time.time - 100; }

    public override void Attack(Transform transform, Transform target, IMonsterClass monsterData)
    {
        FaceTarget(transform, target);
        isAttackAnimation = true;
       
        isAttacking = true;

        // 일정 시간이 지난 후 isAttacking을 false로 변경
        DOVirtual.DelayedCall(0.5f, () =>
        {
            lastAttackTime = Time.time;
            isAttacking = false;
            isAttackAnimation =false;
        });

        //Debug.Log($"after {isAttackAnimation} {lastAttackTime} {isAttacking}");
    }
}
