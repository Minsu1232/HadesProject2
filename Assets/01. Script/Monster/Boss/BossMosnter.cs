using static AttackData;
using System;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public class BossMonster : MonsterClass
{
    // �б� ���� BossData ����
    private readonly BossData bossData;




    // �⺻ ���� ������Ƽ
    public bool CanBeInterrupted { get; private set; }
    public float PhaseTransitionDuration { get; private set; }
    public bool InvincibleOnSpawn { get; private set; }
    public float AggroRange { get; private set; }

    // ������ ���� ����
    private int _currentPhase = 1;  // �ʱⰪ 1�� ����

    public int CurrentPhase
    {
        get => _currentPhase;
        private set
        {
            if (_currentPhase == value) return;  // ������ ���̸� �������� ����
            if (value > bossData.phaseCount) return; // bossData.phaseCount���� ũ�� �������� ����

            Debug.Log($"[CurrentPhase] Phase changed! Old: {_currentPhase}, New: {value}");
            Debug.Log($"[CurrentPhase] Call Stack:\n{System.Environment.StackTrace}");

            _currentPhase = value;
        }
    }

    private List<PhaseData> runtimePhaseData;  // ���� �� ����� �� �ִ� ������ ������
    public PhaseData CurrentPhaseData { get; set; }

    private List<GimmickData> runtimeGimmickData;
    public GimmickData CurrentPhaseGimmickData { get; set; }
    public bool IsInPhaseTransition { get; private set; }

    // ������ ��� ����
    public bool IsInRageMode { get; private set; }
    public float RageModeThreshold { get; private set; }
    public float RageModeDuration { get; private set; }
    private float rageModeTimer;

    // �̴Ͼ� ���� ����
    public bool CanSummonMinions { get; private set; }
    public int CurrentMinionCount { get; private set; }
    public int MaxMinionCount { get; private set; }
    public float CurrentSummonCooldown { get; private set; }

    // ���� ���� ����
    public bool IsInvulnerable { get; private set; }
    public float GlobalStunResistance { get; private set; }
    public float KnockbackResistance { get; private set; }

    // ��ų ���� ����
    public float GlobalSkillCooldownModifier { get; private set; }
    public float SkillPreDelayDefault { get; private set; }
    public float SkillPostDelayDefault { get; private set; }

    
    // ���� ���� ����
    private Dictionary<AttackPatternData, int> patternSuccessCounts;
    private HashSet<AttackPatternData> disabledPatterns;
    private Dictionary<AttackPatternData, float> patternDifficulties = new Dictionary<AttackPatternData, float>();
    // �̺�Ʈ
    public event Action<int> OnPhaseChanged;
    public event Action OnRageModeEntered;
    public event Action OnRageModeEnded;

    int a;

    public bool IsInGimmick = false;
    public BossMonster(BossData data) : base(data)
    {
        bossData = data;
        InitializeRuntimeState();
        InitializeBoss();      

         
    }

    private void InitializeRuntimeState()
    {
        
        CurrentPhase = 1;

        SetArmorValue(bossData.armorValue);
        // �⺻ ���� �ʱ�ȭ
        CanBeInterrupted = bossData.canBeInterrupted;
        PhaseTransitionDuration = bossData.phaseTransitionDuration;
        InvincibleOnSpawn = bossData.invincibleOnSpawn;
        AggroRange = bossData.aggroRange;

        // ������ ���� �ʱ�ȭ
       
        IsInPhaseTransition = false;
        InitializePhaseData();
        InitializeGimmickData();
        CurrentPhaseData = runtimePhaseData[CurrentPhase];
        CurrentPhaseGimmickData = runtimeGimmickData[CurrentPhase];

        // ������ ��� �ʱ�ȭ
        IsInRageMode = false;
        
        RageModeDuration = bossData.rageModeDuration;
        rageModeTimer = 0;

        // �̴Ͼ� ���� �ʱ�ȭ
        CanSummonMinions = bossData.canSummonMinions;
        CurrentMinionCount = 0;
        MaxMinionCount = bossData.maxMinionCount;
        CurrentSummonCooldown = 0;

        // ���� ���� �ʱ�ȭ
        IsInvulnerable = bossData.invincibleOnSpawn;
        GlobalStunResistance = bossData.globalStunResistance;
        KnockbackResistance = bossData.knockbackResistance;

        // ��ų ���� �ʱ�ȭ
        GlobalSkillCooldownModifier = bossData.globalSkillCooldownModifier;
        SkillPreDelayDefault = bossData.skillPreDelayDefault;
        SkillPostDelayDefault = bossData.skillPostDelayDefault;

        // ���� ���� �ʱ�ȭ
        patternSuccessCounts = new Dictionary<AttackPatternData, int>();
        disabledPatterns = new HashSet<AttackPatternData>();
        InitializePatternStates();
    }

    private void InitializePhaseData()
    {
        runtimePhaseData = new List<PhaseData>();
        foreach (var phaseData in bossData.phaseData)
        {           
            var newPhaseData = new PhaseData
            {                
                phaseName = phaseData.phaseName,
                isInvulnerable = phaseData.isInvulnerable,
                phaseTransitionThreshold = phaseData.phaseTransitionThreshold,
                transitionDuration = phaseData.transitionDuration,
                isInvulnerableDuringTransition = phaseData.isInvulnerableDuringTransition,
                patternChangeTime = phaseData.patternChangeTime,
                availablePatterns = new List<AttackPatternData>(phaseData.availablePatterns),
                moveType = phaseData.moveType,
                attackType = phaseData.attackType,
                skillType = phaseData.skillType,
                damageMultiplier = phaseData.damageMultiplier,
                speedMultiplier = phaseData.speedMultiplier,
                defenseMultiplier = phaseData.defenseMultiplier,
                attackSpeedMultiplier = phaseData.attackSpeedMultiplier,
                canBeInterrupted = phaseData.canBeInterrupted,
                stunResistance = phaseData.stunResistance,
                useHealthRetreat = phaseData.useHealthRetreat,
                healthRetreatThreshold = phaseData.healthRetreatThreshold,
                retreatDuration = phaseData.retreatDuration,
                phaseAttackStrategies = new List<AttackStrategyWeight>(phaseData.phaseAttackStrategies) // �� �κ� �߰�
               
            };
           
            runtimePhaseData.Add(newPhaseData);
            Debug.Log(runtimePhaseData[a].phaseName);
            a++;
        }
        
        
    }
    private void InitializeGimmickData()
    {     
        runtimeGimmickData = new List<GimmickData>();

        foreach (var phase in bossData.phaseData)
        {
            foreach (var gimmick in phase.gimmicks)
            {
                var newGimmick = new GimmickData
                {
                    gimmickName = gimmick.gimmickName,
                    type = gimmick.type,
                    triggerHealthThreshold = gimmick.triggerHealthThreshold,
                    duration = gimmick.duration,
                    isEnabled = gimmick.isEnabled,
                    successCount = gimmick.successCount,
                    moveSpeed = gimmick.moveSpeed,
                    hazardSpawnType = gimmick.hazardSpawnType,
                    targetType = gimmick.targetType,
                    requirePlayerAction = gimmick.requirePlayerAction,
                    isInterruptible = gimmick.isInterruptible,
                    destroyAfterUse = gimmick.destroyAfterUse,
                    makeInvulnerable = gimmick.makeInvulnerable,
                    damageMultiplier = gimmick.damageMultiplier,
                    failDamage = gimmick.failDamage,
                    damage = gimmick.damage,
                    affectStatusEffects = gimmick.affectStatusEffects,
                    useCustomPosition = gimmick.useCustomPosition,
                    gimmickPosition = gimmick.gimmickPosition,
                    areaRadius = gimmick.areaRadius,
                    followTarget = gimmick.followTarget,
                    collisionMask = gimmick.collisionMask,
                    preparationTime = gimmick.preparationTime,
                    repeatInterval = gimmick.repeatInterval,
                    warningEffect = gimmick.warningEffect,
                    activeEffect = gimmick.activeEffect,
                    hazardPrefab = gimmick.hazardPrefab,
                    warningSound = gimmick.warningSound,
                    activeSound = gimmick.activeSound
                };

                runtimeGimmickData.Add(newGimmick);
                Debug.Log("hahareh" + newGimmick.gimmickName);
            }
        }

       
    }
        private void InitializePatternStates()
    {
        foreach (var phaseData in runtimePhaseData)
        {
            foreach (var pattern in phaseData.availablePatterns)
            {
                patternSuccessCounts[pattern] = 0;
            }
        }
    }

    public BossData GetBossData()
    {
        return bossData;
    }
    private void InitializeBoss()
    {
       
    }

    public override void TakeDamage(int damage)
    {
        Debug.Log($"[Boss] IsInvulnerable: {IsInvulnerable}"); // ù ��° ����
        Debug.Log($"[Boss] CurrentPhaseData: {CurrentPhaseData != null}"); // �� ��° ������ ù �κ�

        if (CurrentPhaseData != null)
        {
            Debug.Log($"[Boss] isInvulnerableDuringTransition: {CurrentPhaseData.isInvulnerableDuringTransition} ���� ������ {CurrentPhase}"); // �� ��° ������ �� ��° �κ�
            
        }

        if (IsInvulnerable ||
            (CurrentPhaseData != null && CurrentPhaseData.isInvulnerableDuringTransition))
        {
            Debug.Log("Invulner");
            return;
        }

        // ���⼭ base.TakeDamage(damage)�� ȣ���ϱ� ���� ���� ü���� ����غ�����
        Debug.Log($"������ ���� �� ü��: {CurrentHealth}");
    
    base.TakeDamage(damage);
    
    // ������ ���� �� ü�µ� ���
    Debug.Log($"������ ���� �� ü��: {CurrentHealth}");
    
    
    }

 
    public override void Die()
    {
     
        base.Die();
    }

    public List<PhaseData> GetruntimePhaseData()
    {
        return runtimePhaseData;
    }

    #region Set�ż���
    public void SetInvulnerable(bool value, string reason = "")
    {
        IsInvulnerable = value;
        if (Debug.isDebugBuild)
        {
            Debug.Log($"Invulnerable state changed to {value}. Reason: {reason}");
        }
    }
    public void SetInPhaseTransition(bool value, string reason = "")
    {
        IsInPhaseTransition = value;
        if (Debug.isDebugBuild)
        {
            Debug.Log($"IsInPhaseTransition state changed to {value}. Reason: {reason}");
        }
    }
    public void SetCurrentPhase(int phase)
    {
        if (phase <= 0 || phase > bossData.phaseData.Count)
        {
            Debug.LogError($"Invalid phase value: {phase}");
            return;
        }

        Debug.Log($"[Phase Change] SetCurrentPhase called! New Phase: {phase}");

        // ���� Ʈ���̽� ��� (��� ȣ���ߴ��� Ȯ�� ����)
        Debug.Log($"[Phase Change] Call Stack:\n{System.Environment.StackTrace}");

        CurrentPhase = phase;
        CurrentPhaseData = runtimePhaseData[phase];
        
        OnPhaseChanged?.Invoke(phase);
    }
    public void IncreasePhase()
    {
        Debug.Log($"[Phase Change] IncreasePhase called! Current Phase: {CurrentPhase}, New Phase: {CurrentPhase + 1}");

        //  ���� Ʈ���̽� ���
        Debug.Log($"[Phase Change] IncreasePhase Call Stack:\n{System.Environment.StackTrace}");

        SetCurrentPhase(CurrentPhase + 1);
    }
    #endregion

    public void ReleaseGimmickPrefabs()
    {
        foreach (var phase in runtimePhaseData)
        {
            foreach (var gimmick in phase.gimmicks)
            {
                if (gimmick.hazardPrefab != null)
                {
                    Addressables.Release(gimmick.hazardPrefab);
                    gimmick.hazardPrefab = null;  // ���� ����
                }
            }
        }
        Debug.Log("[BossMonster] ��� ��� ������ ������ �Ϸ�.");
    }
    public override DamageType GetDamageType()
    {
        return DamageType.Boss;
    }
}