using static AttackData;
using UnityEngine;

public class ShieldedAbility : IEliteAbility
{
    private const float ARMOR_INTERVAL = 10f;  // 15�ʸ��� 
    private const int ARMOR_AMOUNT = 10;        // 2�� �Ƹ� ȸ��
    private float armorTimer = 0f;

    public string AbilityName => "�Ƹ� ���";
    public string Description => "10�ʸ��� �Ƹ� 10 ȸ��";
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

            // �ִ� �ƸӸ� �ʰ����� �ʵ��� ȸ���� ����
            int actualHealAmount = Mathf.Min(ARMOR_AMOUNT, maxArmor - currentArmor);

            if (actualHealAmount > 0)
            {
                monsterStatus.ModifyArmor(actualHealAmount);
            }
        }
    }
}
