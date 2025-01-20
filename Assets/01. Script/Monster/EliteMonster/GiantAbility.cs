using UnityEngine;
using static AttackData;

public class GiantAbility : IEliteAbility
{
    private const float SIZE_INCREASE = 0.5f;
    private const float RANGE_INCREASE = 0.3f;

    public string AbilityName => "거대화";
    public string Description => "크기 50% 증가, 공격범위 30% 증가";


    public void ApplyAbility(ICreatureStatus creatureStatus)
    {
        // Transform은 MonoBehaviour에서 가져와야 하므로 캐스팅 필요
        MonsterStatus monsterStatus = creatureStatus as MonsterStatus;
        if (monsterStatus == null) return;

        monsterStatus.transform.localScale *= (1 + SIZE_INCREASE);

        // ICreatureStatus를 통해 접근
        IMonsterClass monster = creatureStatus.GetMonsterClass();
        float currentAttackRange = monster.GetMonsterData().attackRange;
        float currentSkillRange = monster.GetMonsterData().skillRange;

        creatureStatus.ModifyAttackRange(currentAttackRange * RANGE_INCREASE);
        creatureStatus.ModifySkillRange(currentSkillRange * RANGE_INCREASE);

        // ShockwaveRadius는 데이터 인터페이스에 추가 필요
        if (monster.GetMonsterData() is MonsterData monsterData)
        {
            monsterData.shockwaveRadius *= (1 + SIZE_INCREASE);
        }
    }

    public void OnAttack(ICreatureStatus creatureStatus) { }
    public void OnHit(ICreatureStatus creatureStatus, int damage, AttackType attackType) { }
    public void OnUpdate(ICreatureStatus creatureStatus) { }
}
