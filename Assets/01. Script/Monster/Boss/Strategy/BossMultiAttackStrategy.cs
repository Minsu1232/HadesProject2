using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossMultiAttackStrategy : BasePhysicalAttackStrategy
{
    // ������ ����ġ�� �����ϴ� ����Ʈ
    private List<IAttackStrategy> strategies = new List<IAttackStrategy>();
    private List<float> weights = new List<float>();
    private IAttackStrategy currentStrategy;

    public BossMultiAttackStrategy(List<IAttackStrategy> strategies, List<float> weights)
    {
        this.strategies = strategies;
        this.weights = weights;
        ValidateWeights();
        SelectRandomStrategy();
    }

    // ����ġ ���� �� ����ȭ
    private void ValidateWeights()
    {
        float totalWeight = weights.Sum();
        if (totalWeight <= 0)
        {
            // ����ġ�� ���ų� �߸��� ��� �յ� �й�
            float equalWeight = 1f / weights.Count;
            for (int i = 0; i < weights.Count; i++)
            {
                weights[i] = equalWeight;
            }
        }
        else if (!Mathf.Approximately(totalWeight, 1f))
        {
            // ����ġ ���� 1�� �ǵ��� ����ȭ
            for (int i = 0; i < weights.Count; i++)
            {
                weights[i] /= totalWeight;
            }
        }
    }

    public override void Attack(Transform transform, Transform target, IMonsterClass monsterData)
    {
        currentStrategy?.Attack(transform, target, monsterData);
    }

    // ����ġ ��� ���� ���� ����
    private void SelectRandomStrategy()
    {
        float totalWeight = weights.Sum();
        float random = UnityEngine.Random.Range(0f, totalWeight);
        float currentSum = 0f;

        for (int i = 0; i < strategies.Count; i++)
        {
            currentSum += weights[i];
            if (random <= currentSum)
            {
                currentStrategy = strategies[i];
                Debug.Log($"Selected new attack strategy: {currentStrategy}");
                break;
            }
        }
    }

    // AttackState���� ȣ���Ͽ� ���ο� ���� ����
    public void ChangePattern()
    {
        SelectRandomStrategy();
    }

    public override bool CanAttack(float distanceToTarget, IMonsterClass monsterData)
    {
        return currentStrategy?.CanAttack(distanceToTarget, monsterData) ?? false;
    }

    public override void StartAttack()
    {
        currentStrategy?.StartAttack();
    }

    public override void StopAttack()
    {
        currentStrategy?.StopAttack();
    }

    public override PhysicalAttackType AttackType =>
        currentStrategy.AttackType;

    public override string GetAnimationTriggerName()
    {
        return currentStrategy?.GetAnimationTriggerName() ?? "DefaultAttack";
    }

    public IEnumerable<IAttackStrategy> GetStrategies()
    {
        return strategies;
    }

    public override bool IsAttacking => currentStrategy?.IsAttacking ?? false;
}