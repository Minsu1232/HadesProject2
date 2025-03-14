using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerClass")]
[System.Serializable]
public class PlayerClassData : ScriptableObject
{
    public string userID;
    public int currentChapter;
    public List<int> completedQuests = new List<int>();

    [System.Serializable]
    public class CharacterStats
    {
        // 변경 가능한 업그레이드 카운트 (이것만 저장/로드 됨)
        public int hpUpgradeCount = 0;
        public int gageUpgradeCount = 0;
        public int attackPowerUpgradeCount = 0;
        public int attackSpeedUpgradeCount = 0;
        public int criticalChanceUpgradeCount = 0;
        public int speedUpgradeCount = 0;
        public int damageReduceUpgradeCount = 0;

        // 호환성을 위한 총 업그레이드 카운트
        public int upgradeCount = 0;

        // 실제 게임에서 사용할 계산된 최종 스탯값 구하기
        public int GetCalculatedHP()
        {
            return StatConstants.BASE_HP + (hpUpgradeCount * StatConstants.HP_PER_UPGRADE);
        }

        public int GetCalculatedGage()
        {
            return StatConstants.BASE_GAGE + (gageUpgradeCount * StatConstants.GAGE_PER_UPGRADE);
        }

        public int GetCalculatedAttackPower()
        {
            return StatConstants.BASE_ATTACK_POWER + (attackPowerUpgradeCount * StatConstants.ATTACK_POWER_PER_UPGRADE);
        }

        public float GetCalculatedAttackSpeed()
        {
            return StatConstants.BASE_ATTACK_SPEED + (attackSpeedUpgradeCount * StatConstants.ATTACK_SPEED_PER_UPGRADE);
        }

        public float GetCalculatedCriticalChance()
        {
            return StatConstants.BASE_CRITICAL_CHANCE + (criticalChanceUpgradeCount * StatConstants.CRITICAL_CHANCE_PER_UPGRADE);
        }

        public float GetCalculatedSpeed()
        {
            return StatConstants.BASE_SPEED + (speedUpgradeCount * StatConstants.SPEED_PER_UPGRADE);
        }

        public float GetCalculatedDamageReceiveRate()
        {
            return StatConstants.BASE_DAMAGE_RECEIVE_RATE - (damageReduceUpgradeCount * StatConstants.DAMAGE_REDUCE_PER_UPGRADE);
        }

        // 모든 업그레이드 카운트의 합계 계산
        public void UpdateTotalUpgradeCount()
        {
            upgradeCount = hpUpgradeCount + gageUpgradeCount + attackPowerUpgradeCount +
                           attackSpeedUpgradeCount + criticalChanceUpgradeCount +
                           speedUpgradeCount + damageReduceUpgradeCount;
        }
    }

    public CharacterStats characterStats = new CharacterStats();
    public List<InventoryItem> inventory = new List<InventoryItem>();

    [System.Serializable]
    public class InventoryItem
    {
        public int itemID;
        public int quantity;
    }

    // 플레이어 스탯 업그레이드 메서드
    public void UpgradeHP(int count = 1)
    {
        characterStats.hpUpgradeCount += count;
        characterStats.UpdateTotalUpgradeCount();
    }

    public void UpgradeGage(int count = 1)
    {
        characterStats.gageUpgradeCount += count;
        characterStats.UpdateTotalUpgradeCount();
    }

    public void UpgradeAttackPower(int count = 1)
    {
        characterStats.attackPowerUpgradeCount += count;
        characterStats.UpdateTotalUpgradeCount();
    }

    public void UpgradeAttackSpeed(int count = 1)
    {
        characterStats.attackSpeedUpgradeCount += count;
        characterStats.UpdateTotalUpgradeCount();
    }

    public void UpgradeCriticalChance(int count = 1)
    {
        characterStats.criticalChanceUpgradeCount += count;
        characterStats.UpdateTotalUpgradeCount();
    }

    public void UpgradeSpeed(int count = 1)
    {
        characterStats.speedUpgradeCount += count;
        characterStats.UpdateTotalUpgradeCount();
    }

    public void UpgradeDamageReduce(int count = 1)
    {
        characterStats.damageReduceUpgradeCount += count;
        characterStats.UpdateTotalUpgradeCount();
    }
}