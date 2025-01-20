using System;
using System.Collections.Generic;
using UnityEngine;

public class PhasedAttackStrategy : BasePhysicalAttackStrategy
{
    // 페이즈별 공격 전략 정의 클래스
    [Serializable]
    public class PhaseAttackData
    {
        public int PhaseNumber;
        public List<BossMultiAttackStrategy.WeightedAttackStrategy> AttackStrategies;

        public PhaseAttackData(int phaseNumber, List<BossMultiAttackStrategy.WeightedAttackStrategy> strategies)
        {
            PhaseNumber = phaseNumber;
            AttackStrategies = strategies;
        }
    }

    // 페이즈별 공격 전략 사전
    private Dictionary<int, BossMultiAttackStrategy> phaseAttackStrategies;

    // 현재 활성화된 페이즈의 공격 전략
    private BossMultiAttackStrategy currentPhaseAttackStrategy;

    // 현재 페이즈
    private int currentPhase;

    public PhasedAttackStrategy(List<PhaseAttackData> phaseAttackData)
    {
        InitializePhaseAttackStrategies(phaseAttackData);
    }

    private void InitializePhaseAttackStrategies(List<PhaseAttackData> phaseAttackData)
    {
        phaseAttackStrategies = new Dictionary<int, BossMultiAttackStrategy>();

        foreach (var phaseData in phaseAttackData)
        {
            var multiAttackStrategy = new BossMultiAttackStrategy(phaseData.AttackStrategies);
            phaseAttackStrategies[phaseData.PhaseNumber] = multiAttackStrategy;
        }

        // 기본적으로 첫 번째 페이즈 설정
        SetCurrentPhase(1);
    }

    // 현재 페이즈 변경 메서드
    public void SetCurrentPhase(int phase)
    {
        if (phaseAttackStrategies.TryGetValue(phase, out BossMultiAttackStrategy strategy))
        {
            currentPhase = phase;
            currentPhaseAttackStrategy = strategy;
        }
        else
        {
            Debug.LogWarning($"Phase {phase} attack strategy not found!");
        }
    }

    public override void Attack(Transform transform, Transform target, IMonsterClass monsterData)
    {
        currentPhaseAttackStrategy?.Attack(transform, target, monsterData);
    }

    public override bool CanAttack(float distanceToTarget, IMonsterClass monsterData)
    {
        return currentPhaseAttackStrategy?.CanAttack(distanceToTarget, monsterData) ?? false;
    }

    // 다른 메서드들도 현재 페이즈 전략에 위임
    public override void StartAttack()
    {
        currentPhaseAttackStrategy?.StartAttack();
    }

    public override void StopAttack()
    {
        currentPhaseAttackStrategy?.StopAttack();
    }

    public override void OnAttackAnimationEnd()
    {
        currentPhaseAttackStrategy?.OnAttackAnimationEnd();
    }

    public override PhysicalAttackType AttackType =>
        currentPhaseAttackStrategy?.AttackType ?? PhysicalAttackType.Basic;
}