using UnityEngine;
using System.Collections.Generic;

// �Ʒÿ� ���� ���� ������ Ŭ����
public class DummyMonsterData : ICreatureData
{
    private int _maxHealth;

    public DummyMonsterData(int maxHealth)
    {
        _maxHealth = maxHealth;
    }

    // �⺻ ����
    public string MonsterName => "TrainingDummy";
    public MonsterData.MonsterGrade Grade => MonsterData.MonsterGrade.Normal;
    public int MonsterID => 9999; // Ư�� ID

    // �⺻ ����
    public int initialHp => _maxHealth;
    public int initialAttackPower => 0; // ���ݷ� ����
    public float initialAttackSpeed => 0;
    public int initialSpeed => 0;
    public int initialDeffense => 0;
    public float attackRange => 0;
    public float dropChance => 0;
    public int dropItem => 0;
    public int armorValue => 0;

    // ���� ����
    public float chargeSpeed => 0;
    public float chargeDuration => 0;
    public float prepareTime => 0;

    // �̵� ����
    public int moveRange => 0;
    public int chaseRange => 0;
    public int aggroDropRange => 0;

    // ��ų ����
    public float skillCooldown => 0;
    public float skillRange => 0;
    public float skillDuration => 0;
    public float skillDamage => 0;
    public float projectileSpeed => 0;
    public float rotateSpeed => 0;
    public string skillSpawnPointTag => "";
    public GameObject areaEffectPrefab => null;
    public GameObject shorckEffectPrefab => null;
    public float areaRadius => 0;
    public float areaDuration => 0;

    // ��Ÿ �Ӽ�
    public float heightFactor => 0;
    public Vector3 projectileRotationAxis => Vector3.zero;
    public float safeZoneRadius => 0;
    public float dangerRadiusMultiplier => 0;
    public GameObject ExplosionEffect => null;
    public float projectileRotationSpeed => 0;

    // ���� ����
    public BuffData buffData => new BuffData
    {
        buffTypes = new BuffType[0],
        durations = new float[0],
        values = new float[0]
    };
    public BuffType buffType => BuffType.None;
    public float buffDuration => 0;
    public float buffValue => 0;
    public GameObject buffEffectPrefab => null;

    // ��Ƽ�� ����
    public int multiShotCount => 0;
    public float multiShotInterval => 0;

    // ��ȯ ����
    public GameObject summonPrefab => null;
    public int summonCount => 0;
    public float summonRadius => 0;

    // ���� ���ӽð�
    public float hitStunDuration => 0.3f;
    public float deathDuration => 1f;
    public float spawnDuration => 0;

    // �Ͽ� ����
    public GameObject howlEffectPrefab => null;
    public AudioClip howlSound => null;
    public float howlRadius => 0;
    public float howlDuration => 0;
    public float EssenceAmount => 0;

    // ��Ʈ ����
    public float superArmorThreshold => 0;
    public float hitStunMultiplier => 0;
    public float knockbackForce => 0;
    public float cameraShakeIntensity => 0;
    public float cameraShakeDuration => 0;

    // ���� Ÿ��
    public SpawnStrategyType spawnStrategy => SpawnStrategyType.Basic;
    public MovementStrategyType moveStrategy => MovementStrategyType.None;
    public AttackStrategyType attackStrategy => AttackStrategyType.None;
    public IdleStrategyType idleStrategy => IdleStrategyType.Basic;
    public SkillStrategyType skillStrategy => SkillStrategyType.None;
    public DieStrategyType dieStrategy => DieStrategyType.Basic;
    public HitStrategyType hitStrategy => HitStrategyType.Basic;
    public ProjectileMovementType projectileType => ProjectileMovementType.None;
    public ProjectileImpactType projectileImpactType => ProjectileImpactType.None;
    public SkillEffectType skillEffectType => SkillEffectType.None;
    public GroggyStrategyType groggyStrategy => GroggyStrategyType.None;

    // ����
    public string monsterPrefabKey => "";
    public GameObject projectilePrefab => null;
    public GameObject hitEffect => null;

    // �ൿ ����
    public bool useHealthRetreat => false;
    public float healthRetreatThreshold => 0;
    public bool isPhaseChange => false;

    // �ð��� ���
    public Material eliteOutlineMaterial => null;
    public float shockwaveRadius => 0;
    public float groggyTime => 0;
    public GameObject chargeIndicatorPrefab => null;
    public GameObject circleIndicatorPrefab => null;

    // ���� ���� ����Ʈ ���� �ʵ�
    public GameObject ChargePrepareDustEffect => null;
    public GameObject ChargeStartEffect => null;
    public GameObject ChargeTrailEffect => null;
    public GameObject WallImpactEffect => null;
    public GameObject PlayerImpactEffect => null;
}