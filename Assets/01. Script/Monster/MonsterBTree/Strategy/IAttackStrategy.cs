// ���� ���� Ÿ�� ����
using UnityEngine;

public enum PhysicalAttackType
{
    Basic,      // �⺻ ���� (���� ��ġ�� ��)
    Jump,       // ���� ����
    Charge,
    Combo,// ���� ����
    Spin,       // ȸ�� ����
    // �߰� ����
}

// Ȯ��� ���� ���� �������̽�
public interface IAttackStrategy
{
    void Attack(Transform transform, Transform target, IMonsterClass monsterData);
    bool CanAttack(float distanceToTarget, IMonsterClass monsterData);
    void StartAttack();
    void StopAttack();
    void ApplyDamage(IDamageable target, IMonsterClass monsterData);
    bool IsAttacking { get; }
    float GetLastAttackTime { get; }
    void OnAttackAnimationEnd();
    void ResetAttackTime();
    // ���� �߰��Ǵ� �Ӽ���
    PhysicalAttackType AttackType { get; }
    string GetAnimationTriggerName();
    float GetAttackPowerMultiplier(); // ���ݷ� ����
}