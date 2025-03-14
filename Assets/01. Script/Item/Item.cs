// Item.cs - �⺻ ������ Ŭ����
using UnityEngine;

[System.Serializable]
public class Item
{
    public int itemID;           // ������ ���� ID
    public string itemName;         // ������ �̸�
    public string description;      // ������ ����
    public Sprite icon;             // ������ ������
    public ItemType itemType;       // ������ ����
    public ItemRarity rarity;       // ������ ��͵�
    public bool isStackable;        // ��ø ���� ����
    public int maxStackSize = 99;   // �ִ� ��ø ����

    // ������ Ÿ�� ������
    public enum ItemType
    {
        Fragment,   // ����
        Material,   // ���
        Potion,     // ����
        QuestItem,  // ����Ʈ ������
        Misc        // ��Ÿ
    }

    // ������ ��͵� ������
    public enum ItemRarity
    {
        Common,     // �Ϲ�
        Uncommon,   // ���
        Rare,       // ���
        Epic,       // ����
        Legendary   // ����
    }

    // ������ ��� ���� �޼���
    public virtual bool Use()
    {
        Debug.Log($"Using item: {itemName}");
        return true; // ���������� ����
    }
}