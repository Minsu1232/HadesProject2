using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackData;
using static MonsterData;

public abstract class MonsterClass : ICreature,IMonsterClass
{
    // 아머 파괴 이벤트 (필요한 경우)
    public event Action OnArmorBreak;

    protected ICreatureData monsterData;

    protected PlayerClass playerClass;

    public event Action<int, int> OnHealthChanged; // (현재 체력, 최대 체력)

    public string MONSTERNAME { get; private set; }
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }
    public int CurrentDeffense { get; private set; }
    public int CurrentAttackPower { get; private set; }
    public float CurrentAttackSpeed { get; private set; }
    public int CurrentSpeed { get; private set; }
    public float CurrentAttackRange { get; private set; }

    public int CurrentMoveRange { get; private set; }
    public int CurrentChaseRange { get; private set; }

    public float CurrentSkillCooldown { get; private set; }

    public int CurrentAggroDropRange { get; private set; }

    public float CurrentSkillRange { get; private set; }
    public float CurrentSKillDuration { get; private set; }

    public float CurrentAreaDuration { get; private set; }
    public float CurrentSkillDamage { get; private set; }

    public float CurrentHitStunDuration { get; private set; }

    public float CurrentDeathDuration { get; private set; }

    public float CurrentSpawnDuration { get; private set; }

    public MonsterGrade grade { get; private set; }
    public SpawnStrategyType CurrentSpawnStrategy { get; private set; }
    public MovementStrategyType CurrentMoveStrategy { get; private set; }
    public AttackStrategyType CurrentAttackStrategy { get; private set; }
    public IdleStrategyType CurrentIdleStrategy { get; private set; }
    public SkillStrategyType CurrentSkillStrategy { get; private set; }
    public DieStrategyType CurrentDieStrategy { get; private set; }

    public bool UseHealthRetreat { get; private set; }           // 체력기반 도주 사용
    public float HealthRetreatThreshold { get; private set; }    // 도주 시작 체력 비율


    public bool IsPhaseChange { get; private set; }             // 페이즈 전환용 도주인지
    public float CurrentProjectileSpeed { get; private set; }
    public float CurrentRotateSpeed { get; private set; }
    public float CurrentAreaRadius { get; private set; }
    public BuffType CurrentBuffType { get; private set; }
    public float CurrentBuffDuration { get; private set; }
    public float CurrentBuffValue { get; private set; }
    public int CurrentSummonCount { get; private set; }
    public float CurrentSummonRadius { get; private set; }

    public float CurrentSuperArmorThreshold { get; private set; }
    public float CurrentHitStunMultplier { get; private set; }
    public float CurrentKnockbackForce { get; private set; }
    public float CurrentCameraShakeIntensity { get; private set; }
    public float CurrentCameraShakeDuration { get; private set; }
    public ProjectileMovementType CurrentProjectileType { get; private set; }
    public ProjectileImpactType CurrentProjectileImpactType { get; private set; }
    public SkillEffectType CurrentSkillEffectType { get; private set; }
    public bool isBasicAttack { get; private set; }
    public int CurrentArmor { get; private set; }

    public float CurrentShockwaveRadius {  get; private set; }

    protected bool isDashing = false;
    public GameObject hitEffect;
    public MonsterClass(ICreatureData data)
    {
        monsterData = data;

        // 체력 초기화
        MaxHealth = data.initialHp;
        CurrentHealth = data.initialHp;

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth); // 초기화 시 이벤트 호출
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
        MONSTERNAME = monsterData.MonsterName;
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
        grade = monsterData.Grade;
        CurrentSkillDamage = monsterData.skillDamage;
        CurrentSpawnStrategy = monsterData.spawnStrategy;
        CurrentMoveStrategy = monsterData.moveStrategy;
        CurrentAttackStrategy = monsterData.attackStrategy;
        CurrentIdleStrategy = monsterData.idleStrategy;
        CurrentSkillStrategy = monsterData.skillStrategy;
        CurrentDieStrategy = monsterData.dieStrategy;
        UseHealthRetreat = monsterData.useHealthRetreat;
        HealthRetreatThreshold = monsterData.healthRetreatThreshold;
        IsPhaseChange = monsterData.isPhaseChange;

        CurrentProjectileSpeed = monsterData.projectileSpeed;
        CurrentRotateSpeed = monsterData.rotateSpeed;
        CurrentAreaRadius = monsterData.areaRadius;
        CurrentBuffType = monsterData.buffType;
        CurrentBuffDuration = monsterData.buffDuration;
        CurrentBuffValue = monsterData.buffValue;
        CurrentSummonCount = monsterData.summonCount;
        CurrentSummonRadius = monsterData.summonRadius;
        CurrentProjectileType = monsterData.projectileType;
        CurrentSkillEffectType = monsterData.skillEffectType;
        CurrentProjectileImpactType = monsterData.projectileImpactType;
        CurrentAreaDuration = monsterData.areaDuration;
        hitEffect = monsterData.hitEffect;
        CurrentSuperArmorThreshold = monsterData.superArmorThreshold;
        CurrentHitStunMultplier = monsterData.hitStunMultiplier;
        CurrentKnockbackForce = monsterData.knockbackForce;
        CurrentCameraShakeIntensity = monsterData.cameraShakeIntensity;
        CurrentCameraShakeDuration = monsterData.cameraShakeDuration;        
        CurrentDeffense = monsterData.initialDeffense;
        CurrentShockwaveRadius = monsterData.shockwaveRadius;

        playerClass = GameInitializer.Instance.GetPlayerClass();

        Debug.Log(CurrentCameraShakeIntensity);
    }
    public ICreatureData GetMonsterData()
    {
        return monsterData;
    }
    public string GetName()
    {
        return monsterData.MonsterName;
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
    public virtual void Attack()
    {

    }  // 공격 메서드 정의
    public virtual void TakeDamage(int damage)
    {
        // 기본 방어력 계산
        float damageTakenMultiplier = CurrentDeffense >= 0
            ? Math.Max(0.1f, 1f - (CurrentDeffense / 1000f))
            : 1f + (Math.Abs(CurrentDeffense) / 1000f);

        int incomingDamage = Mathf.Max(1, (int)(damage * damageTakenMultiplier));

        // 아머 처리
        if (CurrentArmor > 0)
        {
            // 아머에 흡수되는 데미지 계산
            int armorDamage = Mathf.Min(CurrentArmor, incomingDamage);
            CurrentArmor -= armorDamage;

            // 아머를 뚫고 들어가는 남은 데미지 계산
            incomingDamage -= armorDamage;

            // 아머가 파괴되었을 때 이벤트 발생
            if (CurrentArmor == 0)
            {
                OnArmorBreak?.Invoke();
            }
        }

        // 남은 데미지로 체력 감소
        if (incomingDamage > 0)
        {
            CurrentHealth -= incomingDamage;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Die();
            }
        }

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
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
    public virtual void Die()
    {

    }





    public Vector3 GetPlayerPosition()
    {
        return playerClass.playerTransform.position;
    }

   public void ModifyStats(
          int healthAmount = 0,
    int maxHealthAmount = 0, // 최대 체력 변경 추가
    int defenseAmount = 0,
    int attackAmount = 0,
    float attackSpeedAmount = 0,
    int speedAmount = 0,
    float skillCooldownAmount = 0,
    float areaRadiusAmount = 0,
    float buffValueAmount = 0,
    float skillRangeAmount = 0,
    float attackRangeAmount = 0,
    int armorAmount = 0)
    {
        // 현재 체력 비율 저장
        float healthRatio = (float)CurrentHealth / MaxHealth;

        // 최대 체력 변경
        if (maxHealthAmount != 0)
        {
            MaxHealth += maxHealthAmount;

            // 기존 비율에 따라 현재 체력 보정
            CurrentHealth = Mathf.Clamp((int)(MaxHealth * healthRatio), 0, MaxHealth);
        }

        // 현재 체력 변경
        if (healthAmount != 0)
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth + healthAmount, 0, MaxHealth);
        }

        // 다른 스탯 변경
        CurrentDeffense += defenseAmount;
        CurrentAttackPower += attackAmount;
        CurrentAttackSpeed += attackSpeedAmount;
        CurrentSpeed += speedAmount;
        CurrentSkillCooldown += skillCooldownAmount;
        CurrentAreaRadius += areaRadiusAmount;
        CurrentBuffValue += buffValueAmount;
        CurrentSkillRange += skillRangeAmount;
        CurrentAttackRange += attackRangeAmount;
        CurrentArmor += armorAmount;

        // 제한 조건
        CurrentAttackSpeed = Mathf.Max(0.1f, CurrentAttackSpeed);
        CurrentSpeed = Mathf.Max(0, CurrentSpeed);

        // 체력 변화 이벤트 호출
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}
