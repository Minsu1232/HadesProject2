// MaterialItem.cs - ��� ������ Ŭ����
using UnityEngine;

[System.Serializable]
public class MaterialItem : Item
{
    public string materialCategory; // ��� �з�

    public MaterialItem()
    {
        itemType = ItemType.Material;
        isStackable = true; // ���� ��ø ����
    }
}