using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System;  // Enum 타입을 위해
public interface ICreatureData
{
    // Basic Info
    string MonsterName { get; }
    MonsterData.MonsterGrade Grade { get; }

    int MonsterID { get; }


    // Base Stats
    int initialHp { get; }
    int initialAttackPower { get; }
    float initialAttackSpeed { get; }
    int initialSpeed { get; }
    int initialDeffense { get; }
    float attackRange { get; }
    float dropChance { get; }
    int dropItem { get; }
    int armorValue { get; }

    float chargeSpeed {  get; }
    float chargeDuration { get; }

    float prepareTime { get; }
    // Movement Settings
    int moveRange { get; }
    int chaseRange { get; }
    int aggroDropRange { get; }

    // Skill Settings
    float skillCooldown { get; }
    float skillRange { get; }
    float skillDuration { get; }
    float skillDamage { get; }
    float projectileSpeed { get; }
    float rotateSpeed { get; }
    string skillSpawnPointTag { get; }
    GameObject areaEffectPrefab { get; }
    GameObject shorckEffectPrefab { get; }
    float areaRadius { get; }
    float areaDuration { get; }

    float heightFactor { get; } // 포물선 이동 높이 관련
    public Vector3 projectileRotationAxis { get; }
    float safeZoneRadius { get; }
    float dangerRadiusMultiplier { get; }
    GameObject ExplosionEffect { get; }
    public float projectileRotationSpeed { get; }
    // Buff Settings
    BuffData buffData { get; }
    BuffType buffType { get; }
    float buffDuration { get; }
    float buffValue { get; }
    GameObject buffEffectPrefab { get; }

    //multi Settings
    int multiShotCount { get; }
    float multiShotInterval {  get; }

    // Summon Settings
    GameObject summonPrefab { get; }
    int summonCount { get; }
    float summonRadius { get; }

    // State Durations
    float hitStunDuration { get; }
    float deathDuration { get; }
    float spawnDuration { get; }
    // Howl Settings


    GameObject howlEffectPrefab { get; }
    AudioClip howlSound { get; }
    float howlRadius { get; }
    float howlDuration { get; }
    float EssenceAmount { get; }


    // Hit Settings
    float superArmorThreshold { get; }
    float hitStunMultiplier { get; }
    float knockbackForce { get; }
    float cameraShakeIntensity { get; }
    float cameraShakeDuration { get; }

    // Strategies
    SpawnStrategyType spawnStrategy { get; }
    MovementStrategyType moveStrategy { get; }
    AttackStrategyType attackStrategy { get; }
    IdleStrategyType idleStrategy { get; }
    SkillStrategyType skillStrategy { get; }
    DieStrategyType dieStrategy { get; }
    HitStrategyType hitStrategy { get; }
    ProjectileMovementType projectileType { get; }
    ProjectileImpactType projectileImpactType { get; }
    SkillEffectType skillEffectType { get; }
    GroggyStrategyType groggyStrategy { get; }

    // Reference
    string monsterPrefabKey { get; }
    GameObject projectilePrefab { get; }
    GameObject hitEffect { get; }


    // Behavior Conditions
    bool useHealthRetreat { get; }
    float healthRetreatThreshold { get; }
    bool isPhaseChange { get; }

    // Visual
    Material eliteOutlineMaterial { get; }
    float shockwaveRadius { get; }
    float groggyTime { get; }
    //indicatorPrefab
    GameObject chargeIndicatorPrefab { get; }

    GameObject circleIndicatorPrefab { get; }

    // 차지 공격 이펙트 관련 필드들
    GameObject ChargePrepareDustEffect { get; }  // 준비 단계 먼지 이펙트
    GameObject ChargeStartEffect { get; }        // 차징 시작 이펙트
    GameObject ChargeTrailEffect { get; }        // 트레일 이펙트
    GameObject WallImpactEffect { get; }         // 벽 충돌 이펙트
    GameObject PlayerImpactEffect { get; }       // 플레이어 충돌 이펙트
  
}