using UnityEngine;
using static AttackData;

public class GiantAbility : IEliteAbility
{
    private const float SIZE_INCREASE = 0.5f;
    private const float RANGE_INCREASE = 0.3f;

    public string AbilityName => "거대화";
    public string Description => "크기 50% 증가, 공격범위 30% 증가";
   

    public void ApplyAbility(MonsterStatus monsterStatus)
    {
        monsterStatus.transform.localScale *= (1 + SIZE_INCREASE);
        monsterStatus.ModifyAttackRange(monsterStatus.CurrentAttackRange * RANGE_INCREASE);
        monsterStatus.ModifySkillRange(monsterStatus.CurrentSkillRange * RANGE_INCREASE);
        monsterStatus.GetMonsterClass().GetMonsterData().shockwaveRadius *= (1 + SIZE_INCREASE);
        
    }

    public void OnAttack(MonsterStatus monsterStatus) { }
    public void OnHit(MonsterStatus monsterStatus, int damage, AttackType attackType) { }
    public void OnUpdate(MonsterStatus monsterStatus) { }
}
