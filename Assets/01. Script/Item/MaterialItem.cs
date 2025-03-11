// MaterialItem.cs - 재료 아이템 클래스
using UnityEngine;

[System.Serializable]
public class MaterialItem : Item
{
    public string materialCategory; // 재료 분류

    public MaterialItem()
    {
        itemType = ItemType.Material;
        isStackable = true; // 재료는 중첩 가능
    }
}