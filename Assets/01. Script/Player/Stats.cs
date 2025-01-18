using Sirenix.OdinInspector;
using System;

public class Stats
{
    // 스탯 변경 이벤트들
    public event Action<int,int> OnHealthChanged;
    public event Action<int> OnManaChanged;
    public event Action<int> OnAttackPowerChanged;
    public event Action<int> OnAttackSpeedChanged;
    public event Action<float> OnSpeedChanged;
    public event Action<float> OnCriticalChanceChanged;
    public event Action<float> OndamageReceiveRateChanged;
    public event Action<int> OnMaxHealthChanged;

    // 스탯 프로퍼티들
    [ShowInInspector] private int health;
    public int Health
    {
        get => health;
        set
        {
            if (health != value)
            {
                health = value;
                OnHealthChanged?.Invoke(health,MaxHealth);
            }
        }
    }
    public int maxHealth;
    public int MaxHealth
    {
        get => maxHealth;
        set
        {
            if (maxHealth != value)
            {
                maxHealth = value;
                OnHealthChanged?.Invoke(Health, maxHealth);
            }
        }
    }
    [ShowInInspector] private int mana;
    public int Mana
    {
        get => mana;
        set
        {
            if (mana != value)
            {
                mana = value;
                OnManaChanged?.Invoke(mana);
            }
        }
    }

    [ShowInInspector] private int attackPower;
    public int AttackPower
    {
        get => attackPower;
        set
        {
            if (attackPower != value)
            {
                attackPower = value;
                OnAttackPowerChanged?.Invoke(attackPower);
            }
        }
    }

    [ShowInInspector] private int attackSpeed;
    public int AttackSpeed
    {
        get => attackSpeed;
        set
        {
            if (attackSpeed != value)
            {
                attackSpeed = value;
                OnAttackSpeedChanged?.Invoke(attackSpeed);
            }
        }
    }

    [ShowInInspector] private float speed;
    public float Speed
    {
        get => speed;
        set
        {
            if (speed != value)
            {
                speed = value;
                OnSpeedChanged?.Invoke(speed);
            }
        }
    }

    [ShowInInspector] private float criticalChance;
    public float CriticalChance
    {
        get => criticalChance;
        set
        {
            if (criticalChance != value)
            {
                criticalChance = value;
                OnCriticalChanceChanged?.Invoke(criticalChance);
            }
        }
    }

    [ShowInInspector] private float damageReceiveRate;
    public float DamageReceiveRate
    {
        get => damageReceiveRate;
        set
        {
            if (criticalChance != value)
            {
                damageReceiveRate = value;
                OndamageReceiveRateChanged?.Invoke(damageReceiveRate);
            }
        }
    }

    // 기본 스탯 저장을 위한 변수들
    private int defaultHealth;
    private int defaultHealthMax;
    private int defaultMana;
    private int defaultAttackPower;
    private int defaultAttackSpeed;
    private float defaultSpeed;
    private float defaultCriticalChance;
    private float defaultDamageReceiveRate;

    // 기본 스탯 저장 메서드
    public void SaveDefaultStats()
    {
        defaultHealth = Health;
        defaultHealthMax = Health ;
        defaultMana = Mana;
        defaultAttackPower = AttackPower;
        defaultAttackSpeed = AttackSpeed;
        defaultSpeed = Speed;
        defaultCriticalChance = CriticalChance;
        defaultDamageReceiveRate = DamageReceiveRate;
    }

    // 스탯 리셋 메서드
    public void ResetStats(
    bool resetHealth = false,
    bool resetMaxHealth = false,
    bool resetMana = false,
    bool resetAttackPower = false,
    bool resetAttackSpeed = false,
    bool resetSpeed = false,
    bool resetCriticalChance = false,
    bool resetDamageReceive = false)
    {
        if (resetHealth) Health = defaultHealth;
        if (resetMaxHealth) MaxHealth = defaultHealthMax;
        if (resetMana) Mana = defaultMana;
        if (resetAttackPower) AttackPower = defaultAttackPower;
        if (resetAttackSpeed) AttackSpeed = defaultAttackSpeed;
        if (resetSpeed) Speed = defaultSpeed;
        if (resetCriticalChance) CriticalChance = defaultCriticalChance;
        if (resetDamageReceive) DamageReceiveRate = defaultDamageReceiveRate;
    }


    public int MaxMana { get; private set; }
    // 다른 최대 값들도 필요한 경우 추가...

    // 생성자 추가
    public Stats(int baseHealth, int baseMana, int baseAttackPower, int baseAttackSpeed, float baseSpeed, float baseCriticalChance, float damageRecive)
    {
        MaxHealth = baseHealth;
        Health = baseHealth;

        MaxMana = baseMana;
        Mana = baseMana;

        AttackPower = baseAttackPower;
        AttackSpeed = baseAttackSpeed;
        Speed = baseSpeed;
        CriticalChance = baseCriticalChance;

        damageReceiveRate = damageRecive;


        // 기본 스탯 저장
        SaveDefaultStats();
    }
}
