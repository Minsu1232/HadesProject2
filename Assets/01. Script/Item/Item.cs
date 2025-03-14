// Item.cs - 기본 아이템 클래스
using UnityEngine;

[System.Serializable]
public class Item
{
    public int itemID;           // 아이템 고유 ID
    public string itemName;         // 아이템 이름
    public string description;      // 아이템 설명
    public Sprite icon;             // 아이템 아이콘
    public ItemType itemType;       // 아이템 유형
    public ItemRarity rarity;       // 아이템 희귀도
    public bool isStackable;        // 중첩 가능 여부
    public int maxStackSize = 99;   // 최대 중첩 수량

    // 아이템 타입 열거형
    public enum ItemType
    {
        Fragment,   // 파편
        Material,   // 재료
        Potion,     // 포션
        QuestItem,  // 퀘스트 아이템
        Misc        // 기타
    }

    // 아이템 희귀도 열거형
    public enum ItemRarity
    {
        Common,     // 일반
        Uncommon,   // 고급
        Rare,       // 희귀
        Epic,       // 에픽
        Legendary   // 전설
    }

    // 아이템 사용 가상 메서드
    public virtual bool Use()
    {
        Debug.Log($"Using item: {itemName}");
        return true; // 성공적으로 사용됨
    }
}