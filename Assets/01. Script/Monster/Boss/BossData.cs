using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class AttackStepData
{
    [Header("Attack Settings")]
    public AttackStrategyType attackType;
    public float stepDelay;

    [Header("Mini Game Settings")]
    public bool hasMiniGame;
    public MiniGameType miniGameType;
    public bool waitForMiniGame;
    public float miniGameDifficulty;

    [Header("Effects")]
    public GameObject stepStartEffect;
    public GameObject stepEndEffect;
    public string stepAnimationTrigger;

    public bool isTransitionAnim; // �ִϸ��̼� ��ȯ��
}

[System.Serializable]
public class AttackPatternData
{
    [Header("Pattern Settings")]
    public string patternName;
    public List<AttackStepData> steps = new List<AttackStepData>();
    public float patternWeight = 1.0f;
    public int phaseNumber;

    [Header("Timing")]
    public float patternCooldown;
    public float warningDuration;

    [Header("Requirements")]
    public float healthThresholdMin;  // �� ü��% �̻��� ���� ��� ����
    public float healthThresholdMax;  // �� ü��% ������ ���� ��� ����

    [Header("Effects")]
    public GameObject patternStartEffect;
    public GameObject patternEndEffect;
    public string warningMessage;

    [Header("Mini Game Success Requirements")]  // ���� �߰��� �κ�
    public bool isDisabled = false;            // ���� ��Ȱ��ȭ ����
    public int requiredSuccessCount = 3;       // �ʿ��� ���� Ƚ��
    public int currentSuccessCount = 0;        // ���� ���� Ƚ��

    [Header("Difficulty Settings")]
    public float baseDifficulty = 1f;          // �⺻ ���̵�
    public float maxDifficulty = 3f;           // �ִ� ���̵�
    public float difficultyIncreaseStep = 0.5f;  // ������ ������

    // ��Ÿ�ӿ��� ������ ���� ���̵� (BossMonster���� ����)
    public float currentDifficulty;

}
[System.Serializable]
public class GimmickData
{
    [Header("Gimmick Base Settings")]
    public string gimmickName;//
    public GimmickType type;//
    public float triggerHealthThreshold;//
    public float duration;//
    public bool isEnabled = true;  // ��� Ȱ��ȭ/��Ȱ��ȭ ���
    public int successCount;
    [Header("Conditions")]
    public bool requirePlayerAction;//
    public bool isInterruptible;
    public bool destroyAfterUse = true;    // ��� ���ѽ� ���� �Ұ����ϰ�

    [Header("Combat Settings")]
    public bool makeInvulnerable;// ��������
    public float damageMultiplier;//
    public float failDamage;//
    public float damage;     // damagePerSecond (���� ���ؿ�)
    public bool affectStatusEffects;   // �����̻� �鿪/���� ����
   

    [Header("Position Settings")]
    public bool useCustomPosition; //
    public Vector3 gimmickPosition; //
    public float areaRadius; //
    public bool followTarget;          // ����: followPlayer -> followTarget (�� �Ϲ����� �뵵��)
    public LayerMask collisionMask;    // �浹 üũ�� ���̾�

    [Header("Timing Settings")]
    public float preparationTime;      // ��� ���� �� �غ� �ð�
    
    public float repeatInterval;       // �ݺ� ���� ���� (0�̸� �ݺ� ����)

    [Header("Visual & Audio")]
    public GameObject warningEffect;   // ��� ����Ʈ
    public GameObject activeEffect;    // Ȱ��ȭ ����Ʈ
    public AudioClip warningSound;     // ��� ����
    public AudioClip activeSound;      // Ȱ��ȭ ����

    // ���ŵ� �׸�:
    // timeLimit (duration���� ����)
    // endTime (duration���� ����)
}
[System.Serializable]
public class PhaseData
{
    [Header("Phase Base Settings")]
    public string phaseName;
    public bool isInvulnerable;
    public float phaseTransitionThreshold;  // �̸� ����: healthThreshold -> phaseTransitionThreshold
    public float transitionDuration;
    public bool isInvulnerableDuringTransition;
   

    [Header("Pattern Settings")]
    public float patternChangeTime;
    public List<AttackPatternData> availablePatterns = new List<AttackPatternData>();  // patternWeights ��ü

    [Header("Strategy Settings")]
    public MovementStrategyType moveType;
    public AttackStrategyType attackType;
    public SkillStrategyType skillType;
    public PhaseTransitionType phaseTransitionType;
    public GimmickType gimmickType;

    [Header("Phase Multipliers")]
    public float damageMultiplier;
    public float speedMultiplier;
    public float defenseMultiplier;
    public float attackSpeedMultiplier;

    [Header("Combat Settings")]
    public bool canBeInterrupted;
    public float stunResistance;
    public bool useHealthRetreat;
    public float healthRetreatThreshold;
    public float retreatDuration;

    [Header("Visual Effects")]
    public GameObject phaseStartEffect;
    public GameObject phaseLoopEffect;
    public GameObject exitEffect;
    public string cutscenePath;

    [Header("Special Mechanics")]
    public string specialMechanicType;
    public float specialMechanicValue;
    [Header("Gimmicks")]
    public List<GimmickData> gimmicks = new List<GimmickData>();
}

[CreateAssetMenu(fileName = "BossData", menuName = "Monster/Boss Data")]
public class BossData : MonsterData
{
    [Header("Boss Base Settings")]
    public bool canBeInterrupted;
    public float phaseTransitionDuration;
    public float rageModeThreshold;
    public float rageModeDuration;
    public bool invincibleOnSpawn;
    public float aggroRange;

    [Header("Phase Settings")]
    public List<PhaseData> phaseData;
    public int phaseCount;
   
    [Header("Special Abilities")]
    public bool canSummonMinions;
    public GameObject[] minionPrefabs;
    public int maxMinionCount;    
    public float summonInterval;

    [Header("Visual Effects & UI")]
    public GameObject[] phaseTransitionEffects;
    public GameObject rageEffect;
    public GameObject spawnEffect;
    public bool showHealthBar;
    public bool showPhaseNames;
    public Color[] phaseColors;

    [Header("Skill Settings")]
    public float globalSkillCooldownModifier;
    public float skillPreDelayDefault;
    public float skillPostDelayDefault;

    [Header("Camera Settings")]
    public float defaultCameraDistance;
    public float defaultCameraHeight;
    public float defaultCameraFOV;

    [Header("Combat Stats")]
    
    public float globalStunResistance;
    public float knockbackResistance;


   
}
public enum PhaseTransitionType
{
    Basic,
    AreaAttack,
    TerrainChange,
    Summon,
    // ... �ٸ� ��ȯ Ÿ�Ե�
}
// ��� Ÿ�� ����
public enum GimmickType
{
    None,
    FieldHazard,      // ���� ���
    WavePattern,      // ������ ����
    EnvironmentChange, // ȯ�� ��ȭ
    Summon,           // ��ȯ
    DamageReflect,    // ������ �ݻ�
    RestrictArea      // Ȱ�� ���� ����
}