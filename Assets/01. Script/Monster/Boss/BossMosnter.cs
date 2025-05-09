using static AttackData;
using System;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public class BossMonster : MonsterClass
{
    // 읽기 전용 BossData 참조
    private readonly BossData bossData;




    // 기본 상태 프로퍼티
    public bool CanBeInterrupted { get; private set; }
    public float PhaseTransitionDuration { get; private set; }
    public bool InvincibleOnSpawn { get; private set; }
    public float AggroRange { get; private set; }

    // 페이즈 관련 상태
    private int _currentPhase = 1;  // 초기값 1로 설정

    public int CurrentPhase
    {
        get => _currentPhase;
        private set
        {
            if (_currentPhase == value) return;  // 동일한 값이면 변경하지 않음
            if (value > bossData.phaseCount) return; // bossData.phaseCount보다 크면 변경하지 않음

            Debug.Log($"[CurrentPhase] Phase changed! Old: {_currentPhase}, New: {value}");
            Debug.Log($"[CurrentPhase] Call Stack:\n{System.Environment.StackTrace}");

            _currentPhase = value;
        }
    }

    private List<PhaseData> runtimePhaseData;  // 실행 중 변경될 수 있는 페이즈 데이터
    public PhaseData CurrentPhaseData { get; set; }

    private List<GimmickData> runtimeGimmickData;
    public GimmickData CurrentPhaseGimmickData { get; set; }
    public bool IsInPhaseTransition { get; private set; }

    // 레이지 모드 상태
    public bool IsInRageMode { get; private set; }
    public float RageModeThreshold { get; private set; }
    public float RageModeDuration { get; private set; }
    private float rageModeTimer;

    // 미니언 관련 상태
    public bool CanSummonMinions { get; private set; }
    public int CurrentMinionCount { get; private set; }
    public int MaxMinionCount { get; private set; }
    public float CurrentSummonCooldown { get; private set; }

    // 전투 관련 상태
    public bool IsInvulnerable { get; private set; }
    public float GlobalStunResistance { get; private set; }
    public float KnockbackResistance { get; private set; }

    // 스킬 관련 상태
    public float GlobalSkillCooldownModifier { get; private set; }
    public float SkillPreDelayDefault { get; private set; }
    public float SkillPostDelayDefault { get; private set; }

    
    // 패턴 관련 상태
    private Dictionary<AttackPatternData, int> patternSuccessCounts;
    private HashSet<AttackPatternData> disabledPatterns;
    private Dictionary<AttackPatternData, float> patternDifficulties = new Dictionary<AttackPatternData, float>();
    // 이벤트
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
        // 기본 설정 초기화
        CanBeInterrupted = bossData.canBeInterrupted;
        PhaseTransitionDuration = bossData.phaseTransitionDuration;
        InvincibleOnSpawn = bossData.invincibleOnSpawn;
        AggroRange = bossData.aggroRange;

        // 페이즈 상태 초기화
        Debug.Log($"phaseData count: {bossData.phaseData.Count}, Current Phase: {CurrentPhase}");
        IsInPhaseTransition = false;
        InitializePhaseData();
        InitializeGimmickData();
        int phaseIndex = CurrentPhase - 1;
        if (phaseIndex >= 0 && phaseIndex < runtimePhaseData.Count)
        {
            CurrentPhaseData = runtimePhaseData[phaseIndex];
        }
        else
        {
            Debug.LogError($"유효하지 않은 페이즈 인덱스: {phaseIndex}, 데이터 수: {runtimePhaseData.Count}");
            CurrentPhaseData = runtimePhaseData.Count > 0 ? runtimePhaseData[0] : null;
        }

        // 기믹 데이터도 동일하게 처리
        if (phaseIndex >= 0 && phaseIndex < runtimeGimmickData.Count)
        {
            CurrentPhaseGimmickData = runtimeGimmickData[phaseIndex];
        }
        else
        {
            Debug.LogError($"유효하지 않은 기믹 인덱스: {phaseIndex}, 데이터 수: {runtimeGimmickData.Count}");
            CurrentPhaseGimmickData = runtimeGimmickData.Count > 0 ? runtimeGimmickData[0] : null;
        }

        // 레이지 모드 초기화
        IsInRageMode = false;
        
        RageModeDuration = bossData.rageModeDuration;
        rageModeTimer = 0;

        // 미니언 관련 초기화
        CanSummonMinions = bossData.canSummonMinions;
        CurrentMinionCount = 0;
        MaxMinionCount = bossData.maxMinionCount;
        CurrentSummonCooldown = 0;

        // 전투 관련 초기화
        IsInvulnerable = bossData.invincibleOnSpawn;
        GlobalStunResistance = bossData.globalStunResistance;
        KnockbackResistance = bossData.knockbackResistance;

        // 스킬 관련 초기화
        GlobalSkillCooldownModifier = bossData.globalSkillCooldownModifier;
        SkillPreDelayDefault = bossData.skillPreDelayDefault;
        SkillPostDelayDefault = bossData.skillPostDelayDefault;

        // 패턴 상태 초기화
        patternSuccessCounts = new Dictionary<AttackPatternData, int>();
        disabledPatterns = new HashSet<AttackPatternData>();
        InitializePatternStates();

       
    }

    private void InitializePhaseData()
    {
        if (bossData.phaseData.Count == 0)
        {
            Debug.LogError("페이즈 데이터가 없습니다.");
            return;
        }

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
                phaseAttackStrategies = new List<AttackStrategyWeight>(phaseData.phaseAttackStrategies), // 이 부분 추가
                skillConfigIds = new List<int>(phaseData.skillConfigIds),
                skillConfigWeights = new List<float>(phaseData.skillConfigWeights) // 가중치도 추가!



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
        Debug.Log($"[Boss] IsInvulnerable: {IsInvulnerable}"); // 첫 번째 조건
        Debug.Log($"[Boss] CurrentPhaseData: {CurrentPhaseData != null}"); // 두 번째 조건의 첫 부분

        if (CurrentPhaseData != null)
        {
            Debug.Log($"[Boss] isInvulnerableDuringTransition: {CurrentPhaseData.isInvulnerableDuringTransition} 현재 페이즈 {CurrentPhase}"); // 두 번째 조건의 두 번째 부분
            
        }

        if (IsInvulnerable ||
            (CurrentPhaseData != null && CurrentPhaseData.isInvulnerableDuringTransition))
        {
            Debug.Log("Invulner");
            return;
        }

        // 여기서 base.TakeDamage(damage)를 호출하기 전에 현재 체력을 출력해보세요
        Debug.Log($"데미지 적용 전 체력: {CurrentHealth}");
    
    base.TakeDamage(damage);
    
    // 데미지 적용 후 체력도 출력
    Debug.Log($"데미지 적용 후 체력: {CurrentHealth}");
    
    
    }

 
    public override void Die()
    {
     
        base.Die();
    }

    public List<PhaseData> GetruntimePhaseData()
    {
        return runtimePhaseData;
    }

    #region Set매서드
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

        // 스택 트레이스 출력 (어디서 호출했는지 확인 가능)
        Debug.Log($"[Phase Change] Call Stack:\n{System.Environment.StackTrace}");

        CurrentPhase = phase;
        CurrentPhaseData = runtimePhaseData[phase-1];
        
        OnPhaseChanged?.Invoke(phase);
    }
    public void IncreasePhase()
    {
        Debug.Log($"[Phase Change] IncreasePhase called! Current Phase: {CurrentPhase}, New Phase: {CurrentPhase + 1}");

        //  스택 트레이스 출력
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
                    gimmick.hazardPrefab = null;  // 참조 해제
                }
            }
        }
        Debug.Log("[BossMonster] 모든 기믹 프리팹 릴리즈 완료.");
    }
    public override DamageType GetDamageType()
    {
        return DamageType.Boss;
    }
}