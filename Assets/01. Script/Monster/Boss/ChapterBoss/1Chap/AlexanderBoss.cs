using UnityEngine;

public class AlexanderBoss : BossMonster
{
    private IBossEssenceSystem essenceSystem;

    public AlexanderBoss(BossData data) : base(data)
    {
      
        // data를 AlexanderBossData로 캐스팅
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

    // 보스의 특정 공격(스킬이나 패턴)에서 플레이어의 광기 증가
    public void InflictEssence(float amount)
    {
        essenceSystem.IncreaseEssence(amount);
    }

    private void HandleEssenceStateChanged()
    {
        if (essenceSystem.IsInEssenceState)
        {
            // 플레이어 광기 70% 이상 - 강해지지만 위험한 상태
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
            // 플레이어 광기 100% - 제어불가 상태
            Debug.Log($"[Alexander] Player Max Essence - Uncontrollable");
        }
    }

    private void HandleEssenceChanged(float newValue)
    {
        Debug.Log($"[Alexander] Player Essence Level: {newValue}");
    }

    public IBossEssenceSystem GetEssenceSystem() => essenceSystem;
}