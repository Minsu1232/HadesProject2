// PotionItem.cs - 포션 아이템 클래스
using UnityEngine;

[System.Serializable]
public class PotionItem : Item
{
    public float potionValue;     // 효과 수치
    public float duration;        // 지속 시간 (버프 포션의 경우)
    public PotionType potionType; // 포션 타입

    public enum PotionType
    {
        Health,   // 체력 회복
        Buff,     // 버프 효과
        Antidote, // 해독제
        Special   // 특수 효과
    }

    public PotionItem()
    {
        itemType = ItemType.Potion;
        isStackable = true; // 포션은 중첩 가능
    }

    // 포션 사용 메서드 오버라이드
    public override bool Use()
    {
        Debug.Log($"Using potion: {itemName}, Type: {potionType}, Value: {potionValue}");

        // 포션 효과 적용 로직 - 나중에 구현
        return true;
    }
}