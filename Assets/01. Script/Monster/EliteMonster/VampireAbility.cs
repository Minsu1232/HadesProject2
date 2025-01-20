using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackData;

public class VampireAbility : IEliteAbility
{
    private const float LIFESTEAL_PERCENT = 0.2f;
    public string AbilityName => "����";
    public string Description => "���ݷ��� 20%��ŭ ü�� ���";
    public Color OutlineColor => Color.black;

    public void ApplyAbility(ICreatureStatus creatureStatus) { }
    public void OnAttack(ICreatureStatus creatureStatus)
    {
        int healAmount = (int)(creatureStatus.GetMonsterClass().CurrentAttackPower * LIFESTEAL_PERCENT);
        creatureStatus.ModifyHealth(healAmount);
    }
    public void OnHit(ICreatureStatus creatureStatus, int damage, AttackType attackType) { }
    public void OnUpdate(ICreatureStatus creatureStatus) { }
}
