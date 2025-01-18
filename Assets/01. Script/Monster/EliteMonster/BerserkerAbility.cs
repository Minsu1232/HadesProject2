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

    public void ApplyAbility(MonsterStatus monsterStatus)
    {
        // �ʱ�ȭ�� �ʿ��� ó��
    }

    public void OnAttack(MonsterStatus monsterStatus)
    {
        // ���ݽ� ó��
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