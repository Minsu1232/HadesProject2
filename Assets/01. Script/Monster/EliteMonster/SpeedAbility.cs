using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackData;

public class SpeedAbility : IEliteAbility
{
    private const float SPEED_BONUS = 0.3f;
    private const float ATTACK_SPEED_BONUS = 0.2f;
    public string AbilityName => "신속";
    public string Description => "이동속도 30%, 공격속도 20% 증가";
    public Color OutlineColor => Color.cyan;

    public void ApplyAbility(ICreatureStatus creatureStatus)
    {
        creatureStatus.ModifySpeed((int)(creatureStatus.GetMonsterClass().CurrentSpeed * SPEED_BONUS));
        creatureStatus.ModifyAttackSpeed(ATTACK_SPEED_BONUS);
    }
    public void OnAttack(ICreatureStatus creatureStatus) { }
    public void OnHit(ICreatureStatus creatureStatus, int damage, AttackType attackType) { }
    public void OnUpdate(ICreatureStatus creatureStatus) { }
}
