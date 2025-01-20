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
//        // BossDataManager���� ������ �ε�
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
//            // �߰� ������...
//            default:
//                return null;
//        }
//    }

//    private void InitializePatternStrategy()
//    {
//        List<AttackPatternData> patterns = new List<AttackPatternData>();

//        // �� ����� ���� ����
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

//        // �⺻ ���� ����
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

//        // ��ų ����
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
//        // ��ų �����Ϳ��� ������ ���� ����
//        IAttackStrategy skillStrategy = CreateSkillStrategy(phase.skillType);
//        if (skillStrategy == null) return null;

//        return new AttackStep
//        {
//            baseAttack = skillStrategy,
//            stepDelay = bossData.skillCooldown,
//            hasMiniGame = DetermineIfNeedsMiniGame(phase.skillType),
//            miniGameType = GetMiniGameType(phase.skillType),
//            miniGameDifficulty = phase.attackSpeedMultiplier // ���̵��� Ȱ��
//        };
//    }

//    private bool DetermineIfNeedsMiniGame(SkillStrategyType skillType)
//    {
//        // Ư�� ��ų Ÿ�Կ� ���� �̴ϰ��� �ʿ� ���� ����
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
//            // �ٸ� �̴ϰ��� Ÿ�Ե� �߰�
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

//        // ������ ��Ƽ�ö��̾� ����
//        monsterClass.ApplyDamageMultiplier(damageMultiplier);

//        // UI �ǵ��
//        if (UIManager.Instance != null)
//        {
//            UIManager.Instance.ShowDodgeResult(result);
//        }
//    }

//    public void OnPhaseChanged(int newPhase)
//    {
//        PhaseData phaseData = bossData.phaseData[newPhase - 1];

//        // ���� ���� ������Ʈ
//        if (patternStrategy != null)
//        {
//            patternStrategy.UpdatePhaseSettings(phaseData);
//        }

//        // ������ ��ȯ ����Ʈ/UI
//        if (UIManager.Instance != null)
//        {
//            UIManager.Instance.ShowPhaseTransition(phaseData.phaseName);
//        }
//    }
//}

//// AttackStrategyType Ȯ�� �޼���
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
//            // �ٸ� ���� �߰�
//            default:
//                return PhysicalAttackType.Basic;
//        }
//    }
//}