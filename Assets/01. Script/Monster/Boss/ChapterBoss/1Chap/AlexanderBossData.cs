using UnityEngine;

[CreateAssetMenu(fileName = "AlexanderBossData", menuName = "Boss/Alexander Data")]
public class AlexanderBossData : BossData
{
    [Header("Rage System Settings")]
    public float initialRage = 0f;
    public float maxRage = 100f;
    public float rageIncreaseRate = 5f;
    public float rageModeThreshold = 70f;  // 70% �̻�� �������
    public float rageDecayRate = 2f;       // �ʴ� ���ҷ�

    [Header("Rage Mode Effects")]
    public float rageDamageMultiplier = 1.3f;
    public float rageSpeedMultiplier = 1.2f;
    public float rageAttackSpeedMultiplier = 1.2f;
}