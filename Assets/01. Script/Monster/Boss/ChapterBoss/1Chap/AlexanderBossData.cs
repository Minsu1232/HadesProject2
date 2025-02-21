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
}