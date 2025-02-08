using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossMultiAttackStrategy : BasePhysicalAttackStrategy
{
    // 전략과 가중치를 저장하는 리스트
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

    // 가중치 검증 및 정규화
    private void ValidateWeights()
    {
        float totalWeight = weights.Sum();
        if (totalWeight <= 0)
        {
            // 가중치가 없거나 잘못된 경우 균등 분배
            float equalWeight = 1f / weights.Count;
            for (int i = 0; i < weights.Count; i++)
            {
                weights[i] = equalWeight;
            }
        }
        else if (!Mathf.Approximately(totalWeight, 1f))
        {
            // 가중치 합이 1이 되도록 정규화
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

    // 가중치 기반 랜덤 전략 선택
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

    // AttackState에서 호출하여 새로운 패턴 선택
    public void ChangePattern()
    {
        if (!isFirstStrategy)  // 첫 전략이 아닐 때만 새로 선택
        {
            SelectRandomStrategy();
        }
        isFirstStrategy = false;  // 다음부터는 새로 선택하도록
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