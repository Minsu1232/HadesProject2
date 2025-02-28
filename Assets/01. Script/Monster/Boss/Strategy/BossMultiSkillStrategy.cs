using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 보스가 여러 스킬 전략을 사용할 수 있게 해주는 컨테이너 클래스
/// 가중치 기반으로 스킬을 선택하고 쿨타임을 관리합니다.
/// </summary>
public class BossMultiSkillStrategy : ISkillStrategy
{
    private List<ISkillStrategy> strategies = new List<ISkillStrategy>();
    private Dictionary<ISkillStrategy, float> weights = new Dictionary<ISkillStrategy, float>();
    private ISkillStrategy currentStrategy;
    private readonly BasicSkillStrategy defaultStrategy;

    // 통합 타이머 - BossMultiSkillStrategy 자체 쿨타임 관리용
    private float unifiedLastSkillTime;

    // 상태 변경 이벤트 추가
    public event System.Action OnSkillStateChanged;

    public bool IsSkillComplete => currentStrategy?.IsSkillComplete ?? true;
    public bool IsUsingSkill => currentStrategy?.IsUsingSkill ?? false;
    public float GetLastSkillTime => unifiedLastSkillTime;
    public float SkillRange { get; set; } = 10f; // 기본값

    public BossMultiSkillStrategy(CreatureAI owner)
    {
        defaultStrategy = new BasicSkillStrategy(owner);
        unifiedLastSkillTime = 0f;
    }

    /// <summary>
    /// 스킬 전략을 가중치와 함께 추가합니다.
    /// </summary>
    /// <param name="strategy">추가할 스킬 전략</param>
    /// <param name="weight">선택 가중치</param>
    public void AddStrategy(ISkillStrategy strategy, float weight)
    {
        if (!strategies.Contains(strategy))
        {
            strategies.Add(strategy);
            weights[strategy] = weight;
            // 전략에 스킬 범위 설정
            strategy.SkillRange = this.SkillRange;
            Debug.Log($"스킬 전략 추가됨: {strategy.GetType().Name}, 가중치: {weight}");
        }
    }
    /// <summary>
    /// 설정 ID를 기반으로 미리 구성된 스킬 전략을 추가합니다.
    /// </summary>
    /// <param name="configId">구성 ID</param>
    /// <param name="weight">선택 가중치</param>
    /// <param name="owner">AI 소유자</param>
    /// <param name="data">몬스터 데이터</param>
    public void AddSkillStrategyFromConfig(
        int configId,
        float weight,
        CreatureAI owner,
        ICreatureData data)
    {
        try
        {
            Debug.Log($"[AddSkillStrategyFromConfig] 구성 ID: {configId}, 가중치: {weight}");

            if (owner == null)
            {
                Debug.LogError("[AddSkillStrategyFromConfig] owner는 null일 수 없습니다.");
                return;
            }

            if (data == null)
            {
                Debug.LogError("[AddSkillStrategyFromConfig] data는 null일 수 없습니다.");
                return;
            }

            // SkillStrategyFactory를 통해 스킬 전략 생성
            ISkillStrategy skillStrategy = SkillStrategyFactory.CreateFromConfig(configId, owner, data);

            if (skillStrategy != null)
            {
                // 이미 동일한 전략이 있는지 확인
                if (strategies.Contains(skillStrategy))
                {
                    Debug.LogWarning($"[AddSkillStrategyFromConfig] 해당 스킬 전략이 이미 존재합니다: {skillStrategy.GetType().Name}");
                    return;
                }

                AddStrategy(skillStrategy, weight);
                Debug.Log($"[AddSkillStrategyFromConfig] 스킬 전략 추가 완료: {skillStrategy.GetType().Name}, 가중치: {weight}");
            }
            else
            {
                Debug.LogError($"[AddSkillStrategyFromConfig] 스킬 전략 생성 실패: ID {configId}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[AddSkillStrategyFromConfig] 오류 발생: {e.Message}\n{e.StackTrace}");
        }
    }


    // 버프 타입 문자열을 BuffType 배열로 변환
    private BuffType[] ParseBuffTypes(string buffTypesString)
    {
        if (string.IsNullOrEmpty(buffTypesString))
            return new BuffType[0];

        string[] types = buffTypesString.Split('|');
        BuffType[] result = new BuffType[types.Length];

        for (int i = 0; i < types.Length; i++)
        {
            if (System.Enum.TryParse(types[i], out BuffType buffType))
                result[i] = buffType;
            else
                result[i] = BuffType.None;
        }

        return result;
    }

    // 구분자로 나누어진 float 값 문자열을 float 배열로 변환
    private float[] ParseFloatArray(string floatString)
    {
        if (string.IsNullOrEmpty(floatString))
            return new float[0];

        string[] values = floatString.Split('|');
        float[] result = new float[values.Length];

        for (int i = 0; i < values.Length; i++)
        {
            if (float.TryParse(values[i], out float value))
                result[i] = value;
        }

        return result;
    }

    public void Initialize(ISkillEffect skillEffect)
    {
        // 모든 전략에 이펙트 초기화
        foreach (var strategy in strategies)
        {
            strategy.Initialize(skillEffect);
        }

        // 기본 전략에도 초기화
        defaultStrategy.Initialize(skillEffect);
    }

    public void StartSkill(Transform transform, Transform target, IMonsterClass monsterData)
    {
        if (currentStrategy == null || !currentStrategy.IsUsingSkill)
        {
            currentStrategy = SelectStrategy(Vector3.Distance(transform.position, target.position), monsterData);
            Debug.Log($"선택된 스킬 전략: {currentStrategy.GetType().Name}");
        }

        if (currentStrategy != null)
        {
            Debug.Log(currentStrategy.ToString());
            
            currentStrategy.StartSkill(transform, target, monsterData);
        }
    }

    public void UpdateSkill(Transform transform, Transform target, IMonsterClass monsterData)
    {
        if (currentStrategy != null && currentStrategy.IsUsingSkill)
        {
            currentStrategy.UpdateSkill(transform, target, monsterData);

            // 스킬이 완료되면 통합 타이머 업데이트 및 현재 전략 초기화
            if (currentStrategy.IsSkillComplete)
            {
                unifiedLastSkillTime = Time.time;  // 스킬 사용 후 통합 쿨타임 업데이트
                currentStrategy = null;
                OnSkillStateChanged?.Invoke();
            }
        }
    }

    public bool CanUseSkill(float distanceToTarget, IMonsterClass monsterData)
    {
      

        float cooldownTime = monsterData.CurrentSkillCooldown;
        float timeSinceLastSkill = Time.time - unifiedLastSkillTime;
        bool cooldownReady = Time.time >= unifiedLastSkillTime + cooldownTime;

       

        // 통합된 쿨타임 체크
        if (!cooldownReady)
            return false;

        // 현재 스킬 사용 중이면 불가능
        if (IsUsingSkill)
            return false;

        // 우선순위 순서대로 사용 가능한 첫 번째 스킬 찾기
        bool anyStrategyReady = false;
        foreach (var strategy in strategies)
        {
            bool canUse = strategy.CanUseSkill(distanceToTarget, monsterData);
            Debug.Log($"Strategy {strategy.GetType().Name}: CanUse={canUse}, Distance={distanceToTarget}");
            if (canUse)
            {
                anyStrategyReady = true;
                break;
            }
        }

        return anyStrategyReady;
    }

    private ISkillStrategy SelectStrategy(float distanceToTarget, IMonsterClass monsterData)
    {
        // 가중치 기반으로 스킬 선택
        var availableStrategies = strategies
            .Where(s => s.CanUseSkill(distanceToTarget, monsterData))
            .ToList();

        if (availableStrategies.Count == 0)
            return defaultStrategy;

        float totalWeight = availableStrategies.Sum(s => weights[s]);
        float random = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var strategy in availableStrategies)
        {
            cumulative += weights[strategy];
            if (random <= cumulative)
                return strategy;
        }

        // 가중치 기반 선택이 실패한 경우 첫 번째 전략 반환
        return availableStrategies[0];
    }

    /// <summary>
    /// 필요한 경우 스킬 중단 메서드
    /// </summary>
    public void StopSkill()
    {
        if (currentStrategy != null && currentStrategy.IsUsingSkill)
        {
            // 구현되어 있다면 호출
            if (currentStrategy is SkillState skillState)
            {
                skillState.Exit(); // 예시 - 실제 구현에 맞게 조정 필요
            }
            currentStrategy = null;
        }
    }

    /// <summary>
    /// 페이지 전환 후 새 전략에 대해 스킬이 바로 실행되지 않도록 unifiedLastSkillTime을 현재 시간으로 재설정
    /// </summary>
    public void ResetTimer(float bufferTime = 1f)
    {
        unifiedLastSkillTime = Time.time + bufferTime;
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
        unifiedLastSkillTime = Time.time;

        Debug.Log("ResetAll: 스킬 전략 목록 초기화 및 타이머 재설정 완료");
    }
}