using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Sirenix.OdinInspector;

/// <summary>
/// �������̽� Ȱ�� �� ������ ����/���������� ������Ƽ
/// </summary>
[CreateAssetMenu(fileName = "MonsterData", menuName = "Monster/MonsterData")]

public class MonsterData : ScriptableObject, ICreatureData
{
    public enum MonsterGrade
    {
        Normal,     // �⺻ ��
        Elite,      // ���� ��
        MiniBoss,   // �߰�����
        Boss        // ��������
    }

    #region ICreatureData Implementation
    [FoldoutGroup("Basic Info"), ShowInInspector]
    public int MonsterID { get; set; }
    [FoldoutGroup("Basic Info"), ShowInInspector]
    public string MonsterName { get; set; }

    [FoldoutGroup("Basic Info"), ShowInInspector]
    public MonsterGrade Grade { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public int initialHp { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public int initialAttackPower { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public float initialAttackSpeed { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public int initialSpeed { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public int initialDeffense { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public float attackRange { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public float dropChance { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public int dropItem { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public int armorValue { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public float chargeSpeed { get; set; }
    [FoldoutGroup("Base Stats"), ShowInInspector]
    public float chargeDuration { get; set; }
    [FoldoutGroup("Base Stats"), ShowInInspector]
    public float prepareTime { get; set; }
    [FoldoutGroup("Base Stats"), ShowInInspector]
    public GameObject chargeIndicatorPrefab { get; set; }

    [FoldoutGroup("Base Stats"), ShowInInspector]
    public GameObject ChargePrepareDustEffect { get; set; }
    [FoldoutGroup("Base Stats"), ShowInInspector]
    public GameObject ChargeStartEffect { get; set; }
    [FoldoutGroup("Base Stats"), ShowInInspector]
    public GameObject ChargeTrailEffect { get; set; }
    [FoldoutGroup("Base Stats"), ShowInInspector]
    public GameObject WallImpactEffect { get; set; }
    [FoldoutGroup("Base Stats"), ShowInInspector]
    public GameObject PlayerImpactEffect { get; set; }

    [FoldoutGroup("Movement Settings"), ShowInInspector]
    public int moveRange { get; set; }

    [FoldoutGroup("Movement Settings"), ShowInInspector]
    public int chaseRange { get; set; }

    [FoldoutGroup("Movement Settings"), ShowInInspector]
    public int aggroDropRange { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float skillCooldown { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float skillRange { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float skillDuration { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float skillDamage { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float projectileSpeed { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float rotateSpeed { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public string skillSpawnPointTag { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public GameObject areaEffectPrefab { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public GameObject shorckEffectPrefab { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float areaRadius { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float areaDuration { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public int multiShotCount { get; set; }  // ���� �߻� Ƚ��

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float multiShotInterval { get; set; }  // �߻� ����

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public Vector3 projectileRotationAxis { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float projectileRotationSpeed { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float heightFactor { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public GameObject howlEffectPrefab { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public AudioClip howlSound { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float howlRadius { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float howlDuration { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float EssenceAmount { get; set; }

    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public GameObject circleIndicatorPrefab { get; set; }
    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float safeZoneRadius { get; set; }
    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public float dangerRadiusMultiplier { get; set; }
    [FoldoutGroup("Skill Settings"), ShowInInspector]
    public GameObject ExplosionEffect { get; set; }

    [FoldoutGroup("Buff Settings"), ShowInInspector]
    public BuffData buffData { get; set; } = new BuffData();

    [FoldoutGroup("Buff Settings"), ShowInInspector]
    public BuffType buffType { get; set; }

    [FoldoutGroup("Buff Settings"), ShowInInspector]
    public float buffDuration { get; set; }

    [FoldoutGroup("Buff Settings"), ShowInInspector]
    public float buffValue { get; set; }

    [FoldoutGroup("Buff Settings"), ShowInInspector]
    public GameObject buffEffectPrefab { get; set; }

    [FoldoutGroup("Summon Settings"), ShowInInspector]
    public GameObject summonPrefab { get; set; }

    [FoldoutGroup("Summon Settings"), ShowInInspector]
    public int summonCount { get; set; }

    [FoldoutGroup("Summon Settings"), ShowInInspector]
    public float summonRadius { get; set; }

    [FoldoutGroup("State Durations"), ShowInInspector]
    public float hitStunDuration { get; set; }

    [FoldoutGroup("State Durations"), ShowInInspector]
    public float deathDuration { get; set; }

    [FoldoutGroup("State Durations"), ShowInInspector]
    public float spawnDuration { get; set; }

    [FoldoutGroup("Hit Settings"), ShowInInspector]
    public float superArmorThreshold { get; set; }

    [FoldoutGroup("Hit Settings"), ShowInInspector]
    public float hitStunMultiplier { get; set; }

    [FoldoutGroup("Hit Settings"), ShowInInspector]
    public float knockbackForce { get; set; }

    [FoldoutGroup("Hit Settings"), ShowInInspector]
    public float cameraShakeIntensity { get; set; }

    [FoldoutGroup("Hit Settings"), ShowInInspector]
    public float cameraShakeDuration { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public SpawnStrategyType spawnStrategy { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public MovementStrategyType moveStrategy { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public AttackStrategyType attackStrategy { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public IdleStrategyType idleStrategy { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public SkillStrategyType skillStrategy { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public DieStrategyType dieStrategy { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public HitStrategyType hitStrategy { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public ProjectileMovementType projectileType { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public ProjectileImpactType projectileImpactType { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public SkillEffectType skillEffectType { get; set; }

    [FoldoutGroup("Strategies"), ShowInInspector]
    public GroggyStrategyType groggyStrategy { get; set; }

    [FoldoutGroup("References"), ShowInInspector]
    public string monsterPrefabKey { get; set; }

    [FoldoutGroup("References"), ShowInInspector]
    public GameObject projectilePrefab { get; set; }

    [FoldoutGroup("References"), ShowInInspector]
    public GameObject hitEffect { get; set; }

    [FoldoutGroup("Behavior Conditions"), ShowInInspector]
    public bool useHealthRetreat { get; set; }

    [FoldoutGroup("Behavior Conditions"), ShowInInspector]
    public float healthRetreatThreshold { get; set; }

    [FoldoutGroup("Behavior Conditions"), ShowInInspector]
    public bool isPhaseChange { get; set; }

    [FoldoutGroup("Visual"), ShowInInspector]
    public Material eliteOutlineMaterial { get; set; }

    [FoldoutGroup("Visual"), ShowInInspector]
    public float shockwaveRadius { get; set; }

    [FoldoutGroup("Visual"), ShowInInspector]
    public float groggyTime { get; set; }


}
#endregion


public enum SpawnStrategyType
{
    Basic,
    Portal,    // ��Ż���� ����
    Summon     // ��ȯ ȿ���� ����
}

public enum MovementStrategyType
{
    None,
    Basic,
    Aggressive,    // ������ ����
    Defensive,     // �Ÿ� ����
    Ranged,        // ���Ÿ� ����
    Patrol,         // ���� ����
    Retreat         // ���� ����
}

public enum AttackStrategyType
{
    None,
    Basic,
    Charge,
    Rush,
    Jump,
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
    None,
    Basic,
    Buff,          // �ڽ� ��ȭ
    Debuff,        // �� ��ȭ
    MultiShot,  // n�� �߻� 
    Summon,
    Area,// ��ȯ
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
    None,
    Straight,
    StraightRotation,
    Homing,
    Parabolic    // �߰�
}

public enum SkillEffectType
{
    None,
    Projectile,
    AreaEffect,
    Buff,
    CircularProjectile,
    Summon,
    Howl
    // ... �ٸ� ����Ʈ Ÿ�Ե�
}

public enum ProjectileImpactType
{
    None,
    Basic,
    Poison,
    DelayedExplosion,
    Damage

    //Explosion,
    //Freeze
    // �߰� ����
}

public enum GroggyStrategyType
{
    None,
    Basic,
    Poison,
    //Explosion,
    //Freeze
    // �߰� ����
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

[System.Serializable]
public class BuffData
{
    public BuffType[] buffTypes;
    public float[] durations;
    public float[] values;
}