using System;
using System.Collections.Generic;
using UnityEngine;

public class PhasedAttackStrategy : BasePhysicalAttackStrategy
{
    // ����� ���� ���� ���� Ŭ����
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

    // ����� ���� ���� ����
    private Dictionary<int, BossMultiAttackStrategy> phaseAttackStrategies;

    // ���� Ȱ��ȭ�� �������� ���� ����
    private BossMultiAttackStrategy currentPhaseAttackStrategy;

    // ���� ������
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

        // �⺻������ ù ��° ������ ����
        SetCurrentPhase(1);
    }

    // ���� ������ ���� �޼���
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

    // �ٸ� �޼���鵵 ���� ������ ������ ����
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