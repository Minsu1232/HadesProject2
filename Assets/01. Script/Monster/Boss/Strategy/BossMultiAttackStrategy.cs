using System;
using System.Collections.Generic;
using UnityEngine;

public class BossMultiAttackStrategy : BasePhysicalAttackStrategy
{
    // ���� ������ ����ġ�� �Բ� �����ϴ� Ŭ����
    [Serializable]
    public class WeightedAttackStrategy
    {
        public IAttackStrategy Strategy;
        public float Weight;

        public WeightedAttackStrategy(IAttackStrategy strategy, float weight)
        {
            Strategy = strategy;
            Weight = weight;
        }
    }

    // ���� ���� ����Ʈ
    private List<WeightedAttackStrategy> attackStrategies;

    // ���� ���õ� ���� ����
    private IAttackStrategy currentAttackStrategy;

    // ���� ���� ��Ÿ�� ���� ����
    private float patternChangeCooldown = 5f;
    private float lastPatternChangeTime;

    // ���� ������
    private System.Random random = new System.Random();

    public BossMultiAttackStrategy(List<WeightedAttackStrategy> strategies)
    {
        attackStrategies = strategies;
        ValidateWeights();
    }

    // ����ġ ���� 1�� �ǵ��� ����
    private void ValidateWeights()
    {
        float totalWeight = 0;
        foreach (var strategy in attackStrategies)
        {
            totalWeight += strategy.Weight;
        }

        if (Mathf.Approximately(totalWeight, 0))
        {
            // �⺻������ �յ� �й�
            float defaultWeight = 1f / attackStrategies.Count;
            foreach (var strategy in attackStrategies)
            {
                strategy.Weight = defaultWeight;
            }
        }
        else if (!Mathf.Approximately(totalWeight, 1f))
        {
            // ������ �°� ����ȭ
            for (int i = 0; i < attackStrategies.Count; i++)
            {
                attackStrategies[i].Weight /= totalWeight;
            }
        }
    }

    // ����ġ ��� ���� ���� ���� ����
    private IAttackStrategy SelectRandomAttackStrategy()
    {
        float randomValue = (float)random.NextDouble();
        float cumulativeWeight = 0f;

        foreach (var strategyEntry in attackStrategies)
        {
            cumulativeWeight += strategyEntry.Weight;
            if (randomValue <= cumulativeWeight)
            {
                return strategyEntry.Strategy;
            }
        }

        // ���� ���� �߸��Ǿ��ٸ� ������ ���� ��ȯ
        return attackStrategies[attackStrategies.Count - 1].Strategy;
    }

    public override void Attack(Transform transform, Transform target, IMonsterClass monsterData)
    {
        // ���� ���� �ð� üũ
        if (Time.time >= lastPatternChangeTime + patternChangeCooldown)
        {
            currentAttackStrategy = SelectRandomAttackStrategy();
            lastPatternChangeTime = Time.time;
        }

        // ���� ���õ� ���� �������� ���� ����
        currentAttackStrategy?.Attack(transform, target, monsterData);
    }

    public override bool CanAttack(float distanceToTarget, IMonsterClass monsterData)
    {
        // ���� ���õ� ������ ���� ���� ���� Ȯ��
        return currentAttackStrategy?.CanAttack(distanceToTarget, monsterData) ?? false;
    }

    // �ٸ� �޼���鵵 ���� ���õ� ������ ����
    public override void StartAttack()
    {
        currentAttackStrategy?.StartAttack();
    }

    public override void StopAttack()
    {
        currentAttackStrategy?.StopAttack();
    }

    public override void OnAttackAnimationEnd()
    {
        currentAttackStrategy?.OnAttackAnimationEnd();
    }

    public override PhysicalAttackType AttackType =>
        currentAttackStrategy?.AttackType ?? PhysicalAttackType.Basic;
}