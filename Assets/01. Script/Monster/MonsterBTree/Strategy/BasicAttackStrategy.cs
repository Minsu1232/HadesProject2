using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackStrategy : IAttackStrategy
{
    private bool isAttacking;
    private float lastAttackTime;

    public bool IsAttacking => isAttacking;
    public float GetLastAttackTime => lastAttackTime;

    public void Attack(Transform transform, Transform target, MonsterClass monsterData)
    {
        if (!CanAttack(Vector3.Distance(transform.position, target.position), monsterData))
            return;
        
        lastAttackTime = Time.time;
        isAttacking = true;


        isAttacking = false;
    }

    public bool CanAttack(float distanceToTarget, MonsterClass monsterData)
    {
        return distanceToTarget <= monsterData.CurrentAttackRange &&
               Time.time >= lastAttackTime + monsterData.CurrentAttackSpeed;
    }

    public void StartAttack()
    {
        isAttacking = true;
    }

    public void StopAttack()
    {
        isAttacking = false;
    }

    public void ApplyDamage(IDamageable target, MonsterClass monsterData)
    {
        int damageAmount = monsterData.CurrentAttackPower;
        target.TakeDamage(damageAmount,
            monsterData.IsBasicAttack() ? AttackData.AttackType.Normal : AttackData.AttackType.Charge);
    }
}
