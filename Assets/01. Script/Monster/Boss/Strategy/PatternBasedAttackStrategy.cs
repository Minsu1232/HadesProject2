using DG.Tweening;
using static IMonsterState;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PatternBasedAttackStrategy : BasePhysicalAttackStrategy
{
    private BossData bossData;
    private BossStatus bossStatus;
    private PlayerClass playerClass;
    private CreatureAI owner;
    private BossPatternManager patternManager;

    private AttackPatternData currentPattern;
    private bool isExecutingPattern = false;
    private Sequence currentPatternSequence;
    private Transform currentTransform;
    private Transform currentTarget;
    private BossMonster currentMonster;
    private MiniGameManager miniGameManager;
    private BossUIManager bossUIManager;
    private float lastAttackTime = 0f;

    public override bool IsAttacking => isExecutingPattern;
    public override PhysicalAttackType AttackType => PhysicalAttackType.Basic;

    public void Initialize(BossData data, MiniGameManager mgr, CreatureAI ownerAI, BossMonster bossMonster)
    {
        currentMonster = bossMonster;
        bossData = data;
        miniGameManager = mgr;
        owner = ownerAI;

        bossStatus = owner.GetComponent<BossStatus>();
        if (bossStatus != null)
        {
            patternManager = new BossPatternManager(bossStatus);
        }

        playerClass = GameInitializer.Instance.GetPlayerClass();
        miniGameManager.OnMiniGameComplete += HandleMiniGameComplete;
        ValidateInitialization();

        currentPattern = SelectPattern(currentMonster);
    }

    private void ValidateInitialization()
    {
        if (miniGameManager == null)
        {
            Debug.LogError("MiniGameManager is null in Initialize!");
            return;
        }

        Debug.Log("PatternBasedAttackStrategy Initialized");

        if (bossData.phaseData != null && bossData.phaseData.Count > 0)
        {
            Debug.Log($"Phase Count: {bossData.phaseData.Count}");
            Debug.Log($"Phase 1 Pattern Count: {bossData.phaseData[0].availablePatterns.Count}");
            foreach (var pattern in bossData.phaseData[currentMonster.CurrentPhase].availablePatterns)
            {
                Debug.Log($"Pattern: {pattern.patternName}, Steps: {pattern.steps.Count}");
            }
        }
    }

    private List<AttackPatternData> GetCurrentPhasePatterns()
    {
        int currentPhase = currentMonster.CurrentPhase;
        Debug.Log(currentPhase);
        if (currentPhase < bossData.phaseData.Count)
        {
            Debug.Log("들어왔네" + $"{bossData.phaseData[currentPhase].availablePatterns}");
            return bossData.phaseData[currentPhase].availablePatterns;
        }
        Debug.Log("못들어왔네");
        return null;
    }

    public override void Attack(Transform transform, Transform target, IMonsterClass monsterData)
    {
        currentTransform = transform;
        currentTarget = target;

        if (bossUIManager == null)
        {
            bossUIManager = currentTransform.GetComponent<BossStatus>()?.GetBossUIManager();
        }

        if (!isExecutingPattern && CanStartNewPattern())
        {
            Debug.Log("[Attack] Starting new pattern attack.");
            StartNewPattern();
        }
    }

    private void HandleMiniGameComplete(MiniGameType type, MiniGameResult result)
    {
        if (currentMonster == null || patternManager == null) return;

        bool enterGroggy = patternManager.HandleMiniGameSuccess(result, currentPattern);
        miniGameManager.HandleDodgeReward(result);

        if (enterGroggy)
        {
            HandleGroggyState();
            return;
        }

        if (currentPatternSequence != null && currentPatternSequence.IsActive())
        {
            currentPatternSequence.Play();
        }
    }

    private void HandleGroggyState()
    {
        Debug.Log("Pattern completed! Entering groggy state...");
        StopAttack();
        isExecutingPattern = false;

        if (currentPatternSequence != null)
        {
            currentPatternSequence.Kill();
        }

        owner.ChangeState(MonsterStateType.Groggy);
    }

    private AttackPatternData SelectPattern(IMonsterClass monsterData)
    {
        var patterns = GetCurrentPhasePatterns();
        if (patterns == null || patterns.Count == 0)
        {
            Debug.Log("여기다");
            return null;
        }

        float healthRatio = (float)monsterData.CurrentHealth / monsterData.MaxHealth;
        var availablePatterns = GetAvailablePatternsForHealth(healthRatio, patterns);

        if (availablePatterns.Count == 0)
            return null;

        return SelectWeightedPattern(availablePatterns);
    }

    private List<AttackPatternData> GetAvailablePatternsForHealth(float healthRatio, List<AttackPatternData> patterns)
    {
        return patterns.FindAll(p =>
            healthRatio >= p.healthThresholdMin && healthRatio <= p.healthThresholdMax);
    }

    private AttackPatternData SelectWeightedPattern(List<AttackPatternData> patterns)
    {
        float totalWeight = patterns.Sum(p => p.patternWeight);
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0;

        foreach (var pattern in patterns)
        {
            currentWeight += pattern.patternWeight;
            if (randomValue <= currentWeight)
            {
                return pattern;
            }
        }

        return patterns[0];
    }

    private void StartNewPattern()
    {
        if (currentPatternSequence != null && currentPatternSequence.IsPlaying())
        {
            currentPatternSequence.Kill();
        }

        currentPattern = SelectPattern(currentMonster);
        Debug.Log($"Selected Pattern: {currentPattern?.patternName}");

        if (currentPattern != null)
        {
            ExecutePattern();
        }
    }

    private void ExecutePattern()
    {
        isExecutingPattern = true;
        currentPatternSequence = DOTween.Sequence();
        PatternSequenceManager.RegisterSequence(currentPatternSequence);
        currentPatternSequence.AppendInterval(0.5f);

        foreach (var step in currentPattern.steps)
        {
            AddStepToSequence(step);
        }

        AddPatternEndEffect();
        SetupPatternCompletion();
        currentPatternSequence.Play();
    }

    private void AddStepToSequence(AttackStepData step)
    {
        if (step.isTransitionAnim)
        {
            currentPatternSequence.AppendCallback(() =>
            {
                Debug.Log($"[AddStepToSequence] Playing transition animation for attack type: {step.attackType}");
                PlayStepAnimation(step.attackType);
            });
            Debug.Log($"[AddStepToSequence] Step Start - Type: {step.attackType}");
        }

        currentPatternSequence.AppendCallback(() =>
        {
            Debug.Log($"[AddStepToSequence] Executing attack step: {step.attackType}");
            ExecuteStep(step);
        });

        if (step.hasMiniGame)
        {
            AddMiniGameToSequence(step);
        }

        currentPatternSequence.AppendInterval(step.stepDelay);
    }

    private void AddMiniGameToSequence(AttackStepData step)
    {
        Debug.Log($"Setting up minigame for: {step.attackType}");
        currentPatternSequence.AppendCallback(() =>
        {
            if (miniGameManager != null && patternManager != null)
            {
                Debug.Log($"[AddMiniGameToSequence] Starting minigame for: {step.attackType}");
                float currentDifficulty = patternManager.GetPatternDifficulty(currentPattern);
                miniGameManager.StartMiniGame(step.miniGameType, currentDifficulty);
            }
            else
            {
                Debug.LogError("MiniGameManager or PatternManager is null!");
            }
        });

        if (step.waitForMiniGame)
        {
            currentPatternSequence.AppendCallback(() =>
            {
                Debug.Log("Pattern Paused for MiniGame");
            });
        }
    }

    private void AddPatternEndEffect()
    {
        if (currentPattern.patternEndEffect != null)
        {
            currentPatternSequence.AppendCallback(() =>
                GameObject.Instantiate(currentPattern.patternEndEffect,
                    currentTransform.position,
                    Quaternion.identity));
        }
    }

    private void SetupPatternCompletion()
    {
        currentPatternSequence.OnComplete(() =>
        {
            isExecutingPattern = false;
            lastAttackTime = Time.time;
        });
    }

    private void ExecuteStep(AttackStepData step)
    {
        Debug.Log($"Creating strategy for type: {step.attackType}");
        IAttackStrategy baseAttack = StrategyFactory.CreateAttackStrategy(step.attackType, bossData);
        Debug.Log($"Created strategy type: {baseAttack?.GetType()}");
        baseAttack?.Attack(currentTransform, currentTarget, currentMonster);
    }

    public override bool CanAttack(float distanceToTarget, IMonsterClass monsterData)
    {
        if (distanceToTarget > monsterData.CurrentAttackRange * 1.5f)
        {
            StopCurrentPattern();
            return false;
        }

        if (isExecutingPattern)
            return true;

        return CanStartNewPattern() && distanceToTarget <= monsterData.CurrentAttackRange;
    }

    private bool CanStartNewPattern()
    {
        if (currentPattern == null)
            return true;

        return Time.time >= lastAttackTime + currentPattern.patternCooldown;
    }
    
    public void OnPhaseChanged(int newPhase)
    {
        StopCurrentPattern();  
       
       
       
    }

    private void StopCurrentPattern()
    {
        if (currentPatternSequence != null)
        {
            if (currentPatternSequence.IsActive())
            {
                Debug.Log("Killing current pattern sequence.");
                currentPatternSequence.Kill();
            }
            currentPatternSequence = null;
        }
        isExecutingPattern = false;
    }

    public override void StopAttack()
    {
        StopCurrentPattern();
        base.StopAttack();
    }

    private void OnDestroy()
    {
        if (currentPatternSequence != null && currentPatternSequence.IsPlaying())
        {
            currentPatternSequence.Kill();
        }
        if (miniGameManager != null)
        {
            miniGameManager.OnMiniGameComplete -= HandleMiniGameComplete;
        }
    }

    private void PlayStepAnimation(AttackStrategyType attackType)
    {
        IAttackStrategy baseAttack = StrategyFactory.CreateAttackStrategy(attackType, bossData);
        string triggerName = baseAttack?.GetAnimationTriggerName() ?? "DefaultAttack";
        Debug.Log($"[PlayStepAnimation] Triggering animation '{triggerName}' for attack type: {attackType}");
        if (owner.animator != null)
        {
            owner.animator.SetTrigger(triggerName);
        }
        else
        {
            Debug.LogWarning("[PlayStepAnimation] Owner animator is null!");
        }
    }
}