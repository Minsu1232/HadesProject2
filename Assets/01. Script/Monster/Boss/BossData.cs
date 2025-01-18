using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class PhaseData
{
    public bool isInvulnerable;
    [Header("Phase Settings")]
    public string phaseName;
    public float healthThreshold;
    public float transitionDuration;
    public bool isInvulnerableDuringTransition;

    [Header("Pattern Settings")]
    public float patternChangeTime;
    public float[] patternWeights;

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