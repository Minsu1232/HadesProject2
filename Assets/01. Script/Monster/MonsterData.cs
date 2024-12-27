using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Monster")]

public class MonsterData : ScriptableObject
{
    public enum MonsterGrade
    {
        Normal,     // �⺻ ��
        Elite,      // ���� ��
        MiniBoss,   // �߰�����
        Boss        // ��������
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
    public int moveRange;    // �⺻ ��ȸ ����
    public int chaseRange;   // ���� ���� ����
    public int aggroDropRange;  // ��׷ΰ� Ǯ���� ���� (chaseRange���� ū ��)

    [Header("Skill Settings")]
    public float skillCooldown;   // ��ų ��Ÿ��
    public float skillRange;      // ��ų ��� ���� ����
    public float skillDuration;   // ��ų ���������� �ð�
    public float skillDamage;     // ��ų ������
    public float projectileSpeed; // ������ �̵� �ӵ�
    public float rotateSpeed; // ȣ��Ÿ�� ȸ�� �ӵ�
    public string skillSpawnPointTag = "SkillSpawnPoint";  // �����տ��� ã�� ��ų �߻� ��ġ�� �±�
    // ���� ��ų�� ������
    public GameObject areaEffectPrefab;
    public float areaRadius;
    

    // ���� ��ų�� ������
    public BuffType buffType;
    public float buffDuration;
    public float buffValue;

    // ��ȯ ��ų�� ������
    public GameObject summonPrefab;
    public int summonCount;
    public float summonRadius;
    [Header("State Durations")]

    public float hitStunDuration; // �ǰݽ� ���� �ð�
    public float deathDuration;   // ��� ���� �ð�
    public float spawnDuration;   // ���� ���� �ð�

    [Header("AI Strategies")]
    public SpawnStrategyType spawnStrategy = SpawnStrategyType.Basic;
    public MovementStrategyType moveStrategy = MovementStrategyType.Basic;
    public AttackStrategyType attackStrategy = AttackStrategyType.Basic;
    public IdleStrategyType idleStrategy = IdleStrategyType.Basic;
    public SkillStrategyType skillStrategy = SkillStrategyType.Basic;
    public DieStrategyType dieStrategy = DieStrategyType.Basic;
    public HitStrategyType hitStrategy = HitStrategyType.Basic;
    public ProjectileMovementType projectileType = ProjectileMovementType.Straight;
    public SkillEffectType skillEffectType;
    [Header("Reference")]
    public string monsterPrefabKey; // Prefab Addressables Key
    public GameObject projectilePrefab;
    

    [Header("Behavior Conditions")]
    public bool useHealthRetreat;           // ü�±�� ���� ���
    public float healthRetreatThreshold;    // ���� ���� ü�� ����
    public bool isPhaseChange;             // ������ ��ȯ�� ��������

}

public enum SpawnStrategyType
{
    Basic,
    Portal,    // ��Ż���� ����
    Summon     // ��ȯ ȿ���� ����
}

public enum MovementStrategyType
{
    Basic,
    Aggressive,    // ������ ����
    Defensive,     // �Ÿ� ����
    Ranged,        // ���Ÿ� ����
    Patrol,         // ���� ����
    Retreat         // ���� ����
}

public enum AttackStrategyType
{
    Basic,
    Melee,         // ���� ����
    Ranged,        // ���Ÿ� ����
    AoE,           // ���� ����
    Combo          // ���� ����
}
public enum HitStrategyType
{
    Basic,
    Elite,
    MiniBoss,   // �߰�����
    Boss        // ��������
}
public enum IdleStrategyType
{
    Basic,
    Defensive,     // ��� �ڼ�
    Alert,         // ��� ����
    Passive        // ������
}

public enum SkillStrategyType
{
    Basic,
    Buff,          // �ڽ� ��ȭ
    Debuff,        // �� ��ȭ
    Summon,        // ��ȯ
    AreaControl    // ���� ����
}
public enum DieStrategyType
{
    Basic,
    Explosion,     // �����ϸ� ����
    Split,         // �п��ϸ� ����
    Resurrection,  // ��Ȱ ����
    DropItem       // Ư�� ������ ���
}
public enum ProjectileMovementType
{
    Straight,
    Homing
}
public enum SkillEffectType
{
    Projectile,
    AreaEffect,
    Buff,
    Summon,
    // ... �ٸ� ����Ʈ Ÿ�Ե�
}
public enum BuffType
{
    None,
    AttackUp,       // ���ݷ� ����
    DefenseUp,      // ���� ����
    SpeedUp,        // �̵��ӵ� ����
    AttackSpeedUp,  // ���ݼӵ� ����
    Heal,           // ü�� ȸ��
    Rage,           // �г�(���ݷ� + ���ݼӵ�)
    Invincible,     // ����
    // �����
    AttackDown,     // ���ݷ� ����
    DefenseDown,    // ���� ����
    SpeedDown,      // �̵��ӵ� ����
    Stun,           // ����
    Poison,         // ��
    Burn,           // ȭ��
    Freeze          // ����
}

