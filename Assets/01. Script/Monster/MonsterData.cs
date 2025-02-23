using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Sirenix.OdinInspector;

/// <summary>
/// 인터페이스 활용 및 데이터 저장/수정을위한 프로퍼티
/// </summary>
[CreateAssetMenu(fileName = "MonsterData", menuName = "Monster/MonsterData")]

public class MonsterData : ScriptableObject, ICreatureData 
{
    public enum MonsterGrade
    {
        Normal,     // 기본 몹
        Elite,      // 정예 몹
        MiniBoss,   // 중간보스
        Boss        // 최종보스
    }

    #region ICreatureData Implementation
    [FoldoutGroup("Basic Info"), ShowInInspector]
    public string MonsterName { get; set; }

    [FoldoutGroup("Basic Info"), ShowInInspector]
    public MonsterGrade Grade { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public int initialHp { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public int initialAttackPower { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public float initialAttackSpeed { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public int initialSpeed { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public int initialDeffense { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public float attackRange { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public float dropChance { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public int dropItem { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public int armorValue { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public float chargeSpeed { get; set; }
    [FoldoutGroup("Base Stats"), ShowInInspector]
    public float chargeDuration { get; set; }

    [FoldoutGroup("Movement Settings"), ShowInInspector]
    public int moveRange { get; set; }

    [FoldoutGroup("Movement Settings"), ShowInInspector]
    public int chaseRange { get; set; }

    [FoldoutGroup("Movement Settings"), ShowInInspector]
    public int aggroDropRange { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float skillCooldown { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float skillRange { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float skillDuration { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float skillDamage { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float projectileSpeed { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float rotateSpeed { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public string skillSpawnPointTag { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public GameObject areaEffectPrefab { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public GameObject shorckEffectPrefab { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float areaRadius { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float areaDuration { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public int multiShotCount { get; set; }  // 연속 발사 횟수

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float multiShotInterval { get; set; }  // 발사 간격

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public Vector3 projectileRotationAxis { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float projectileRotationSpeed { get; set; }
    [FoldoutGroup("Buff Settings"), ShowInInspector]
    public BuffData buffData { get; set; } = new BuffData();

    [FoldoutGroup("Buff Settings"), ShowInInspector]
    public BuffType buffType { get; set; }

    [FoldoutGroup("Buff Settings"), ShowInInspector]
    public float buffDuration { get; set; }

    [FoldoutGroup("Buff Settings"), ShowInInspector]
    public float buffValue { get; set; }

    [FoldoutGroup("Buff Settings"), ShowInInspector]
    public GameObject buffEffectPrefab { get; set; }

    [FoldoutGroup("Summon Settings"), ShowInInspector]
    public GameObject summonPrefab { get; set; }

    [FoldoutGroup("Summon Settings"), ShowInInspector]
    public int summonCount { get; set; }

    [FoldoutGroup("Summon Settings"), ShowInInspector]
    public float summonRadius { get; set; }

    [FoldoutGroup("State Durations"), ShowInInspector]
    public float hitStunDuration { get; set; }

    [FoldoutGroup("State Durations"), ShowInInspector]
    public float deathDuration { get; set; }

    [FoldoutGroup("State Durations"), ShowInInspector]
    public float spawnDuration { get; set; }

    [FoldoutGroup("Hit Settings"), ShowInInspector]
    public float superArmorThreshold { get; set; }

    [FoldoutGroup("Hit Settings"), ShowInInspector]
    public float hitStunMultiplier { get; set; }

    [FoldoutGroup("Hit Settings"), ShowInInspector]
    public float knockbackForce { get; set; }

    [FoldoutGroup("Hit Settings"), ShowInInspector]
    public float cameraShakeIntensity { get; set; }

    [FoldoutGroup("Hit Settings"), ShowInInspector]
    public float cameraShakeDuration { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public SpawnStrategyType spawnStrategy { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public MovementStrategyType moveStrategy { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public AttackStrategyType attackStrategy { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public IdleStrategyType idleStrategy { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public SkillStrategyType skillStrategy { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public DieStrategyType dieStrategy { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public HitStrategyType hitStrategy { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public ProjectileMovementType projectileType { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public ProjectileImpactType projectileImpactType { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public SkillEffectType skillEffectType { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public GroggyStrategyType groggyStrategy { get; set; }

    [FoldoutGroup("References"), ShowInInspector]
    public string monsterPrefabKey { get; set; }

    [FoldoutGroup("References"), ShowInInspector]
    public GameObject projectilePrefab { get; set; }

    [FoldoutGroup("References"), ShowInInspector]
    public GameObject hitEffect { get; set; }

    [FoldoutGroup("Behavior Conditions"), ShowInInspector]
    public bool useHealthRetreat { get; set; }

    [FoldoutGroup("Behavior Conditions"), ShowInInspector]
    public float healthRetreatThreshold { get; set; }

    [FoldoutGroup("Behavior Conditions"), ShowInInspector]
    public bool isPhaseChange { get; set; }

    [FoldoutGroup("Visual"), ShowInInspector]
    public Material eliteOutlineMaterial { get; set; }

    [FoldoutGroup("Visual"), ShowInInspector]
    public float shockwaveRadius { get; set; }

    [FoldoutGroup("Visual"), ShowInInspector]
    public float groggyTime { get; set; }

 }
    #endregion


public enum SpawnStrategyType
{
    Basic,
    Portal,    // 포탈에서 생성
    Summon     // 소환 효과로 생성
}

public enum MovementStrategyType
{
    Basic,
    Aggressive,    // 적극적 추적
    Defensive,     // 거리 유지
    Ranged,        // 원거리 유지
    Patrol,         // 정찰 패턴
    Retreat         // 도망 패턴
}

public enum AttackStrategyType
{
    Basic,
    Charge,
    Rush,
    Jump,
    Melee,         // 근접 공격
    Ranged,        // 원거리 공격
    AoE,           // 범위 공격
    Combo          // 연속 공격
}

public enum HitStrategyType
{
    Basic,
    Elite,
    MiniBoss,   // 중간보스
    Boss        // 최종보스
}

public enum IdleStrategyType
{
    Basic,
    Defensive,     // 방어 자세
    Alert,         // 경계 상태
    Passive        // 수동적
}

public enum SkillStrategyType
{
    Basic,
    Buff,          // 자신 강화
    Debuff,        // 적 약화
    MultiShot,  // n발 발사 
    Summon,        // 소환
    AreaControl    // 영역 제어
}

public enum DieStrategyType
{
    Basic,
    Explosion,     // 폭발하며 죽음
    Split,         // 분열하며 죽음
    Resurrection,  // 부활 가능
    DropItem       // 특별 아이템 드랍
}

public enum ProjectileMovementType
{
    None,
    Straight,
    StraightRotation,
    Homing,
    Parabolic    // 추가
}

public enum SkillEffectType
{
    None,
    Projectile,
    AreaEffect,
    Buff,
    Summon,
    // ... 다른 이펙트 타입들
}

public enum ProjectileImpactType
{
    None,
    Basic,
    Poison,
    //Explosion,
    //Freeze
    // 추가 가능
}

public enum GroggyStrategyType
{
    Basic,
    Poison,
    //Explosion,
    //Freeze
    // 추가 가능
}

public enum BuffType
{
    None,
    AttackUp,       // 공격력 증가
    DefenseUp,      // 방어력 증가
    SpeedUp,        // 이동속도 증가
    AttackSpeedUp,  // 공격속도 증가
    Heal,           // 체력 회복
    Rage,           // 분노(공격력 + 공격속도)
    Invincible,     // 무적
                    // 디버프
    AttackDown,     // 공격력 감소
    DefenseDown,    // 방어력 감소
    SpeedDown,      // 이동속도 감소
    Stun,           // 기절
    Poison,         // 독
    Burn,           // 화상
    Freeze          // 빙결
}

[System.Serializable]
public class BuffData
{
    public BuffType[] buffTypes;
    public float[] durations;
    public float[] values;
}