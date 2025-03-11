using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System;  // Enum Ÿ���� ����
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

    float heightFactor { get; } // ������ �̵� ���� ����
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

    // ���� ���� ����Ʈ ���� �ʵ��
    GameObject ChargePrepareDustEffect { get; }  // �غ� �ܰ� ���� ����Ʈ
    GameObject ChargeStartEffect { get; }        // ��¡ ���� ����Ʈ
    GameObject ChargeTrailEffect { get; }        // Ʈ���� ����Ʈ
    GameObject WallImpactEffect { get; }         // �� �浹 ����Ʈ
    GameObject PlayerImpactEffect { get; }       // �÷��̾� �浹 ����Ʈ
  
}