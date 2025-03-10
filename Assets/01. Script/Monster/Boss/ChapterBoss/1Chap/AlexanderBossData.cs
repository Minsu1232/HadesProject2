using UnityEngine;

[CreateAssetMenu(fileName = "AlexanderBossData", menuName = "Boss/Alexander Data")]
public class AlexanderBossData : BossData
{
    [Header("Essence System Settings")]
    public float initialEssence;
    public string essenceName;
    public float maxEssence;
    public float essenceThreshold;
    public float playerAttackBuff;
    public float playerDamageBuff;
    public float maxEssenceStunTime;

    [Header("Madness Crack Hazard Settings")]
    public bool enableMadnessCrack = true;       // 광기 균열 활성화 여부
    public float crackWarningDuration = 1.5f;    // 경고 지속 시간
    public float crackRadius = 3f;               // 균열 반경
    public float crackDamage = 20f;              // 균열 데미지
    public float crackDamageMultiplier = 1.2f;   // 데미지 배수
    public float crackCooldownMin = 2f;          // 최소 쿨다운
    public float crackCooldownMax = 5f;          // 최대 쿨다운
    public GameObject crackPrefab;               // 균열 프로젝타일 프리팹
    public GameObject crackIndicatorPrefab;      // 인디케이터 프리팹
    public GameObject crackExplosionPrefab;      // 폭발 이펙트 프리팹
}