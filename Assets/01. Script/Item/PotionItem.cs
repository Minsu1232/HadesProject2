// PotionItem.cs - ���� ������ Ŭ����
using UnityEngine;

[System.Serializable]
public class PotionItem : Item
{
    public float potionValue;     // ȿ�� ��ġ
    public float duration;        // ���� �ð� (���� ������ ���)
    public PotionType potionType; // ���� Ÿ��

    public enum PotionType
    {
        Health,   // ü�� ȸ��
        Buff,     // ���� ȿ��
        Antidote, // �ص���
        Special   // Ư�� ȿ��
    }

    public PotionItem()
    {
        itemType = ItemType.Potion;
        isStackable = true; // ������ ��ø ����
    }

    // ���� ��� �޼��� �������̵�
    public override bool Use()
    {
        Debug.Log($"Using potion: {itemName}, Type: {potionType}, Value: {potionValue}");

        // ���� ȿ�� ���� ���� - ���߿� ����
        return true;
    }
}