// 물리 공격 타입 정의
using UnityEngine;

public enum PhysicalAttackType
{
    Basic,      // 기본 공격 (몸통 박치기 등)
    Jump,       // 점프 공격
    Charge,     // 돌진 공격
    Spin,       // 회전 공격
    // 추가 가능
}

// 확장된 공격 전략 인터페이스
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

    // 새로 추가되는 속성들
    PhysicalAttackType AttackType { get; }
    string GetAnimationTriggerName();
    float GetAttackPowerMultiplier(); // 공격력 배율
}