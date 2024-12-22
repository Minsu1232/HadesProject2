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

    [Header("Movement Settings")]
    public int moveRange;    // 기본 배회 범위
    public int chaseRange;   // 추적 감지 범위
    public int aggroDropRange;  // 어그로가 풀리는 범위 (chaseRange보다 큰 값)

    [Header("Skill Settings")]
    public float skillCooldown;   // 스킬 쿨타임
    public float skillRange;      // 스킬 사용 가능 범위
    public float skillDuration;   // 스킬 시전까지의 시간
    public float skillDamage;     // 스킬 데미지

    [Header("State Durations")]
    public float hitStunDuration; // 피격시 경직 시간
    public float deathDuration;   // 사망 연출 시간
    public float spawnDuration;   // 스폰 연출 시간

    [Header("AI Strategies")]
    public SpawnStrategyType spawnStrategy = SpawnStrategyType.Basic;
    public MovementStrategyType moveStrategy = MovementStrategyType.Basic;
    public AttackStrategyType attackStrategy = AttackStrategyType.Basic;
    public IdleStrategyType idleStrategy = IdleStrategyType.Basic;
    public SkillStrategyType skillStrategy = SkillStrategyType.Basic;
    public DieStrategyType dieStrategy = DieStrategyType.Basic;
    public HitStrategyType hitStrategy = HitStrategyType.Basic;
    [Header("Reference")]
    public string monsterPrefabKey; // Prefab Addressables Key
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
    Patrol         // 정찰 패턴
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

