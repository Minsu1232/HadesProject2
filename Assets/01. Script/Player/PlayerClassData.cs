using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerClass")]
[System.Serializable]
public class PlayerClassData : ScriptableObject
{
    public string userID;
    public int currentChapter;
    public List<int> completedQuests;

    [System.Serializable]
    public class CharacterStats
    {
        public int baseHp;  // 대소문자 JSON과 맞춤
        public int baseGage;
        public int baseAttackPower;
        public float baseAttackSpeed;
        public float baseCriticalCance;
        public float baseSpeed;
        public int upgradeCount;
        public float damageReceiveRate;  // 기본 피해 배율
    }
    public CharacterStats characterStats;  // JSON 구조와 맞춤

    public List<InventoryItem> inventory;

    [System.Serializable]
    public class InventoryItem
    {
        public int itemID;
        public int quantity;
    }
}
