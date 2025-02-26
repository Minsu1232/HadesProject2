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

    public bool isTransitionAnim; // 애니메이션 전환용
}

[System.Serializable]
public class AttackPatternData
{
    [Header("Pattern Settings")]
    public string patternName;
    public BossPatternType patternType;  // 추가
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

    [Header("Mini Game Success Requirements")]  // 새로 추가한 부분
    public bool isDisabled = false;            // 패턴 비활성화 여부
    public int requiredSuccessCount = 3;       // 필요한 성공 횟수
    public int currentSuccessCount = 0;        // 현재 성공 횟수

    [Header("Difficulty Settings")]
    public float baseDifficulty = 1f;          // 기본 난이도
    public float maxDifficulty = 3f;           // 최대 난이도
    public float difficultyIncreaseStep = 0.5f;  // 성공당 증가량

    // 런타임에서 관리될 현재 난이도 (BossMonster에서 관리)
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
    public bool isEnabled = true;  // 기믹 활성화/비활성화 토글
    public int successCount;
    public float moveSpeed;             // 이동 속도
    public HazardSpawnType hazardSpawnType;         // 스폰 타입
    public TargetType targetType;  // Transform 대신 타겟 타입으로 관리

    [Header("Conditions")]
    public bool requirePlayerAction;//
    public bool isInterruptible;
    public bool destroyAfterUse = true;    // 기믹 파훼시 재사용 불가능하게

    [Header("Combat Settings")]
    public bool makeInvulnerable;// 무적유무
    public float damageMultiplier;//
    public float failDamage;//
    public float damage;     // damagePerSecond (지속 피해용)
    public bool affectStatusEffects;   // 상태이상 면역/적용 여부
    
   
   
    [Header("Position Settings")]
    public bool useCustomPosition; //
    public Vector3 gimmickPosition; //
    public float areaRadius; //
    public bool followTarget;          // 변경: followPlayer -> followTarget (더 일반적인 용도로)
    public LayerMask collisionMask;    // 충돌 체크용 레이어

    [Header("Timing Settings")]
    public float preparationTime;      // 기믹 시작 전 준비 시간    
    public float repeatInterval;       // 반복 실행 간격 (0이면 반복 안함)

    [Header("Visual & Audio")]
    public GameObject warningEffect;   // 경고 이펙트
    public GameObject activeEffect;    // 활성화 이펙트
    public GameObject hazardPrefab;     // 공격 프리팹
    public string hazardPrefabKey;
    public AudioClip warningSound;     // 경고 사운드
    public AudioClip activeSound;      // 활성화 사운드

    // 제거된 항목:
    // timeLimit (duration으로 통합)
    // endTime (duration으로 통합)
}
[System.Serializable]
public class AttackStrategyWeight
{
    public AttackStrategyType type;
    public float weight;
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
    public List<AttackStrategyWeight> phaseAttackStrategies = new List<AttackStrategyWeight>();
    public float patternStrategyWeight = 0.4f;  // 기본값 0.4
    [Header("Skill Config Settings")]
    public List<int> skillConfigIds = new List<int>();          // 스킬 구성 ID 리스트
    public List<float> skillConfigWeights = new List<float>();  // 각 스킬 구성의 선택 가중치
    public float skillStrategyWeight = 0.4f;                    // 스킬 전략의 전체 가중치 (기본값 0.4)
    [Header("Strategy Settings")]
    public MovementStrategyType moveType;
    public AttackStrategyType attackType;
    public SkillStrategyType skillType;
    public PhaseTransitionType phaseTransitionType;
    

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

    [Header("Audio Settings")]
    public AudioClip roarSound;


}
public enum PhaseTransitionType
{
    Basic,
    AreaAttack,
    TerrainChange,
    Summon,
    // ... 다른 전환 타입들
}
// 기믹 타입 정의
public enum GimmickType
{
    None,
    FieldHazard,      // 장판 기믹
    WavePattern,      // 패턴형 공격
    EnvironmentChange, // 환경 변화
    Summon,           // 소환
    DamageReflect,    // 데미지 반사
    RestrictArea      // 활동 영역 제한
}
public enum BossPatternType
{
    None,
    BasicToJump,
    JumpToBasic,
    ChargeToJump,
    SpinToJump,
    // 필요한 패턴 타입들 추가
}