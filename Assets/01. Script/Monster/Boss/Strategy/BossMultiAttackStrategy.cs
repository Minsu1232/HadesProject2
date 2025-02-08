using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossMultiAttackStrategy : BasePhysicalAttackStrategy
{
    // ������ ����ġ�� �����ϴ� ����Ʈ
    private List<IAttackStrategy> strategies = new List<IAttackStrategy>();
    private List<float> weights = new List<float>();
    private IAttackStrategy _currentStrategy;
    private IAttackStrategy currentStrategy
    {
        get => _currentStrategy;
        set
        {
            _currentStrategy = value;
            Debug.Log($"Current strategy changed to: {value?.GetType().Name}\nStack trace:\n{new System.Diagnostics.StackTrace(true)}");
        }
    }
    private bool isFirstStrategy = true;
    public BossMultiAttackStrategy(List<IAttackStrategy> strategies, List<float> weights)
    {
        this.strategies = strategies;
        this.weights = weights;
        Debug.Log($"BossMultiAttackStrategy initialized with strategies:");
        foreach (var strategy in strategies)
        {
            Debug.Log($"- {strategy.GetType().Name}");
        }
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
        var stackTrace = new System.Diagnostics.StackTrace(true);
        Debug.Log($"SelectRandomStrategy called from:\n{stackTrace}");
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
        if (!isFirstStrategy)  // ù ������ �ƴ� ���� ���� ����
        {
            SelectRandomStrategy();
        }
        isFirstStrategy = false;  // �������ʹ� ���� �����ϵ���
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
        Debug.Log($"Current Strategy: {currentStrategy?.GetType().Name}");
        Debug.Log($"Current Strategy Attack Type: {currentStrategy?.AttackType}");
        return currentStrategy?.GetAnimationTriggerName() ?? "DefaultAttack";
    }

    public IEnumerable<IAttackStrategy> GetStrategies()
    {
        return strategies;
    }

    public override bool IsAttacking => currentStrategy.IsAttacking;
}