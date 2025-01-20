//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class BossPatternManager : MonoBehaviour
//{
//    private BossAI bossAI;
//    private MonsterClass monsterClass;
//    private BossData bossData;

//    private PatternBasedAttackStrategy patternStrategy;
//    private Dictionary<PhysicalAttackType, IAttackStrategy> baseAttackStrategies;
//    private DodgeMiniGame dodgeMiniGame;

//    private void Awake()
//    {
//        bossAI = GetComponent<BossAI>();
//        monsterClass = GetComponent<MonsterClass>();
//        InitializeBaseAttackStrategies();
//        dodgeMiniGame = new DodgeMiniGame();
//        dodgeMiniGame.OnDodgeResultReceived += HandleDodgeResult;
//    }

//    private async void Start()
//    {
//        // BossDataManager에서 데이터 로드
//        bossData = BossDataManager.Instance.GetBossData(monsterClass.MonsterId);
//        if (bossData != null)
//        {
//            InitializePatternStrategy();
//        }
//    }

//    private void InitializeBaseAttackStrategies()
//    {
//        baseAttackStrategies = new Dictionary<PhysicalAttackType, IAttackStrategy>();
//        foreach (AttackStrategyType strategyType in Enum.GetValues(typeof(AttackStrategyType)))
//        {
//            IAttackStrategy strategy = CreateAttackStrategy(strategyType);
//            if (strategy != null)
//            {
//                baseAttackStrategies[strategy.AttackType] = strategy;
//            }
//        }
//    }

//    private IAttackStrategy CreateAttackStrategy(AttackStrategyType type)
//    {
//        switch (type)
//        {
//            case AttackStrategyType.Basic:
//                return new BasicAttackStrategy();
//            case AttackStrategyType.Rush:
//                return new RushAttackStrategy();
//            case AttackStrategyType.Melee:
//                return new MeleeAttackStrategy();
//            // 추가 전략들...
//            default:
//                return null;
//        }
//    }

//    private void InitializePatternStrategy()
//    {
//        List<AttackPatternData> patterns = new List<AttackPatternData>();

//        // 각 페이즈별 패턴 생성
//        foreach (PhaseData phase in bossData.phaseData)
//        {
//            AttackPatternData pattern = CreatePhasePattern(phase);
//            if (pattern != null)
//            {
//                patterns.Add(pattern);
//            }
//        }

//        patternStrategy = new PatternBasedAttackStrategy(patterns);
//        bossAI.SetAttackStrategy(patternStrategy);
//    }

//    private AttackPatternData CreatePhasePattern(PhaseData phase)
//    {
//        AttackPatternData pattern = new AttackPatternData
//        {
//            patternName = phase.phaseName,
//            patternCooldown = phase.patternChangeTime,
//            warningDuration = phase.transitionDuration,
//            attackSteps = new List<AttackStep>()
//        };

//        // 기본 공격 스텝
//        if (baseAttackStrategies.TryGetValue(phase.attackType.GetPhysicalAttackType(), out var baseAttack))
//        {
//            AttackStep basicStep = new AttackStep
//            {
//                baseAttack = baseAttack,
//                stepDelay = 1.0f / monsterClass.CurrentAttackSpeed,
//                hasMiniGame = false
//            };
//            pattern.attackSteps.Add(basicStep);
//        }

//        // 스킬 스텝
//        if (phase.skillType != SkillStrategyType.None)
//        {
//            AttackStep skillStep = CreateSkillStep(phase);
//            if (skillStep != null)
//            {
//                pattern.attackSteps.Add(skillStep);
//            }
//        }

//        return pattern;
//    }

//    private AttackStep CreateSkillStep(PhaseData phase)
//    {
//        // 스킬 데이터에서 적절한 전략 생성
//        IAttackStrategy skillStrategy = CreateSkillStrategy(phase.skillType);
//        if (skillStrategy == null) return null;

//        return new AttackStep
//        {
//            baseAttack = skillStrategy,
//            stepDelay = bossData.skillCooldown,
//            hasMiniGame = DetermineIfNeedsMiniGame(phase.skillType),
//            miniGameType = GetMiniGameType(phase.skillType),
//            miniGameDifficulty = phase.attackSpeedMultiplier // 난이도로 활용
//        };
//    }

//    private bool DetermineIfNeedsMiniGame(SkillStrategyType skillType)
//    {
//        // 특정 스킬 타입에 대해 미니게임 필요 여부 결정
//        switch (skillType)
//        {
//            case SkillStrategyType.JumpAttack:
//            case SkillStrategyType.ChargeAttack:
//                return true;
//            default:
//                return false;
//        }
//    }

//    private MiniGameType GetMiniGameType(SkillStrategyType skillType)
//    {
//        switch (skillType)
//        {
//            case SkillStrategyType.JumpAttack:
//            case SkillStrategyType.ChargeAttack:
//                return MiniGameType.Dodge;
//            // 다른 미니게임 타입들 추가
//            default:
//                return MiniGameType.None;
//        }
//    }

//    private void HandleDodgeResult(DodgeMiniGame.DodgeResult result)
//    {
//        float damageMultiplier = 1.0f;
//        switch (result)
//        {
//            case DodgeMiniGame.DodgeResult.Perfect:
//                damageMultiplier = 0f;
//                break;
//            case DodgeMiniGame.DodgeResult.Good:
//                damageMultiplier = 0.3f;
//                break;
//            case DodgeMiniGame.DodgeResult.Miss:
//                damageMultiplier = 1f;
//                break;
//        }

//        // 데미지 멀티플라이어 적용
//        monsterClass.ApplyDamageMultiplier(damageMultiplier);

//        // UI 피드백
//        if (UIManager.Instance != null)
//        {
//            UIManager.Instance.ShowDodgeResult(result);
//        }
//    }

//    public void OnPhaseChanged(int newPhase)
//    {
//        PhaseData phaseData = bossData.phaseData[newPhase - 1];

//        // 패턴 전략 업데이트
//        if (patternStrategy != null)
//        {
//            patternStrategy.UpdatePhaseSettings(phaseData);
//        }

//        // 페이즈 전환 이펙트/UI
//        if (UIManager.Instance != null)
//        {
//            UIManager.Instance.ShowPhaseTransition(phaseData.phaseName);
//        }
//    }
//}

//// AttackStrategyType 확장 메서드
//public static class AttackStrategyTypeExtensions
//{
//    public static PhysicalAttackType GetPhysicalAttackType(this AttackStrategyType type)
//    {
//        switch (type)
//        {
//            //case AttackStrategyType.Rush:
//            //    return PhysicalAttackType.Rush;
//            case AttackStrategyType.Melee:
//                return PhysicalAttackType.Basic;
//            // 다른 매핑 추가
//            default:
//                return PhysicalAttackType.Basic;
//        }
//    }
//}