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
    public bool enableMadnessCrack = true;       // ���� �տ� Ȱ��ȭ ����
    public float crackWarningDuration = 1.5f;    // ��� ���� �ð�
    public float crackRadius = 3f;               // �տ� �ݰ�
    public float crackDamage = 20f;              // �տ� ������
    public float crackDamageMultiplier = 1.2f;   // ������ ���
    public float crackCooldownMin = 2f;          // �ּ� ��ٿ�
    public float crackCooldownMax = 5f;          // �ִ� ��ٿ�
    public GameObject crackPrefab;               // �տ� ������Ÿ�� ������
    public GameObject crackIndicatorPrefab;      // �ε������� ������
    public GameObject crackExplosionPrefab;      // ���� ����Ʈ ������
}