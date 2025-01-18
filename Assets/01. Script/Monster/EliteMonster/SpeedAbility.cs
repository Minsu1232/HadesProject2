using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackData;

public class SpeedAbility : IEliteAbility
{
    private const float SPEED_BONUS = 0.3f;
    private const float ATTACK_SPEED_BONUS = 0.2f;

    public string AbilityName => "�ż�";
    public string Description => "�̵��ӵ� 30%, ���ݼӵ� 20% ����";
    public Color OutlineColor => Color.cyan;

    public void ApplyAbility(MonsterStatus monsterStatus)
    {
        monsterStatus.ModifySpeed((int)(monsterStatus.CurrentSpeed * SPEED_BONUS));
        monsterStatus.ModifyAttackSpeed(ATTACK_SPEED_BONUS);
    }

    public void OnAttack(MonsterStatus monsterStatus) { }
    public void OnHit(MonsterStatus monsterStatus, int damage, AttackType attackType) { }
    public void OnUpdate(MonsterStatus monsterStatus) { }
}
