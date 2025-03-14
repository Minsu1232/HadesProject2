using UnityEngine;

public class PlayerStatsUpgradeTester : MonoBehaviour
{
    public PlayerClass playerClass;

    void Start()
    {

        playerClass = GameInitializer.Instance.GetPlayerClass();
        // ���� �� ���� ���� �α�
        LogCurrentStats();


        TestUpgradeHP();


    }

    void LogCurrentStats()
    {
        if (playerClass == null) return;

        Stats stats = playerClass.GetStats();
        Debug.Log($"=== ���� ���� ===");
        Debug.Log($"ü��: {stats.Health}/{stats.MaxHealth}");
        Debug.Log($"���ݷ�: {stats.AttackPower}");
        Debug.Log($"�̵��ӵ�: {stats.Speed}");

        PlayerClassData data = playerClass._playerClassData;
        Debug.Log($"=== ���׷��̵� ī��Ʈ ===");
        Debug.Log($"ü�� ���׷��̵�: {data.characterStats.hpUpgradeCount}");
        Debug.Log($"���ݷ� ���׷��̵�: {data.characterStats.attackPowerUpgradeCount}");
        Debug.Log($"�̵��ӵ� ���׷��̵�: {data.characterStats.speedUpgradeCount}");
    }

    // �ν����Ϳ��� ȣ���ϴ� �׽�Ʈ �޼���
    public void TestUpgradeHP()
    {
        if (playerClass == null) return;

        playerClass.UpgradeHP();
        Debug.Log("ü�� ���׷��̵� �Ϸ�!");
        LogCurrentStats();


    }

    public void TestUpgradeAttack()
    {
        if (playerClass == null) return;

        playerClass.UpgradeAttackPower();
        Debug.Log("���ݷ� ���׷��̵� �Ϸ�!");
        LogCurrentStats();
    }

    public void TestSaveAndLoad()
    {
        SaveManager.Instance.SaveAllData();
        Debug.Log("���� �Ϸ�!");

 
        Debug.Log("�ε� �Ϸ�!");
        LogCurrentStats();
    }
}