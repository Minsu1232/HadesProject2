using UnityEngine;
using static AttackData;

public class BerserkerAbility : IEliteAbility
{
    private const float HEALTH_THRESHOLD = 0.5f;
    private const float ATTACK_BOOST = 0.5f;
    private bool isEnraged = false;

    public string AbilityName => "광폭화";
    public string Description => "체력이 50% 이하일 때 공격력 50% 증가";
    public Color OutlineColor => Color.red;

    public void ApplyAbility(MonsterStatus monsterStatus)
    {
        // 초기화시 필요한 처리
    }

    public void OnAttack(MonsterStatus monsterStatus)
    {
        // 공격시 처리
    }

    public void OnHit(MonsterStatus monsterStatus, int damage, AttackType attackType)
    {
        CheckBerserkStatus(monsterStatus);
    }

    public void OnUpdate(MonsterStatus monsterStatus)
    {
        CheckBerserkStatus(monsterStatus);
    }

    private void CheckBerserkStatus(MonsterStatus monsterStatus)
    {
        float healthPercentage = (float)monsterStatus.CurrentHealth / monsterStatus.GetMonsterClass().GetMonsterData().initialHp;

        if (healthPercentage <= HEALTH_THRESHOLD && !isEnraged)
        {
            isEnraged = true;
            monsterStatus.ModifyAttackPower((int)(monsterStatus.CurrentAttackPower * ATTACK_BOOST));
        }
        else if (healthPercentage > HEALTH_THRESHOLD && isEnraged)
        {
            isEnraged = false;
            monsterStatus.ModifyAttackPower(-(int)(monsterStatus.CurrentAttackPower * ATTACK_BOOST));
        }
    }
}