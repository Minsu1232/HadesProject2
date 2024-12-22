using UnityEngine;

public interface IAttackStrategy
{
    void Attack(Transform transform, Transform target, MonsterClass monsterData);
    bool CanAttack(float distanceToTarget, MonsterClass monsterData);
    void StartAttack();
    void StopAttack();
    bool IsAttacking { get; }
    float GetLastAttackTime { get; }
}