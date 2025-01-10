using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackStrategy : IAttackStrategy
{
    private bool isAttacking;
    private float lastAttackTime;
    private float rotationSpeed = 5f; // 필요하다면 생성자에서 받을 수도 있습니다

    public bool IsAttacking => isAttacking;
    public float GetLastAttackTime => lastAttackTime;
    private bool isAttackAnimation;
    public bool IsAttackAnimation => isAttackAnimation;
    public void Attack(Transform transform, Transform target, MonsterClass monsterData)
    {
        if (!CanAttack(Vector3.Distance(transform.position, target.position), monsterData))
            return;

        // 공격 시작 시에만 회전
        if (!isAttackAnimation)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            if (directionToTarget != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(directionToTarget);
            }
            isAttackAnimation = true;
        }

        lastAttackTime = Time.time;
        isAttacking = true;
      
    }

    // 나머지 메서드들은 그대로 유지
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
    // 애니메이션 이벤트에서 호출될 메서드
    public void OnAttackAnimationEnd()
    {
        isAttackAnimation = false;
        isAttacking = false;
    }
    public void ApplyDamage(IDamageable target, MonsterClass monsterData)
    {
        int damageAmount = monsterData.CurrentAttackPower;
        target.TakeDamage(damageAmount,
            monsterData.IsBasicAttack() ? AttackData.AttackType.Normal : AttackData.AttackType.Charge);
    }
}
