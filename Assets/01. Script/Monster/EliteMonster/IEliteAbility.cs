using UnityEngine;
using static AttackData;

public interface IEliteAbility
{
    string AbilityName { get; }
    string Description { get; }

    void ApplyAbility(ICreatureStatus creatureStatus);  // 특성 적용시 발동
    void OnAttack(ICreatureStatus creatureStatus);      // 공격시 발동
    void OnHit(ICreatureStatus creatureStatus, int damage, AttackType attackType);  // 피격시 발동
    void OnUpdate(ICreatureStatus creatureStatus);      // 매 프레임 체크할 것들
}