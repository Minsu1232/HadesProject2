using UnityEngine;
using static AttackData;

public class GiantAbility : IEliteAbility
{
    private const float SIZE_INCREASE = 0.5f;
    private const float RANGE_INCREASE = 0.3f;

    public string AbilityName => "�Ŵ�ȭ";
    public string Description => "ũ�� 50% ����, ���ݹ��� 30% ����";


    public void ApplyAbility(ICreatureStatus creatureStatus)
    {
        // Transform�� MonoBehaviour���� �����;� �ϹǷ� ĳ���� �ʿ�
        MonsterStatus monsterStatus = creatureStatus as MonsterStatus;
        if (monsterStatus == null) return;

        monsterStatus.transform.localScale *= (1 + SIZE_INCREASE);

        // ICreatureStatus�� ���� ����
        IMonsterClass monster = creatureStatus.GetMonsterClass();
        float currentAttackRange = monster.GetMonsterData().attackRange;
        float currentSkillRange = monster.GetMonsterData().skillRange;

        creatureStatus.ModifyAttackRange(currentAttackRange * RANGE_INCREASE);
        creatureStatus.ModifySkillRange(currentSkillRange * RANGE_INCREASE);

        // ShockwaveRadius�� ������ �������̽��� �߰� �ʿ�
        if (monster.GetMonsterData() is MonsterData monsterData)
        {
            monsterData.shockwaveRadius *= (1 + SIZE_INCREASE);
        }
    }

    public void OnAttack(ICreatureStatus creatureStatus) { }
    public void OnHit(ICreatureStatus creatureStatus, int damage, AttackType attackType) { }
    public void OnUpdate(ICreatureStatus creatureStatus) { }
}
