// FragmentItem.cs - ���� ������ Ŭ����
using UnityEngine;

[System.Serializable]
public class FragmentItem : Item
{
    // �⺻ ���� ���ʽ�
    public float attackBonus;    // ���ݷ� ���ʽ� 
    public float defenseBonus;   // ���� ���ʽ�
    public float healthBonus;    // ü�� ���ʽ�
    public float speedBonus;     // �̵��ӵ� ���ʽ�

    // Ư�� �Ӽ���
    public bool isResonated;         // ���� ���� ����
    public string associatedBossID;  // ����� ���� ID
    public FragmentType fragType;    // ���� Ÿ��

    // ���� Ÿ�� ������
    public enum FragmentType
    {
        Human,    // �ΰ���
        Beast,    // �߼���
        Dragon,   // ����
        Undead,   // �𵥵���
        Fusion    // ����(���� ����)
    }

    public FragmentItem()
    {
        itemType = ItemType.Fragment;
        isStackable = false; // ������ ��ø �Ұ�
    }

    // ���� ���(����) �޼��� �������̵�
    public override bool Use()
    {
        Debug.Log($"Equipping fragment: {itemName}");

        // ���� ���� ó�� - ���߿� ������ FragmentManager�� ����
        return true;
    }
}
