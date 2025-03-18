using UnityEngine;

public class PassiveAbilityTester : MonoBehaviour
{
    private PlayerClass playerClass;
    private PassiveAbility testAbility;

    void Start()
    {
        // �÷��̾� Ŭ���� ��������
        playerClass = GameInitializer.Instance.GetPlayerClass();

        if (playerClass == null)
        {
            Debug.LogError("�÷��̾� Ŭ������ ã�� �� �����ϴ�.");
            return;
        }

        // �׽�Ʈ�� �нú� �ɷ� ���� (���� ���� �ɷ�)
        testAbility = new PassiveAbility();
        testAbility.Initialize(
            PassiveAbility.PassiveType.DamageReduction,
            20f, // 20% ���� ����
            "�׽�Ʈ ����",
            "���ظ� 20% ���ҽ�ŵ�ϴ�.",
            Rarity.Common
        );

        // ���� ������ ��� �α�
        Debug.Log($"�нú� ���� �� ������ ���: {playerClass.PlayerStats.DamageReceiveRate}");

        // �нú� ����
        testAbility.OnAcquire(playerClass);

        // ���� �� ������ ��� �α�
        Debug.Log($"�нú� ���� �� ������ ���: {playerClass.PlayerStats.DamageReceiveRate}");

        // 5�� �Ŀ� �нú� ���� ����
        Invoke("RemovePassive", 5f);
    }

    void RemovePassive()
    {
        // �нú� ����
        testAbility.OnReset(playerClass);

        // ���� �� ������ ��� �α�
        Debug.Log($"�нú� ���� �� ������ ���: {playerClass.PlayerStats.DamageReceiveRate}");
    }
}