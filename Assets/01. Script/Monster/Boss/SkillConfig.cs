using UnityEngine;

/// <summary>
/// 스킬 구성을 저장하는 클래스
/// BossSkillConfigs.csv에서 로드된 정보를 저장합니다.
/// </summary>
[System.Serializable]
public class SkillConfig
{
    public int configId;                    // 구성 ID
    public SkillStrategyType strategyType;  // 스킬 전략 타입
    public SkillEffectType effectType;      // 스킬 이펙트 타입
    public ProjectileMovementType moveType; // 발사체 움직임 타입
    public ProjectileImpactType impactType; // 발사체 충돌 이펙트 타입
    public string configName;               // 구성 이름
    public float damageMultiplier = 1.0f; // 기본값은 1.0
    // 버프 관련 정보 (버프 스킬일 경우만 사용)
    public string buffTypes;                // 버프 타입 (파이프 구분자로 여러 개 가능)
    public string buffDurations;            // 버프 지속시간 (파이프 구분자로 여러 개 가능)
    public string buffValues;               // 버프 수치값 (파이프 구분자로 여러 개 가능)

    public SkillConfig(int id, string name, SkillStrategyType strategy, SkillEffectType effect,
                      ProjectileMovementType move, ProjectileImpactType impact,
                      string buffs = "", string durations = "", string values = "")
    {
        configId = id;
        configName = name;
        strategyType = strategy;
        effectType = effect;
        moveType = move;
        impactType = impact;
        buffTypes = buffs;
        buffDurations = durations;
        buffValues = values;
    }
}