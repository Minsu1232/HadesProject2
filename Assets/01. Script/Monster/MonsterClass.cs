using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackData;
using static MonsterData;

public abstract class MonsterClass : ICreature,IDamageable
{
    private MonsterData monsterData;
    
    public string MONSTERNAME { get; private set; }
    public int CurrentHealth { get; private set; }
    public int CurrentDeffense { get; private set; }
    public int CurrentAttackPower { get; private set; }
    public float CurrentAttackSpeed { get; private set; }
    public int CurrentSpeed { get; private set; }
    public float CurrentAttackRange {  get; private set; }  

    public int CurrentMoveRange {  get; private set; }
    public int CurrentChaseRange { get; private set; }

    public float CurrentSkillCooldown { get; private set; }

    public int CurrentAggroDropRange { get; private set; }

    public float CurrentSkillRange {  get; private set; }
    public float CurrentSKillDuration {  get; private set; }
    public float CurrentSkillDamage { get; private set; }

    public float CurrentHitStunDuration {  get; private set; }

    public float CurrentDeathDuration { get; private set; }

    public float CurrentSpawnDuration { get; private set; }

    public MonsterGrade grade { get; private set; }
    public SpawnStrategyType CurrentSpawnStrategy { get; private set; }
    public MovementStrategyType CurrentMoveStrategy { get; private set; }
    public AttackStrategyType CurrentAttackStrategy { get; private set; }
    public IdleStrategyType CurrentIdleStrategy { get; private set; }
    public SkillStrategyType CurrentSkillStrategy { get; private set; }
    public DieStrategyType CurrentDieStrategy { get; private set; }
    public bool isBasicAttack { get; private set; }

    protected bool isDashing = false;
   
    public MonsterClass(MonsterData data)
    {
        monsterData = data;
        InitializeStats();
    }
    public bool IsBasicAttack()
    {
        return isBasicAttack;
    }
    private void InitializeStats()
    {
        CurrentHealth = monsterData.initialHp;
        CurrentAttackPower = monsterData.initialAttackPower;
        CurrentAttackSpeed = monsterData.initialAttackSpeed;
        CurrentSpeed = monsterData.initialSpeed;
        MONSTERNAME = monsterData.monsterName;
        CurrentAttackRange = monsterData.attackRange;
        CurrentAttackSpeed = monsterData.initialAttackSpeed;
        CurrentMoveRange = monsterData.moveRange;
        CurrentChaseRange = monsterData.chaseRange;
        CurrentSkillCooldown = monsterData.skillCooldown;
        CurrentAggroDropRange = monsterData.aggroDropRange;
        CurrentSkillRange = monsterData.skillRange;
        CurrentSKillDuration = monsterData.skillDuration;
        CurrentHitStunDuration = monsterData.hitStunDuration;
        CurrentDeathDuration = monsterData.deathDuration;
        CurrentSpawnDuration = monsterData.spawnDuration;
        grade = monsterData.grade;
        CurrentSkillDamage = monsterData.skillDamage;
        CurrentSpawnStrategy = monsterData.spawnStrategy;
        CurrentMoveStrategy = monsterData.moveStrategy;
        CurrentAttackStrategy = monsterData.attackStrategy;
        CurrentIdleStrategy = monsterData.idleStrategy;
        CurrentSkillStrategy = monsterData.skillStrategy;
        CurrentDieStrategy = monsterData.dieStrategy;
    }
    public MonsterData GetMonsterData()
    {
        return monsterData;
    }
    public string GetName()
    {
        return monsterData.monsterName;
    }
    
    protected virtual void Debuff()
    {
        GameInitializer.Instance.GetPlayerClass();
    }
    protected void ModifyPower(int healthAmount = 0, int defenseAmount = 0, int attackAmount = 0, int attackSpeedAmount = 0, int speedAmount = 0)
    {
        CurrentHealth += healthAmount;
        CurrentDeffense += defenseAmount;
        CurrentAttackPower += attackAmount;
        CurrentAttackSpeed += attackSpeedAmount;
        CurrentSpeed += speedAmount;
    }
    public abstract void Attack();  // 공격 메서드 정의
    public virtual void TakeDamage(int damage, AttackType attackType)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
    }
    public virtual void TakeDotDamage(int dotDamage)
    {
        CurrentHealth -= dotDamage;
        Debug.Log($"지속 피해: {dotDamage}, 남은 체력: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
        // 지속 피해에는 애니메이션 트리거 없음
    }
    public abstract void Die();
 
    

    public virtual void SetPosition(Vector3 spawnPosition)
    {
        throw new NotImplementedException();
    }
}
