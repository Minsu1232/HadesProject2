using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackData;

public class VampireAbility : IEliteAbility
{
    private const float LIFESTEAL_PERCENT = 0.2f;

    public string AbilityName => "ÈíÇ÷";
    public string Description => "°ø°Ý·ÂÀÇ 20%¸¸Å­ Ã¼·Â Èí¼ö";
    public Color OutlineColor => Color.black;

    public void ApplyAbility(MonsterStatus monsterStatus) { }

    public void OnAttack(MonsterStatus monsterStatus)
    {
        int healAmount = (int)(monsterStatus.CurrentAttackPower * LIFESTEAL_PERCENT);
        monsterStatus.ModifyHealth(healAmount);
    }

    public void OnHit(MonsterStatus monsterStatus, int damage, AttackType attackType) { }
    public void OnUpdate(MonsterStatus monsterStatus) { }
}
