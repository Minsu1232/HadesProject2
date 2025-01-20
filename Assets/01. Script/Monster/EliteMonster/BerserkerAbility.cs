using UnityEngine;
using static AttackData;

public class BerserkerAbility : IEliteAbility
{
    private const float HEALTH_THRESHOLD = 0.5f;
    private const float ATTACK_BOOST = 0.5f;
    private bool isEnraged = false;
    public string AbilityName => "����ȭ";
    public string Description => "ü���� 50% ������ �� ���ݷ� 50% ����";
    public Color OutlineColor => Color.red;

    public void ApplyAbility(ICreatureStatus creatureStatus) { }
    public void OnAttack(ICreatureStatus creatureStatus) { }
    public void OnHit(ICreatureStatus creatureStatus, int damage, AttackType attackType)
    {
        CheckBerserkStatus(creatureStatus);
    }
    public void OnUpdate(ICreatureStatus creatureStatus)
    {
        CheckBerserkStatus(creatureStatus);
    }

    private void CheckBerserkStatus(ICreatureStatus creatureStatus)
    {
        float healthPercentage = (float)creatureStatus.GetMonsterClass().CurrentHealth / creatureStatus.GetMonsterClass().GetMonsterData().initialHp;
        if (healthPercentage <= HEALTH_THRESHOLD && !isEnraged)
        {
            isEnraged = true;
            creatureStatus.ModifyAttackPower((int)(creatureStatus.GetMonsterClass().CurrentAttackPower * ATTACK_BOOST));
        }
        else if (healthPercentage > HEALTH_THRESHOLD && isEnraged)
        {
            isEnraged = false;
            creatureStatus.ModifyAttackPower(-(int)(creatureStatus.GetMonsterClass().CurrentAttackPower * ATTACK_BOOST));
        }
    }
}