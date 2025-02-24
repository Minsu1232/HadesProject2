using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// 어택들을 넣을 컨테이너 가중치를 부여해 선택 후 공격
/// </summary>
public class BossMultiAttackStrategy : IAttackStrategy
{
    private List<IAttackStrategy> strategies = new List<IAttackStrategy>();
    private Dictionary<IAttackStrategy, float> weights = new Dictionary<IAttackStrategy, float>();
    private IAttackStrategy currentStrategy;
    private readonly BasicAttackStrategy defaultStrategy;

    // 통합 타이머: 페이즈 전환 후 초기화할 수 있도록 public 메서드 제공
    private float unifiedLastAttackTime;

    public event System.Action OnStrategyStateChanged;  // 상태 변경 이벤트 추가
    public BossMultiAttackStrategy()
    {
        defaultStrategy = new BasicAttackStrategy();
        // 기본 생성 시 바로 공격 가능하도록 (개발 중에는 이렇게, 페이즈 전환 시에는 ResetTimer()로 재설정)
       
    }

    public bool IsAttacking => currentStrategy?.IsAttacking ?? false;
    public float GetLastAttackTime => unifiedLastAttackTime;
    public PhysicalAttackType AttackType => currentStrategy?.AttackType ?? defaultStrategy.AttackType;

    public void AddStrategy(IAttackStrategy strategy, float weight)
    {
        if (!strategies.Contains(strategy))
        {
            strategies.Add(strategy);
            weights[strategy] = weight;
            Debug.Log(strategies.ToString() + "추가");
        }
    }

    public void Attack(Transform transform, Transform target, IMonsterClass monsterData)
    {

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (Time.time < unifiedLastAttackTime + monsterData.CurrentAttackSpeed)
            return;

        if (currentStrategy == null ||
            (!currentStrategy.IsAttacking && !currentStrategy.CanAttack(distanceToTarget, monsterData)))
        {
            if (currentStrategy?.IsAttacking ?? false)
                return;

            currentStrategy = SelectStrategy(distanceToTarget, monsterData);
            
            // 새로운 전략이 ChargeAttackStrategy인 경우 이벤트 구독
            if (currentStrategy is ChargeAttackStrategy chargeStrategy)
            {
                chargeStrategy.OnChargeStateChanged += () => 
                {
                    OnStrategyStateChanged?.Invoke();
                };
            }
        }

        if (currentStrategy != null && currentStrategy.CanAttack(distanceToTarget, monsterData))
        {
            currentStrategy.Attack(transform, target, monsterData);
        }
    } 

    public bool CanAttack(float distanceToTarget, IMonsterClass monsterData)
    {
        if (Time.time < unifiedLastAttackTime + monsterData.CurrentAttackSpeed)
            return false;

        if (currentStrategy != null && currentStrategy.CanAttack(distanceToTarget, monsterData))
            return true;

        return defaultStrategy.CanAttack(distanceToTarget, monsterData);
    }

    private IAttackStrategy SelectStrategy(float distanceToTarget, IMonsterClass monsterData)
    {
        var availableStrategies = strategies.Where(s => s.CanAttack(distanceToTarget, monsterData)).ToList();
        Debug.Log($"사용 가능한 전략 수: {availableStrategies.Count}");
        foreach (var strat in availableStrategies)
        {
            Debug.Log($"가능한 전략: {strat.GetType().Name}");
        }
        if (availableStrategies.Count == 0)
        {
            Debug.Log("전략없음");
            return defaultStrategy;
        }

        float totalWeight = availableStrategies.Sum(s => weights[s]);
        float random = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var strat in availableStrategies)
        {
            cumulative += weights[strat];
            if (random <= cumulative)
                return strat;
        }

        // foreach 루프에서 선택되지 않았을 경우의 기본값
        return availableStrategies[0];
    }

    public void StopAttack()
    {
        currentStrategy?.StopAttack();
        currentStrategy = null;
        UpdateLastAttackTime();
    }

    public string GetAnimationTriggerName() => currentStrategy?.GetAnimationTriggerName() ?? defaultStrategy.GetAnimationTriggerName();
    public float GetAttackPowerMultiplier() => currentStrategy?.GetAttackPowerMultiplier() ?? defaultStrategy.GetAttackPowerMultiplier();
    public void StartAttack() => currentStrategy?.StartAttack();
    public void OnAttackAnimationEnd() => currentStrategy?.OnAttackAnimationEnd();
    public void ApplyDamage(IDamageable target, IMonsterClass monsterData) => currentStrategy?.ApplyDamage(target, monsterData);

    public void UpdateLastAttackTime()
    {
        
        unifiedLastAttackTime = Time.time;
       
        if (currentStrategy is BasePhysicalAttackStrategy baseStrategy)
        {
            baseStrategy.UpdateLastAttackTime();
        }
    }
    // Execute 로직 추가
    public void UpdateStrategy(Transform transform)
    {
        if (currentStrategy is ChargeAttackStrategy chargeStrategy)
        {
            chargeStrategy.UpdateCharge(transform);
        }
    }
    /// <summary>
    /// 페이즈 전환 후 새 전략에 대해 공격이 바로 실행되지 않도록 unifiedLastAttackTime을 현재 시간으로 재설정
    /// </summary>
    public void ResetTimer(float bufferTime = 1f)
    {
        unifiedLastAttackTime = Time.time + bufferTime;
    }
    /// <summary>
    /// 내부 전략 리스트를 비우고, 타이머와 버퍼를 모두 초기화합니다.
    /// </summary>
    public void ResetAll()
    {
        // 내부 전략 리스트와 가중치 정보를 모두 비웁니다.
        strategies.Clear();
        weights.Clear();
        currentStrategy = null;

        // 타이머를 현재 시간으로 초기화합니다.
        unifiedLastAttackTime = Time.time;

        Debug.Log("ResetAll: Strategies cleared and timer reset to " + unifiedLastAttackTime);
    }

    public void ResetAttackTime()
    {
        throw new System.NotImplementedException();
    }
}
