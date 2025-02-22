using System;
using UnityEngine;

public class AlexanderEssenceSystem : IBossEssenceSystem
{
    private float currentEssence;
    private string esseceName;
    private float maxEssence;
    private bool isInEssenceState;
    private bool isMaxEssence;
    private readonly BossMonster boss;

    private readonly float essenceThreshold;
    private readonly float playerAttackBuff;
    private readonly float playerDamageBuff;
    private readonly float stunDuration;

    public float CurrentEssence => currentEssence;
    public float MaxEssence => maxEssence;
    public bool IsInEssenceState => isInEssenceState;
    public bool IsMaxEssence => isMaxEssence;

    public string BossEssenceName => esseceName;

    public event System.Action<float> OnEssenceChanged;
    public event System.Action OnEssenceStateChanged;
    public event System.Action OnMaxEssenceStateChanged;

    public AlexanderEssenceSystem(
     BossMonster boss,
     string essenceName,
     float initialEssence,
     float maxEssence,
     float essenceThreshold,
     float playerAttackBuff,
     float playerDamageBuff,
     float stunDuration)
    {
        this.boss = boss;
        this.esseceName = essenceName;
        this.currentEssence = initialEssence;
        this.maxEssence = maxEssence;
        this.essenceThreshold = essenceThreshold;
        this.playerAttackBuff = playerAttackBuff;
        this.playerDamageBuff = playerDamageBuff;
        this.stunDuration = stunDuration;
    }

    public void IncreaseEssence(float amount)
    {
        float previousEssence = currentEssence;
        currentEssence = Mathf.Min(currentEssence + amount, maxEssence);

        CheckEssenceState(previousEssence);
        OnEssenceChanged?.Invoke(currentEssence);
    }

    public void DecreaseEssence(float amount)
    {
        float previousEssence = currentEssence;
        currentEssence = Mathf.Max(currentEssence - amount, 0f);

        CheckEssenceState(previousEssence);
        OnEssenceChanged?.Invoke(currentEssence);
    }

    public void UpdateEssence()
    {
        // �ʿ��� ��� �����Ӻ� ������Ʈ ����
    }

    private void CheckEssenceState(float previousEssence)
    {
        // 70% �Ӱ��� üũ
        bool wasInEssenceState = previousEssence >= essenceThreshold && previousEssence < maxEssence;
        bool isNowInEssenceState = currentEssence >= essenceThreshold && currentEssence < maxEssence;

        if (wasInEssenceState != isNowInEssenceState)
        {
            isInEssenceState = isNowInEssenceState;
            OnEssenceStateChanged?.Invoke();
            ApplyEssenceEffects(isNowInEssenceState);
        }

        // 100% ���� üũ
        bool wasMaxEssence = previousEssence >= maxEssence;
        bool isNowMaxEssence = currentEssence >= maxEssence;

        if (wasMaxEssence != isNowMaxEssence)
        {
            isMaxEssence = isNowMaxEssence;
            OnMaxEssenceStateChanged?.Invoke();
            if (isNowMaxEssence)
            {
                ApplyMaxEssenceEffects();
            }
        }
    }

    private void ApplyEssenceEffects(bool activate)
    {
        PlayerClass player = GameInitializer.Instance.GetPlayerClass();
        if (activate)
        {
            // �÷��̾� ����/����� ����
            player.ModifyPower(
                attackAmount: (int)(player.PlayerStats.AttackPower * playerAttackBuff)
            );
            player.GetStats().DamageReceiveRate += playerDamageBuff;
        }
        else
        {
            // ����/����� ����
            player.ModifyPower(
                attackAmount: -(int)(player.PlayerStats.AttackPower * playerAttackBuff)
            );
            player.GetStats().DamageReceiveRate -= playerDamageBuff;
        }
    }

    private void ApplyMaxEssenceEffects()
    {
        PlayerClass player = GameInitializer.Instance.GetPlayerClass();

        // 2�� ����
        player.ApplyStun(stunDuration);

        // ������ �ʱ�ȭ
        currentEssence = 0f;
        OnEssenceChanged?.Invoke(currentEssence);
    }
}