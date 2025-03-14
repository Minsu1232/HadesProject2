using UnityEngine;

public class PlayerStatsUpgradeTester : MonoBehaviour
{
    public PlayerClass playerClass;

    void Start()
    {

        playerClass = GameInitializer.Instance.GetPlayerClass();
        // 시작 시 현재 스탯 로그
        LogCurrentStats();


        TestUpgradeHP();


    }

    void LogCurrentStats()
    {
        if (playerClass == null) return;

        Stats stats = playerClass.GetStats();
        Debug.Log($"=== 현재 스탯 ===");
        Debug.Log($"체력: {stats.Health}/{stats.MaxHealth}");
        Debug.Log($"공격력: {stats.AttackPower}");
        Debug.Log($"이동속도: {stats.Speed}");

        PlayerClassData data = playerClass._playerClassData;
        Debug.Log($"=== 업그레이드 카운트 ===");
        Debug.Log($"체력 업그레이드: {data.characterStats.hpUpgradeCount}");
        Debug.Log($"공격력 업그레이드: {data.characterStats.attackPowerUpgradeCount}");
        Debug.Log($"이동속도 업그레이드: {data.characterStats.speedUpgradeCount}");
    }

    // 인스펙터에서 호출하는 테스트 메서드
    public void TestUpgradeHP()
    {
        if (playerClass == null) return;

        playerClass.UpgradeHP();
        Debug.Log("체력 업그레이드 완료!");
        LogCurrentStats();


    }

    public void TestUpgradeAttack()
    {
        if (playerClass == null) return;

        playerClass.UpgradeAttackPower();
        Debug.Log("공격력 업그레이드 완료!");
        LogCurrentStats();
    }

    public void TestSaveAndLoad()
    {
        SaveManager.Instance.SaveAllData();
        Debug.Log("저장 완료!");

 
        Debug.Log("로드 완료!");
        LogCurrentStats();
    }
}