using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class PatternBasedAttackStrategy : BasePhysicalAttackStrategy
{
    private BossData bossData;
    private List<AttackPatternData> currentPhasePatterns;
    private AttackPatternData currentPattern;
    private int currentStepIndex = 0;
    private bool isExecutingPattern = false;
    private DG.Tweening.Sequence currentPatternSequence;
    private int currentPhase = 1;
    private DodgeMiniGame dodgeMiniGame;
    private Transform currentTransform;
    private Transform currentTarget;
    private BossMonster currentMonster;

   

    public void Initialize(BossData data)
    {
        bossData = data;
        dodgeMiniGame = new DodgeMiniGame();
        dodgeMiniGame.OnDodgeResultReceived += HandleDodgeResult;
        UpdateAvailablePatterns();
    }

    public override void Attack(Transform transform, Transform target, IMonsterClass monsterData)
    {
        currentTransform = transform;
        currentTarget = target;
        currentMonster = monsterData as BossMonster;

        if (!isExecutingPattern && CanStartNewPattern())
        {
            StartNewPattern();
        }
    }

    private void UpdateAvailablePatterns()
    {
        if (currentPhase - 1 < bossData.phaseData.Count)
        {
            currentPhasePatterns = bossData.phaseData[currentPhase - 1].availablePatterns;
        }
    }

    private void StartNewPattern()
    {
        if (currentPatternSequence != null && currentPatternSequence.IsPlaying())
        {
            currentPatternSequence.Kill();
        }

        currentPattern = SelectPattern(currentMonster);
        if (currentPattern != null)
        {
            ExecutePattern();
        }
    }

    private AttackPatternData SelectPattern(MonsterClass monsterData)
    {
        if (currentPhasePatterns == null || currentPhasePatterns.Count == 0)
            return null;

        float healthRatio = (float)monsterData.CurrentHealth / monsterData.MaxHealth;

        var availablePatterns = currentPhasePatterns.FindAll(p =>
            healthRatio >= p.healthThresholdMin && healthRatio <= p.healthThresholdMax);

        if (availablePatterns.Count == 0)
            return null;

        float totalWeight = availablePatterns.Sum(p => p.patternWeight);
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0;

        foreach (var pattern in availablePatterns)
        {
            currentWeight += pattern.patternWeight;
            if (randomValue <= currentWeight)
            {
                return pattern;
            }
        }

        return availablePatterns[0];
    }

    private void ExecutePattern()
    {
        isExecutingPattern = true;
        currentStepIndex = 0;

        currentPatternSequence = DOTween.Sequence();

        // 패턴 시작 경고
        if (!string.IsNullOrEmpty(currentPattern.warningMessage))
        {
            //currentPatternSequence.AppendCallback(() =>
            //    BossUIManager.Instance?.ShowWarning(currentPattern.warningMessage));
            //currentPatternSequence.AppendInterval(currentPattern.warningDuration);
        }

        // 각 스텝 실행
        foreach (var step in currentPattern.steps)
        {
            // 미니게임이 있는 경우
            if (step.hasMiniGame)
            {
                currentPatternSequence.AppendCallback(() => StartMiniGame(step));
                if (step.waitForMiniGame)
                {
                    currentPatternSequence.AppendInterval(GetMiniGameDuration(step.miniGameType));
                }
            }

            // 공격 실행
            currentPatternSequence.AppendCallback(() => ExecuteStep(step));
            currentPatternSequence.AppendInterval(step.stepDelay);
        }

        // 패턴 종료
        currentPatternSequence.OnComplete(() => {
            isExecutingPattern = false;
            lastAttackTime = Time.time;
        });

        currentPatternSequence.Play();
    }

    private void StartMiniGame(AttackStepData step)
    {
        switch (step.miniGameType)
        {
            case MiniGameType.Dodge:
                dodgeMiniGame.StartDodgeMiniGame();
                break;
                // 다른 미니게임 타입들...
        }
    }

    private float GetMiniGameDuration(MiniGameType type)
    {
        switch (type)
        {
            case MiniGameType.Dodge:
                return 3f; // 도지 미니게임 기본 시간
            default:
                return 1f;
        }
    }

    private void ExecuteStep(AttackStepData step)
    {
        IAttackStrategy baseAttack = CreateAttackStrategy(step.attackType);
        if (baseAttack != null)
        {
            baseAttack.Attack(currentTransform, currentTarget, currentMonster);
        }

        // 스텝 이펙트
        if (step.stepStartEffect != null)
        {
            GameObject.Instantiate(step.stepStartEffect,
                currentTransform.position,
                Quaternion.identity);
        }
    }

    private void HandleDodgeResult(DodgeMiniGame.DodgeResult result)
    {
        switch (result)
        {
            //case DodgeMiniGame.DodgeResult.Perfect:
            //    currentMonster.ApplyDamageMultiplier(0f);
            //    break;
            //case DodgeMiniGame.DodgeResult.Good:
            //    currentMonster.ApplyDamageMultiplier(0.3f);
            //    break;
            //case DodgeMiniGame.DodgeResult.Miss:
            //    currentMonster.ApplyDamageMultiplier(1f);
            //    break;
        }

        // UI 피드백
        //BossUIManager.Instance?.ShowDodgeResult(result);
    }

    private IAttackStrategy CreateAttackStrategy(AttackStrategyType type)
    {
        return StrategyFactory.CreateAttackStrategy(type, bossData);
    }

    public override bool CanAttack(float distanceToTarget, IMonsterClass monsterData)
    {
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
        currentPhase = newPhase;
        UpdateAvailablePatterns();
        StopCurrentPattern();
    }

    private void StopCurrentPattern()
    {
        if (currentPatternSequence != null && currentPatternSequence.IsPlaying())
        {
            currentPatternSequence.Kill();
        }
        isExecutingPattern = false;
    }

    public override PhysicalAttackType AttackType => PhysicalAttackType.Basic;
        //currentPattern?.steps[currentStepIndex]?.attackType.GetPhysicalAttackType() ?? PhysicalAttackType.Basic;

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

        if (dodgeMiniGame != null)
        {
            dodgeMiniGame.OnDodgeResultReceived -= HandleDodgeResult;
        }
    }
}