using UnityEngine;
using static AttackData;

public interface IEliteAbility
{
    string AbilityName { get; }
    string Description { get; }
    
    void ApplyAbility(MonsterStatus monsterStatus);  // 특성 적용시 발동
    void OnAttack(MonsterStatus monsterStatus);      // 공격시 발동
    void OnHit(MonsterStatus monsterStatus, int damage, AttackType attackType);  // 피격시 발동
    void OnUpdate(MonsterStatus monsterStatus);      // 매 프레임 체크할 것들
}