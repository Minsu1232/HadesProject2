using static AttackData;
using UnityEngine;

public class ShieldedAbility : IEliteAbility
{
    private const float ARMOR_INTERVAL = 10f;
    private const int ARMOR_AMOUNT = 10;
    private float armorTimer = 0f;
    public string AbilityName => "�Ƹ� ���";
    public string Description => "10�ʸ��� �Ƹ� 10 ȸ��";
    public Color OutlineColor => Color.yellow;

    public void ApplyAbility(ICreatureStatus creatureStatus) { }
    public void OnAttack(ICreatureStatus creatureStatus) { }
    public void OnHit(ICreatureStatus creatureStatus, int damage, AttackType attackType) { }
    public void OnUpdate(ICreatureStatus creatureStatus)
    {
        armorTimer += Time.deltaTime;
        if (armorTimer >= ARMOR_INTERVAL)
        {
            armorTimer = 0f;
            int maxArmor = creatureStatus.GetMonsterClass().GetMonsterData().armorValue;
            int currentArmor = creatureStatus.GetMonsterClass().CurrentArmor;
            int actualHealAmount = Mathf.Min(ARMOR_AMOUNT, maxArmor - currentArmor);
            if (actualHealAmount > 0)
            {
                creatureStatus.ModifyArmor(actualHealAmount);
            }
        }
    }
}
