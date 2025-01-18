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
        public int baseHp;  // ��ҹ��� JSON�� ����
        public int baseGage;
        public int baseAttackPower;
        public float baseAttackSpeed;
        public float baseCriticalCance;
        public float baseSpeed;
        public int upgradeCount;
        public float damageReceiveRate;  // �⺻ ���� ����
    }
    public CharacterStats characterStats;  // JSON ������ ����

    public List<InventoryItem> inventory;

    [System.Serializable]
    public class InventoryItem
    {
        public int itemID;
        public int quantity;
    }
}
