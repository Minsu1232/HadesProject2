using System.Collections.Generic;
using UnityEngine;

public enum MiniGameType
{
    None,
    Dodge,
    Parry,
    QuickTime
}

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
    public float healthThresholdMin;  // 이 체력% 이상일 때만 사용 가능
    public float healthThresholdMax;  // 이 체력% 이하일 때만 사용 가능

    [Header("Effects")]
    public GameObject patternStartEffect;
    public GameObject patternEndEffect;
    public string warningMessage;
}

[System.Serializable]
public class PhaseData
{
    [Header("Phase Base Settings")]
    public string phaseName;
    public bool isInvulnerable;
    public float phaseTransitionThreshold;  // 이름 변경: healthThreshold -> phaseTransitionThreshold
    public float transitionDuration;
    public bool isInvulnerableDuringTransition;

    [Header("Pattern Settings")]
    public float patternChangeTime;
    public List<AttackPatternData> availablePatterns = new List<AttackPatternData>();  // patternWeights 대체

    [Header("Strategy Settings")]
    public MovementStrategyType moveType;
    public AttackStrategyType attackType;
    public SkillStrategyType skillType;

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