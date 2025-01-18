using UnityEngine;
using static AttackData;

public class GiantAbility : IEliteAbility
{
    private const float SIZE_INCREASE = 0.5f;
    private const float RANGE_INCREASE = 0.3f;

    public string AbilityName => "�Ŵ�ȭ";
    public string Description => "ũ�� 50% ����, ���ݹ��� 30% ����";
   

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
