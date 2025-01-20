using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MonsterData;

public interface IMonsterClass
{
    public Vector3 GetPlayerPosition();
    public event Action OnArmorBreak;
    ICreatureData GetMonsterData();  // MonsterData 대신 인터페이스 반환
    string MONSTERNAME { get; }
    int CurrentHealth { get; }
    int MaxHealth { get; }
    int CurrentDeffense { get; }
    int CurrentAttackPower { get; }
    float CurrentAttackSpeed { get; }
    int CurrentSpeed { get; }
    float CurrentAttackRange { get; }
    int CurrentMoveRange { get; }
    int CurrentChaseRange { get; }
    float CurrentSkillCooldown { get; }
    int CurrentAggroDropRange { get; }
    float CurrentSkillRange { get; }
    float CurrentSKillDuration { get; }
    float CurrentAreaDuration { get; }
    float CurrentSkillDamage { get; }
    float CurrentHitStunDuration { get; }
    float CurrentDeathDuration { get; }
    float CurrentSpawnDuration { get; }

    MonsterGrade grade { get; }
    SpawnStrategyType CurrentSpawnStrategy { get; }
    MovementStrategyType CurrentMoveStrategy { get; }
    AttackStrategyType CurrentAttackStrategy { get; }
    IdleStrategyType CurrentIdleStrategy { get; }
    SkillStrategyType CurrentSkillStrategy { get; }
    DieStrategyType CurrentDieStrategy { get; }

    bool UseHealthRetreat { get; }
    float HealthRetreatThreshold { get; }
    bool IsPhaseChange { get; }
    float CurrentProjectileSpeed { get; }
    float CurrentRotateSpeed { get; }
    float CurrentAreaRadius { get; }
    BuffType CurrentBuffType { get; }
    float CurrentBuffDuration { get; }
    float CurrentBuffValue { get; }
    int CurrentSummonCount { get; }
    float CurrentSummonRadius { get; }

    float CurrentSuperArmorThreshold { get; }
    float CurrentHitStunMultplier { get; }
    float CurrentKnockbackForce { get; }
    float CurrentCameraShakeIntensity { get; }
    float CurrentCameraShakeDuration { get; }
    ProjectileMovementType CurrentProjectileType { get; }
    ProjectileImpactType CurrentProjectileImpactType { get; }
    SkillEffectType CurrentSkillEffectType { get; }
    bool isBasicAttack { get; }
    int CurrentArmor { get; }
    float CurrentShockwaveRadius { get; }

    void TakeDamage(int damage);

    void ModifyStats(
        int healthAmount = 0,
        int maxHealthAmount = 0,
        int defenseAmount = 0,
        int attackAmount = 0,
        float attackSpeedAmount = 0,
        int speedAmount = 0,
        float skillCooldownAmount = 0,
        float areaRadiusAmount = 0,
        float buffValueAmount = 0,
        float skillRangeAmount = 0,
        float attackRangeAmount = 0,
        int armorAmount = 0);
}
