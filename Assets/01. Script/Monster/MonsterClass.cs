using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackData;
using static MonsterData;

public abstract class MonsterClass : ICreature,IMonsterClass, IDamageable
{    // ����Ʈ ���� ����� ������
    protected int defaultMaxHealth;
    protected int defaultCurrentHealth;
    protected int defaultDefense;
    protected int defaultAttackPower;
    protected float defaultAttackSpeed;
    protected int defaultSpeed;
    protected float defaultSkillCooldown;
    protected float defaultAreaRadius;
    protected float defaultBuffValue;
    protected float defaultSkillRange;
    protected float defaultAttackRange;
    protected int defaultArmor;

    private int _lastAppliedDamage = 0;
    public int LastAppliedDamage => _lastAppliedDamage;
    protected virtual void SaveDefaultStats()
    {
        defaultMaxHealth = MaxHealth;
        defaultCurrentHealth = CurrentHealth;
        defaultDefense = CurrentDeffense;
        defaultAttackPower = CurrentAttackPower;
        defaultAttackSpeed = CurrentAttackSpeed;
        defaultSpeed = CurrentSpeed;
        defaultSkillCooldown = CurrentSkillCooldown;
        defaultAreaRadius = CurrentAreaRadius;
        defaultBuffValue = CurrentBuffValue;
        defaultSkillRange = CurrentSkillRange;
        defaultAttackRange = CurrentAttackRange;
        defaultArmor = CurrentArmor;
    }

    public virtual void ResetToDefault()
    {
        float healthRatio = (float)CurrentHealth / MaxHealth;

        MaxHealth = defaultMaxHealth;
        CurrentHealth = Mathf.Clamp((int)(MaxHealth * healthRatio), 0, MaxHealth);
        CurrentDeffense = defaultDefense;
        CurrentAttackPower = defaultAttackPower;
        CurrentAttackSpeed = defaultAttackSpeed;
        CurrentSpeed = defaultSpeed;
        CurrentSkillCooldown = defaultSkillCooldown;
        CurrentAreaRadius = defaultAreaRadius;
        CurrentBuffValue = defaultBuffValue;
        CurrentSkillRange = defaultSkillRange;
        CurrentAttackRange = defaultAttackRange;
        CurrentArmor = defaultArmor;

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }


    // �Ƹ� �ı� �̺�Ʈ (�ʿ��� ���)
    public event Action OnArmorBreak;

    protected ICreatureData monsterData;

    protected PlayerClass playerClass;

    public event Action<int, int> OnHealthChanged; // (���� ü��, �ִ� ü��)

    public string MONSTERNAME { get; private set; }

    private int _currentHealth;
    public int CurrentHealth
    {
        get => _currentHealth;
        protected set => _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
    }

    private int _maxHealth;
    public int MaxHealth
    {
        get => _maxHealth;
        private set => _maxHealth = Mathf.Max(value, 0);
    }

    private int _currentDefense;
    public int CurrentDeffense
    {
        get => _currentDefense;
        private set => _currentDefense = Mathf.Max(value, 0);
    }

    private int _currentAttackPower;
    public int CurrentAttackPower
    {
        get => _currentAttackPower;
        private set => _currentAttackPower = Mathf.Max(value, 0);
    }

    private float _currentAttackSpeed;
    public float CurrentAttackSpeed
    {
        get => _currentAttackSpeed;
        private set => _currentAttackSpeed = Mathf.Max(value, 0.1f);
    }

    private int _currentSpeed;
    public int CurrentSpeed
    {
        get => _currentSpeed;
        private set => _currentSpeed = Mathf.Max(value, 0);
    }

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

    public bool IsAlive {  get; private set; }
    public MonsterGrade grade { get; private set; }
    public SpawnStrategyType CurrentSpawnStrategy { get; private set; }
    public MovementStrategyType CurrentMoveStrategy { get; private set; }
    public AttackStrategyType CurrentAttackStrategy { get; private set; }
    public IdleStrategyType CurrentIdleStrategy { get; private set; }
    public SkillStrategyType CurrentSkillStrategy { get; private set; }
    public DieStrategyType CurrentDieStrategy { get; private set; }

    public bool UseHealthRetreat { get; private set; }
    public float HealthRetreatThreshold { get; private set; }

    public bool IsPhaseChange { get; private set; }
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

    private int _currentArmor;
    public int CurrentArmor
    {
        get => _currentArmor;
        private set => _currentArmor = Mathf.Max(value, 0);
    }

    public float CurrentShockwaveRadius { get; private set; }

    public int CurrentMultiShotCount { get; private set; }
     public float CurrentMultiShotInterval { get; private set; }

    protected bool isDashing = false;
    public GameObject hitEffect;
    public MonsterClass(ICreatureData data)
    {
       
        monsterData = data;

        // ü�� �ʱ�ȭ
        MaxHealth = data.initialHp;
        CurrentHealth = data.initialHp;

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth); // �ʱ�ȭ �� �̺�Ʈ ȣ��
        InitializeStats();
        SaveDefaultStats();

        Debug.Log(MONSTERNAME);
    }
    public bool IsBasicAttack()
    {
        return isBasicAttack;
    }
    protected virtual void InitializeStats()
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
        CurrentMultiShotCount = monsterData.multiShotCount;
        CurrentMultiShotInterval = monsterData.multiShotInterval;
        IsAlive = true;
        playerClass = GameInitializer.Instance.GetPlayerClass();

       
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

    }  // ���� �޼��� ����
    public virtual void TakeDamage(int damage)
    {
        // �⺻ ���� ���
        float damageTakenMultiplier = CurrentDeffense >= 0
            ? Math.Max(0.1f, 1f - (CurrentDeffense / 1000f))
            : 1f + (Math.Abs(CurrentDeffense) / 1000f);

        int incomingDamage = Mathf.Max(1, (int)(damage * damageTakenMultiplier));

        // ������ ���� ������ �ʱ�ȭ
        _lastAppliedDamage = incomingDamage;

        // �Ƹ� ó��
        if (CurrentArmor > 0)
        {
            // �Ƹӿ� ����Ǵ� ������ ���
            int armorDamage = Mathf.Min(CurrentArmor, incomingDamage);
            CurrentArmor -= armorDamage;

            // �ƸӸ� �հ� ���� ���� ������ ���
            _lastAppliedDamage -= armorDamage;

            // �ƸӰ� �ı��Ǿ��� �� �̺�Ʈ �߻�
            if (CurrentArmor == 0)
            {
                OnArmorBreak?.Invoke();
            }
        }

        // ���� �������� ü�� ����
        if (_lastAppliedDamage > 0)
        {
            int previousHealth = CurrentHealth;
            CurrentHealth -= _lastAppliedDamage;

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
        Debug.Log($"���� ����: {dotDamage}, ���� ü��: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
        // ���� ���ؿ��� �ִϸ��̼� Ʈ���� ����
    }
    public virtual void Die()
    {
        Debug.Log("�׾����ϴ� ���Ͱ�@@@@@@@@@@@@@@@@@@@@@@@@!!!!!!!!");
        IsAlive = false;
    }



    public void SetArmorValue(int value)
    {
        CurrentArmor = value;
    }
    public Vector3 GetPlayerPosition()
    {
        return playerClass.playerTransform.position;
    }

   public void ModifyStats(
          int healthAmount = 0,
    int maxHealthAmount = 0, // �ִ� ü�� ���� �߰�
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
        // ���� ü�� ���� ����
        float healthRatio = (float)CurrentHealth / MaxHealth;

        // �ִ� ü�� ����
        if (maxHealthAmount != 0)
        {
            MaxHealth += maxHealthAmount;

            // ���� ������ ���� ���� ü�� ����
            CurrentHealth = Mathf.Clamp((int)(MaxHealth * healthRatio), 0, MaxHealth);
        }

        // ���� ü�� ����
        if (healthAmount != 0)
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth + healthAmount, 0, MaxHealth);
        }

        // �ٸ� ���� ����
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

        // ���� ����
        CurrentAttackSpeed = Mathf.Max(0.1f, CurrentAttackSpeed);
        CurrentSpeed = Mathf.Max(0, CurrentSpeed);

        // ü�� ��ȭ �̺�Ʈ ȣ��
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public virtual DamageType GetDamageType()
    {
        Debug.Log("����������Ÿ��ȣ��");
        return DamageType.Monster;
    }
}
