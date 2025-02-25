//using System.Collections.Generic;
//using UnityEngine;

///// <summary>
///// 여러 스킬 전략들을 담는 컨테이너
///// 독립적인 쿨타임 관리와 함께 우선순위 기반 선택을 지원합니다
///// </summary>
//public class BossMultiSkillStrategy : ISkillStrategy
//{
//    private List<ISkillStrategy> strategies = new List<ISkillStrategy>();
//    private Dictionary<ISkillStrategy, float> weights = new Dictionary<ISkillStrategy, float>();
//    private ISkillStrategy currentStrategy;
//    private readonly BasicSkillStrategy defaultStrategy;

//    // 통합 타이머 - BossMultiSkillStrategy 자체 쿨타임 관리용
//    private float unifiedLastSkillTime;

//    // 스킬 범위 - 기본값
//    private float skillRange = 10f;

//    public event System.Action OnSkillStateChanged;  // 상태 변경 이벤트 추가

//    public BossMultiSkillStrategy(CreatureAI owner)
//    {
//        defaultStrategy = new BasicSkillStrategy(owner);
//        unifiedLastSkillTime = 0f;
//    }

//    public bool IsSkillComplete => currentStrategy?.IsSkillComplete ?? true;
//    public bool IsUsingSkill => currentStrategy?.IsUsingSkill ?? false;

//    // 통합 스킬 쿨타임 - SkillState에서 이 값을 기준으로 스킬 사용 가능 여부를 결정
//    public float GetLastSkillTime => unifiedLastSkillTime;

//    public float SkillRange
//    {
//        get => skillRange;
//        set
//        {
//            skillRange = value;
//            // 모든 전략에 범위 적용
//            foreach (var strategy in strategies)
//            {
//                strategy.SkillRange = value;
//            }
//        }
//    }


//    // 스킬 추가 메서드 
//    public void AddSkillStrategy(
//        SkillStrategyType strategyType,   // 스킬 전략 타입 (멀티샷, 버프 등)
//        SkillEffectType effectType,       // 스킬 이펙트 타입
//        ProjectileMovementType moveType,  // 발사체 움직임 타입
//        ProjectileImpactType impactType,  // 발사체 충돌 효과 타입
//        float weight,                     // 가중치
//        CreatureAI owner,
//        ICreatureData data,
//        BuffData buffData = null)        // 버프 데이터 (null이면 기본값 사용)
//    {
//        // 각 구성 요소 생성
//        ISkillEffect skillEffect = StrategyFactory.CreateSkillEffect(effectType, data, owner);
//        IProjectileMovement moveStrategy = StrategyFactory.CreateProjectileMovement(moveType, data);
//        IProjectileImpact impactEffect = StrategyFactory.CreateProjectileImpact(impactType, data);

//        // 버프 스킬인 경우 BuffData 설정
//        if (strategyType == SkillStrategyType.Buff && buffData != null)
//        {
//            // data에 buffData 설정 (임시 객체 생성 또는 기존 데이터 업데이트)
//            if (data is BossData bossData)
//            {
//                bossData.buffData = buffData;
//            }
//        }

//        // 스킬 전략 생성
//        ISkillStrategy skillStrategy = StrategyFactory.CreateSkillStrategy(strategyType, owner);

//        // 구성 요소 주입
//        if (skillStrategy is ISkillStrategyComponentInjection injectionStrategy)
//        {
//            injectionStrategy.SetSkillEffect(skillEffect);
//            injectionStrategy.SetProjectileMovement(moveStrategy);
//            injectionStrategy.SetProjectileImpact(impactEffect);
//        }

//        // 전략과 가중치 저장
//        strategies.Add(skillStrategy);
//        weights[skillStrategy] = weight;
//    }

//    public void Initialize(ISkillEffect skillEffect)
//    {
//        // 모든 전략에 이펙트 초기화
//        foreach (var strategy in strategies)
//        {
//            strategy.Initialize(skillEffect);
//        }

//        // 기본 전략에도 초기화
//        defaultStrategy.Initialize(skillEffect);
//    }

//    public void StartSkill(Transform transform, Transform target, IMonsterClass monsterData)
//    {
//        if (currentStrategy == null || !currentStrategy.IsUsingSkill)
//        {
//            currentStrategy = SelectStrategy(Vector3.Distance(transform.position, target.position), monsterData);
//            Debug.Log($"선택된 스킬 전략: {currentStrategy.GetType().Name}");
//        }

//        if (currentStrategy != null)
//        {
//            currentStrategy.StartSkill(transform, target, monsterData);
//        }
//    }

//    public void UpdateSkill(Transform transform, Transform target, IMonsterClass monsterData)
//    {
//        if (currentStrategy != null && currentStrategy.IsUsingSkill)
//        {
//            currentStrategy.UpdateSkill(transform, target, monsterData);

//            // 스킬이 완료되면 통합 타이머 업데이트 및 현재 전략 초기화
//            if (currentStrategy.IsSkillComplete)
//            {
//                unifiedLastSkillTime = Time.time;  // 스킬 사용 후 통합 쿨타임 업데이트
//                currentStrategy = null;
//                OnSkillStateChanged?.Invoke();
//            }
//        }
//    }

//    public bool CanUseSkill(float distanceToTarget, IMonsterClass monsterData)
//    {
//        // 통합된 쿨타임 체크
//        if (Time.time < unifiedLastSkillTime + monsterData.CurrentSkillCooldown)
//            return false;

//        // 현재 스킬 사용 중이면 불가능
//        if (currentStrategy != null && currentStrategy.IsUsingSkill)
//            return false;

//        // 우선순위 순서대로 사용 가능한 첫 번째 스킬 찾기
//        foreach (var strategy in strategies)
//        {
//            if (strategy.CanUseSkill(distanceToTarget, monsterData))
//                return true;
//        }

//        return false;
//    }

//    private ISkillStrategy SelectStrategy(float distanceToTarget, IMonsterClass monsterData)
//    {
//        // 우선순위 순서대로 사용 가능한 첫 번째 스킬 선택
//        foreach (var strategy in strategies)
//        {
//            if (strategy.CanUseSkill(distanceToTarget, monsterData))
//                return strategy;
//        }

//        return defaultStrategy;
//    }

//    // 필요한 경우 스킬 중단 메서드
//    public void StopSkill()
//    {
//        if (currentStrategy != null && currentStrategy.IsUsingSkill)
//        {
//            // 구현되어 있다면 호출
//            if (currentStrategy is SkillState skillState)
//            {
//                skillState.Exit(); // 예시 - 실제 구현에 맞게 조정 필요
//            }
//            currentStrategy = null;
//        }
//    }

//    /// <summary>
//    /// 페이즈 전환 후 새 전략에 대해 스킬이 바로 실행되지 않도록 unifiedLastSkillTime을 현재 시간으로 재설정
//    /// </summary>
//    public void ResetTimer(float bufferTime = 1f)
//    {
//        unifiedLastSkillTime = Time.time + bufferTime;
//    }

//    /// <summary>
//    /// 내부 전략 리스트를 비우고, 타이머와 버퍼를 모두 초기화합니다.
//    /// </summary>
//    public void ResetAll()
//    {
//        // 내부 전략 리스트와 가중치 정보를 모두 비웁니다.
//        strategies.Clear();
//        weights.Clear();
//        currentStrategy = null;

//        // 타이머를 현재 시간으로 초기화합니다.
//        unifiedLastSkillTime = Time.time;

//        Debug.Log("ResetAll: 스킬 전략 목록 초기화 및 타이머 재설정 완료");
//    }
//}