using UnityEngine;

public class AlexanderBoss : BossMonster
{
    private IBossEssenceSystem essenceSystem;

    public AlexanderBoss(BossData data) : base(data)
    {
      
        // data�� AlexanderBossData�� ĳ����
        if (data is AlexanderBossData alexanderData)
        {
            essenceSystem = new AlexanderEssenceSystem(
                this,
                alexanderData.essenceName,
                alexanderData.initialEssence,
                alexanderData.maxEssence,
                alexanderData.essenceThreshold,
                alexanderData.playerAttackBuff,
                alexanderData.playerDamageBuff,
                alexanderData.maxEssenceStunTime
            );
        }
        InitializeAlexander();
    }

    private void InitializeAlexander()
    {
        essenceSystem.OnEssenceStateChanged += HandleEssenceStateChanged;
        essenceSystem.OnMaxEssenceStateChanged += HandleMaxEssenceStateChanged;
        essenceSystem.OnEssenceChanged += HandleEssenceChanged;
    }

    // ������ Ư�� ����(��ų�̳� ����)���� �÷��̾��� ���� ����
    public void InflictEssence(float amount)
    {
        essenceSystem.IncreaseEssence(amount);
    }

    private void HandleEssenceStateChanged()
    {
        if (essenceSystem.IsInEssenceState)
        {
            // �÷��̾� ���� 70% �̻� - ���������� ������ ����
            Debug.Log($"[Alexander] Player Essence High State");
        }
        else
        {
            Debug.Log($"[Alexander] Player Essence Normal State");
        }
    }

    private void HandleMaxEssenceStateChanged()
    {
        if (essenceSystem.IsMaxEssence)
        {
            // �÷��̾� ���� 100% - ����Ұ� ����
            Debug.Log($"[Alexander] Player Max Essence - Uncontrollable");
        }
    }

    private void HandleEssenceChanged(float newValue)
    {
        Debug.Log($"[Alexander] Player Essence Level: {newValue}");
    }

    public IBossEssenceSystem GetEssenceSystem() => essenceSystem;
}