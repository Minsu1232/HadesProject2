using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackStrategy : IAttackStrategy
{
    private bool isAttacking;
    private float lastAttackTime;
    private float rotationSpeed = 5f; // �ʿ��ϴٸ� �����ڿ��� ���� ���� �ֽ��ϴ�

    public bool IsAttacking => isAttacking;
    public float GetLastAttackTime => lastAttackTime;
    private bool isAttackAnimation;
    public bool IsAttackAnimation => isAttackAnimation;
    public void Attack(Transform transform, Transform target, MonsterClass monsterData)
    {
        if (!CanAttack(Vector3.Distance(transform.position, target.position), monsterData))
            return;

        // ���� ���� �ÿ��� ȸ��
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

    // ������ �޼������ �״�� ����
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
    // �ִϸ��̼� �̺�Ʈ���� ȣ��� �޼���
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
