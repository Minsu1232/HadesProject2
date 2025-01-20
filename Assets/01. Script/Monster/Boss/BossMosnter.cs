using static AttackData;
using System;
using UnityEngine;
using Unity.VisualScripting;

public class BossMonster : MonsterClass
{
    private BossData bossData;
    private int currentPhase = 0;
    private bool isInRageMode = false;
    private bool isInvulnerable = false;
    private float rageModeDuration;
    private float rageModeTimer;

    public event Action<int> OnPhaseChanged;
    public event Action OnRageModeEntered;
    public event Action OnRageModeEnded;

    public int CurrentPhase => currentPhase;
    public bool IsInRageMode => isInRageMode;
    public PhaseData CurrentPhaseData => bossData.phaseData[currentPhase];

    public BossMonster(BossData data) : base(data)
    {
        bossData = data;
        InitializeBoss();
    }
    public BossData GetBossData()
    {
        return bossData;
    }
    private void InitializeBoss()
    {
        if (bossData.invincibleOnSpawn)
        {
            isInvulnerable = true;
        }
        ApplyPhaseModifiers(currentPhase);
    }

    public override void TakeDamage(int damage)
    {
        if (isInvulnerable ||
            (CurrentPhaseData != null && CurrentPhaseData.isInvulnerableDuringTransition))
            return;

        base.TakeDamage(damage);

        CheckPhaseTransition();
        CheckRageMode();
    }

    private void CheckPhaseTransition()
    {
        if (currentPhase >= bossData.phaseData.Count - 1) return;

        float healthRatio = (float)CurrentHealth / MaxHealth;
        PhaseData nextPhase = bossData.phaseData[currentPhase + 1];

        if (healthRatio <= nextPhase.phaseTransitionThreshold)
        {
            TransitionToNextPhase();
        }
    }

    private void CheckRageMode()
    {
        if (!isInRageMode &&
            (float)CurrentHealth / MaxHealth <= bossData.rageModeThreshold)
        {
            EnterRageMode();
        }
    }

    private void TransitionToNextPhase()
    {
        currentPhase++;
        isInvulnerable = true;
        ApplyPhaseModifiers(currentPhase);
        OnPhaseChanged?.Invoke(currentPhase);
    }

    private void ApplyPhaseModifiers(int phase)
    {
        if (phase >= bossData.phaseData.Count) return;

        PhaseData phaseData = bossData.phaseData[phase];

        ModifyStats(
            attackAmount: (int)((CurrentAttackPower * phaseData.damageMultiplier) - CurrentAttackPower),
            speedAmount: (int)((CurrentSpeed * phaseData.speedMultiplier) - CurrentSpeed),
            defenseAmount: (int)((CurrentDeffense * phaseData.defenseMultiplier) - CurrentDeffense)
        );
    }

    private void EnterRageMode()
    {
        isInRageMode = true;
        rageModeDuration = bossData.rageModeThreshold;
        rageModeTimer = 0;

        ModifyStats(
            attackAmount: (int)(CurrentAttackPower * 0.5f),
            speedAmount: (int)(CurrentSpeed * 0.3f)
        );

        OnRageModeEntered?.Invoke();
    }

    private void ExitRageMode()
    {
        isInRageMode = false;

        ModifyStats(
            attackAmount: -(int)(CurrentAttackPower * 0.5f),
            speedAmount: -(int)(CurrentSpeed * 0.3f)
        );

        OnRageModeEnded?.Invoke();
    }

    public void UpdateRageMode()
    {
        if (isInRageMode)
        {
            rageModeTimer += Time.deltaTime;
            if (rageModeTimer >= rageModeDuration)
            {
                ExitRageMode();
            }
        }
    }

    public override void Die()
    {
        if (isInRageMode)
        {
            ExitRageMode();
        }
        base.Die();
    }
}