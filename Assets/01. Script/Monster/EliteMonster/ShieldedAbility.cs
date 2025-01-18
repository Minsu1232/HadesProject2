using static AttackData;
using UnityEngine;

public class ShieldedAbility : IEliteAbility
{
    private const float ARMOR_INTERVAL = 10f;  // 15초마다 
    private const int ARMOR_AMOUNT = 10;        // 2의 아머 회복
    private float armorTimer = 0f;

    public string AbilityName => "아머 재생";
    public string Description => "10초마다 아머 10 회복";
    public Color OutlineColor => Color.yellow;

    public void ApplyAbility(MonsterStatus monsterStatus) { }
    public void OnAttack(MonsterStatus monsterStatus) { }
    public void OnHit(MonsterStatus monsterStatus, int damage, AttackType attackType) { }

    public void OnUpdate(MonsterStatus monsterStatus)
    {
        armorTimer += Time.deltaTime;
        if (armorTimer >= ARMOR_INTERVAL)
        {
            armorTimer = 0f;

            MonsterClass monsterClass = monsterStatus.GetMonsterClass();
            int maxArmor = monsterClass.GetMonsterData().armorValue;
            int currentArmor = monsterClass.CurrentArmor;

            // 최대 아머를 초과하지 않도록 회복량 조절
            int actualHealAmount = Mathf.Min(ARMOR_AMOUNT, maxArmor - currentArmor);

            if (actualHealAmount > 0)
            {
                monsterStatus.ModifyArmor(actualHealAmount);
            }
        }
    }
}
