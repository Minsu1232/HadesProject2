using System;
using UnityEngine;

/// <summary>
/// 스킬 ID와 설정 기반으로 스킬 전략 객체를 생성하는 팩토리 클래스
/// </summary>
public static class SkillStrategyFactory
{
    /// <summary>
    /// 스킬 구성 ID를 기반으로 스킬 전략 객체를 생성합니다.
    /// </summary>
    /// <param name="configId">스킬 구성 ID</param>
    /// <param name="owner">소유자 AI</param>
    /// <param name="data">몬스터 데이터</param>
    /// <returns>생성된 스킬 전략 객체</returns>
    public static ISkillStrategy CreateFromConfig(int configId, CreatureAI owner, ICreatureData data)
    {
        var config = SkillConfigManager.Instance.GetSkillConfig(configId);
        if (config == null)
        {
            Debug.LogError($"스킬 구성을 찾을 수 없음: ID {configId}");
            return null;
        }

        Debug.Log($"[SkillStrategyFactory] 스킬 전략 생성 시작: {config.configName} (ID: {configId})");

        try
        {
            // 스킬 전략 생성
            ISkillStrategy skillStrategy = StrategyFactory.CreateSkillStrategy(config.strategyType, owner);
            if (skillStrategy == null)
            {
                Debug.LogError($"[SkillStrategyFactory] 스킬 전략 생성 실패: {config.strategyType}");
                return null;
            }

            // StrategyFactory로 이동 전략 먼저 생성 (컨피그의 moveType 사용)
            IProjectileMovement moveStrategy = StrategyFactory.CreateProjectileMovement(config.moveType, data);
            IProjectileImpact impactEffect = StrategyFactory.CreateProjectileImpact(config.impactType, data);

            // 커스텀 SkillEffect 생성
            ISkillEffect skillEffect = null;

            // 보스 데이터 가져오기
            BossData bossData = data as BossData;
            int bossId = bossData?.BossID ?? 0;

            // 이펙트 타입별 처리
            switch (config.effectType)
            {
                case SkillEffectType.Projectile:
                    if (bossData != null)
                    {
                        skillEffect = CreateProjectileSkillEffect(configId, bossId, bossData, owner,
                                                                 moveStrategy, impactEffect, config);
                    }
                    break;

                case SkillEffectType.CircularProjectile:
                    if (bossData != null)
                    {
                        skillEffect = CreateCircularProjectileSkillEffect(configId, bossId, bossData, owner,
                                                                        moveStrategy, impactEffect, config);
                    }
                    break;

                case SkillEffectType.Howl:
                    if (bossData != null)
                    {
                        skillEffect = CreateHowlSkillEffect(configId, bossId, bossData, owner,
                                                          moveStrategy, impactEffect, config);
                    }
                    break;

                default:
                    // 그 외 이펙트 타입은 기본 생성 로직 사용
                    skillEffect = StrategyFactory.CreateSkillEffect(config.effectType, data, owner);
                    break;
            }

            // skillEffect가 null인 경우 기본 생성 시도
            if (skillEffect == null)
            {
                try
                {
                    skillEffect = StrategyFactory.CreateSkillEffect(config.effectType, data, owner);

                    // 타입별 추가 초기화
                    InitializeSkillEffect(skillEffect, config, owner, data);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SkillStrategyFactory] 기본 스킬 이펙트 생성 중 오류: {e.Message}");
                }
            }

            if (skillEffect == null)
            {
                Debug.LogError($"[SkillStrategyFactory] 스킬 이펙트 생성 실패: {config.effectType}");
                return null;
            }

            // 컴포넌트 주입이 필요한 전략이면 주입
            if (skillStrategy is ISkillStrategyComponentInjection injectionStrategy)
            {
                Debug.Log($"[SkillStrategyFactory] 컴포넌트 주입: MoveType={config.moveType}");
                injectionStrategy.SetSkillEffect(skillEffect);
                injectionStrategy.SetProjectileMovement(moveStrategy);
                injectionStrategy.SetProjectileImpact(impactEffect);
            }

            // 버프 스킬 처리
            if (config.strategyType == SkillStrategyType.Buff && bossData != null)
            {
                SetupBuffData(config, bossData);
            }

            // 스킬 초기화 (컴포넌트 주입 후)
            skillStrategy.Initialize(skillEffect);
            skillStrategy.SkillRange = data.skillRange;

            Debug.Log($"[SkillStrategyFactory] 스킬 전략 생성 완료: {skillStrategy.GetType().Name}");
            return skillStrategy;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SkillStrategyFactory] 스킬 전략 생성 중 오류: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// 스킬 이펙트 초기화
    /// </summary>
    private static void InitializeSkillEffect(ISkillEffect skillEffect, SkillConfig config, CreatureAI owner, ICreatureData data)
    {
        // 프로젝타일 이펙트인 경우 Initialize 호출
        if (config.effectType == SkillEffectType.Projectile && skillEffect is ProjectileSkillEffect projectileEffect)
        {
            projectileEffect.Initialize(
                owner.GetStatus(),
                null, // target은 스킬 시작 시 설정됨
                config.damageMultiplier,
                config.speedMultiplier
            );
        }
        // CircularProjectile 이펙트인 경우 Initialize 호출
        else if (config.effectType == SkillEffectType.CircularProjectile && skillEffect is CircularProjectileSkillEffect circularEffect)
        {
            circularEffect.Initialize(
                owner.GetStatus(),
                null,
                config.damageMultiplier,
                config.speedMultiplier
            );
        }
        // Howl 이펙트인 경우 Initialize 호출
        else if (config.effectType == SkillEffectType.Howl && skillEffect is HowlSkillEffect howlEffect)
        {
            howlEffect.Initialize(
                owner.GetStatus(),
                owner.transform,
                config.damageMultiplier,
                config.speedMultiplier > 0 ? config.speedMultiplier : 1.0f
            );
        }
    }

    /// <summary>
    /// CircularProjectileSkillEffect 생성
    /// </summary>
    private static ISkillEffect CreateCircularProjectileSkillEffect(int configId, int bossId, BossData bossData,
                                                    CreatureAI owner, IProjectileMovement moveStrategy,
                                                    IProjectileImpact impactEffect, SkillConfig config)
    {
        try
        {
            // 필요한 프리팹 가져오기
            GameObject customPrefab = BossDataManager.Instance.GetSkillPrefab(bossId, configId);
            GameObject customHitPrefab = BossDataManager.Instance.GetSkillImpactPrefab(bossId, configId);

            if (customPrefab == null)
            {
                Debug.LogWarning($"[SkillStrategyFactory] 보스 {bossId}의 스킬 {configId}에 프리팹이 없습니다.");
                return null;
            }

            // 지연 폭발 임팩트 타입 생성
            DelayedExplosionImpact delayedImpact = new DelayedExplosionImpact(
                2f, // 안전 구역 반경
                1.75f, // 위험 영역 배수
                bossData.skillDuration, // 폭발 지연 시간
                true, // 기본값으로 링형(빨간색) 설정
                customHitPrefab
            );

            // CircularProjectileSkillEffect 생성
            CircularProjectileSkillEffect circularEffect = new CircularProjectileSkillEffect(
                customPrefab,
                bossData.circleIndicatorPrefab,
                bossData.projectileSpeed,
                moveStrategy,
                delayedImpact, // 지연 폭발 임팩트 사용
                customHitPrefab,
                bossData.heightFactor
            );

            // 추가 파라미터 설정
            circularEffect.SetCircleParameters(
                5, // 기본 발사체 개수
                bossData.areaRadius, // 원의 반지름
                2f, // 안전 구역 반경
                2f, // 위험 영역 배수
                bossData.howlDuration,
                bossData.EssenceAmount*1.5f// 폭발 지연 시간
            );

            // Initialize 호출하여 데미지 계수 전달
            circularEffect.Initialize(
                owner.GetStatus(),
                null, // target은 스킬 시작 시 설정됨
                config.damageMultiplier, // 데미지 계수 적용
                config.speedMultiplier // 발사체 스피드 계수 적용
            );

            Debug.Log($"[SkillStrategyFactory] 보스 {bossId}의 CircularProjectile 스킬 {configId} 생성 완료");
            return circularEffect;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SkillStrategyFactory] CircularProjectileSkillEffect 생성 오류: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// ProjectileSkillEffect 생성
    /// </summary>
    private static ISkillEffect CreateProjectileSkillEffect(int configId, int bossId, BossData bossData,
                                                CreatureAI owner, IProjectileMovement moveStrategy,
                                                IProjectileImpact impactEffect, SkillConfig config)
    {
        try
        {
            // 필요한 프리팹 가져오기
            GameObject customPrefab = BossDataManager.Instance.GetSkillPrefab(bossId, configId);
            GameObject customHitPrefab = BossDataManager.Instance.GetSkillImpactPrefab(bossId, configId);

            if (customPrefab == null)
            {
                Debug.LogWarning($"[SkillStrategyFactory] 보스 {bossId}의 스킬 {configId}에 프리팹이 없습니다.");
                return null;
            }

            // ProjectileSkillEffect 생성
            ProjectileSkillEffect projectileEffect = new ProjectileSkillEffect(
                customPrefab,
                bossData.projectileSpeed,
                moveStrategy,
                impactEffect,
                customHitPrefab,
                bossData.heightFactor
            );

            // Initialize 호출하여 데미지 계수 전달
            projectileEffect.Initialize(
                owner.GetStatus(),
                null, // target은 스킬 시작 시 설정됨
                config.damageMultiplier, // 데미지 계수 적용
                config.speedMultiplier // 발사체 스피드 계수 적용
            );

            Debug.Log($"[SkillStrategyFactory] 보스 {bossId}의 Projectile 스킬 {configId} 생성 완료");
            return projectileEffect;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SkillStrategyFactory] ProjectileSkillEffect 생성 오류: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// HowlSkillEffect 생성
    /// </summary>
    private static ISkillEffect CreateHowlSkillEffect(int configId, int bossId, BossData bossData,
                                         CreatureAI owner, IProjectileMovement moveStrategy,
                                         IProjectileImpact impactEffect, SkillConfig config)
    {
        try
        {
            // 필요한 프리팹 가져오기
            GameObject howlEffectPrefab = BossDataManager.Instance.GetHowlEffectPrefab(bossId, configId);
            GameObject indicatorPrefab = BossDataManager.Instance.GetIndicatorPrefab(bossId, configId);
            GameObject areaExplosionPrefab = BossDataManager.Instance.GetSkillAreaPrefab(bossId, configId); 
            if (howlEffectPrefab == null)
            {
                Debug.LogWarning($"[SkillStrategyFactory] 보스 {bossId}의 스킬 {configId}에 하울 이펙트 프리팹이 없습니다.");
                return null;
            }

            // 필요한 값 계산
            float radius = bossData.howlRadius > 0 ? bossData.howlRadius : bossData.skillRange; // 기본값 처리
            float essenceAmount = bossData.EssenceAmount; // 에센스 증가량
            float duration = bossData.howlDuration; // 지속시간
            float damage = bossData.skillDamage * config.damageMultiplier; // 스킬 데미지에 계수 적용

            // HowlSkillEffect 생성
            HowlSkillEffect howlEffect = new HowlSkillEffect(
                howlEffectPrefab, // 하울링 프리팹
                areaExplosionPrefab, // 임팩트 프리팹
                bossData.howlSound, // 울음소리
                radius,
                essenceAmount,
                duration,
                damage,
                owner.transform,
                "여기임22222222222",
                indicatorPrefab
            );

            // Initialize 호출
            howlEffect.Initialize(
                owner.GetStatus(),
                owner.transform,
                config.damageMultiplier,
                config.speedMultiplier > 0 ? config.speedMultiplier : 1.0f
            );

            Debug.Log($"[SkillStrategyFactory] 보스 {bossId}의 Howl 스킬 {configId} 생성 완료");
            return howlEffect;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SkillStrategyFactory] HowlSkillEffect 생성 오류: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// 버프 스킬 데이터를 설정합니다.
    /// </summary>
    private static void SetupBuffData(SkillConfig config, BossData bossData)
    {
        // 커스텀 버프 데이터 생성
        BuffData buffData = new BuffData();

        // 버프 타입, 지속시간, 수치값 파싱 및 설정
        buffData.buffTypes = ParseBuffTypes(config.buffTypes);
        buffData.durations = ParseFloatArray(config.buffDurations);
        buffData.values = ParseFloatArray(config.buffValues);

        // 보스 데이터에 버프 데이터 업데이트
        bossData.buffData = buffData;

        Debug.Log($"[SkillStrategyFactory] 버프 데이터 설정 완료: 버프 타입 수 {buffData.buffTypes.Length}");
    }

    /// <summary>
    /// 버프 타입 문자열을 BuffType 배열로 변환합니다.
    /// </summary>
    private static BuffType[] ParseBuffTypes(string buffTypesString)
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

    /// <summary>
    /// 구분자로 나누어진 float 값 문자열을 float 배열로 변환합니다.
    /// </summary>
    private static float[] ParseFloatArray(string floatString)
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
}