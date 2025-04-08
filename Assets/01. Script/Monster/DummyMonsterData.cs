using UnityEngine;
using System.Collections.Generic;

// 훈련용 더미 몬스터 데이터 클래스
public class DummyMonsterData : ICreatureData
{
    private int _maxHealth;

    public DummyMonsterData(int maxHealth)
    {
        _maxHealth = maxHealth;
    }

    // 기본 정보
    public string MonsterName => "TrainingDummy";
    public MonsterData.MonsterGrade Grade => MonsterData.MonsterGrade.Normal;
    public int MonsterID => 9999; // 특별 ID

    // 기본 스탯
    public int initialHp => _maxHealth;
    public int initialAttackPower => 0; // 공격력 없음
    public float initialAttackSpeed => 0;
    public int initialSpeed => 0;
    public int initialDeffense => 0;
    public float attackRange => 0;
    public float dropChance => 0;
    public int dropItem => 0;
    public int armorValue => 0;

    // 차지 관련
    public float chargeSpeed => 0;
    public float chargeDuration => 0;
    public float prepareTime => 0;

    // 이동 설정
    public int moveRange => 0;
    public int chaseRange => 0;
    public int aggroDropRange => 0;

    // 스킬 설정
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

    // 기타 속성
    public float heightFactor => 0;
    public Vector3 projectileRotationAxis => Vector3.zero;
    public float safeZoneRadius => 0;
    public float dangerRadiusMultiplier => 0;
    public GameObject ExplosionEffect => null;
    public float projectileRotationSpeed => 0;

    // 버프 설정
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

    // 멀티샷 설정
    public int multiShotCount => 0;
    public float multiShotInterval => 0;

    // 소환 설정
    public GameObject summonPrefab => null;
    public int summonCount => 0;
    public float summonRadius => 0;

    // 상태 지속시간
    public float hitStunDuration => 0.3f;
    public float deathDuration => 1f;
    public float spawnDuration => 0;

    // 하울 설정
    public GameObject howlEffectPrefab => null;
    public AudioClip howlSound => null;
    public float howlRadius => 0;
    public float howlDuration => 0;
    public float EssenceAmount => 0;

    // 히트 설정
    public float superArmorThreshold => 0;
    public float hitStunMultiplier => 0;
    public float knockbackForce => 0;
    public float cameraShakeIntensity => 0;
    public float cameraShakeDuration => 0;

    // 전략 타입
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

    // 참조
    public string monsterPrefabKey => "";
    public GameObject projectilePrefab => null;
    public GameObject hitEffect => null;

    // 행동 조건
    public bool useHealthRetreat => false;
    public float healthRetreatThreshold => 0;
    public bool isPhaseChange => false;

    // 시각적 요소
    public Material eliteOutlineMaterial => null;
    public float shockwaveRadius => 0;
    public float groggyTime => 0;
    public GameObject chargeIndicatorPrefab => null;
    public GameObject circleIndicatorPrefab => null;

    // 차지 공격 이펙트 관련 필드
    public GameObject ChargePrepareDustEffect => null;
    public GameObject ChargeStartEffect => null;
    public GameObject ChargeTrailEffect => null;
    public GameObject WallImpactEffect => null;
    public GameObject PlayerImpactEffect => null;
}