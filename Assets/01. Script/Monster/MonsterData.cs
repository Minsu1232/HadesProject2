using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Monster")]

public class MonsterData : ScriptableObject
{
    public enum MonsterGrade
    {
        Normal,     // 기본 몹
        Elite,      // 정예 몹
        MiniBoss,   // 중간보스
        Boss        // 최종보스
    }

    [Header("Basic Info")]
    public string monsterName;
    public MonsterGrade grade;

    [Header("Base Stats")]
    public int initialHp;
    public int initialAttackPower;
    public float initialAttackSpeed;
    public int initialSpeed;
    public float attackRange;
    public float dropChance;
    public int dropItem;
    public int armorValue;

    [Header("Movement Settings")]
    public int moveRange;    // 기본 배회 범위
    public int chaseRange;   // 추적 감지 범위
    public int aggroDropRange;  // 어그로가 풀리는 범위 (chaseRange보다 큰 값)

    [Header("Skill Settings")]
    public float skillCooldown;   // 스킬 쿨타임
    public float skillRange;      // 스킬 사용 가능 범위
    public float skillDuration;   // 스킬 시전까지의 시간
    public float skillDamage;     // 스킬 데미지
    public float projectileSpeed; // 프리팹 이동 속도
    public float rotateSpeed; // 호핑타입 회전 속도
    public string skillSpawnPointTag = "SkillSpawnPoint";  // 프리팹에서 찾을 스킬 발사 위치의 태그
    // 범위 스킬용 데이터
    public GameObject areaEffectPrefab;
    public float areaRadius;

    // 0106 csv에 추가해야할사항
    public float areaDuration = 5f;

    // 버프 스킬용 데이터
    public BuffType buffType;
    public float buffDuration;
    public float buffValue;

    // 소환 스킬용 데이터
    public GameObject summonPrefab;
    public int summonCount;
    public float summonRadius;
    [Header("State Durations")]

    public float hitStunDuration; // 피격시 경직 시간
    public float deathDuration;   // 사망 연출 시간
    public float spawnDuration;   // 스폰 연출 시간
    [Header("Hit Settings")]

    public float superArmorThreshold;    // 슈퍼아머 발동 기준 데미지
    public float hitStunMultiplier = 1f; // 경직 시간 배율 (기본:1, 엘리트:0.5, 미니보스:0.35, 보스:0.25)
    public float knockbackForce;         // 넉백 강도
    public float cameraShakeIntensity;   // 카메라 흔들림 강도
    public float cameraShakeDuration;    // 카메라 흔들림 지속시간

    [Header("AI Strategies")]
    public SpawnStrategyType spawnStrategy = SpawnStrategyType.Basic;
    public MovementStrategyType moveStrategy = MovementStrategyType.Basic;
    public AttackStrategyType attackStrategy = AttackStrategyType.Basic;
    public IdleStrategyType idleStrategy = IdleStrategyType.Basic;
    public SkillStrategyType skillStrategy = SkillStrategyType.Basic;
    public DieStrategyType dieStrategy = DieStrategyType.Basic;
    public HitStrategyType hitStrategy = HitStrategyType.Basic;
    public ProjectileMovementType projectileType = ProjectileMovementType.Straight;
    public ProjectileImpactType projectileImpactType = ProjectileImpactType.Basic;
    public SkillEffectType skillEffectType;

    [Header("Reference")]
    public string monsterPrefabKey; // Prefab Addressables Key
    public GameObject projectilePrefab;
    public GameObject hitEffect;
    

    [Header("Behavior Conditions")]
    public bool useHealthRetreat;           // 체력기반 도주 사용
    public float healthRetreatThreshold;    // 도주 시작 체력 비율
    public bool isPhaseChange;             // 페이즈 전환용 도주인지

}

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
    Straight,
    Homing,
    Parabolic    // 추가
}
public enum SkillEffectType
{
    Projectile,
    AreaEffect,
    Buff,
    Summon,
    // ... 다른 이펙트 타입들
}
public enum ProjectileImpactType
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

