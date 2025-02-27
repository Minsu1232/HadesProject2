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

            // 커스텀 SkillEffect 생성 - 중요 수정: 직접 ProjectileSkillEffect 생성
            ISkillEffect skillEffect = null;


                // 다른 이펙트 타입은 기존 StrategyFactory 활용
                skillEffect = StrategyFactory.CreateSkillEffect(config.effectType, data, owner);
            

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
            if (config.strategyType == SkillStrategyType.Buff && data is BossData bossData)
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