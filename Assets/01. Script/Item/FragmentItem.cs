// FragmentItem.cs - 파편 아이템 클래스
using UnityEngine;

[System.Serializable]
public class FragmentItem : Item
{
    // 기본 스탯 보너스
    public float attackBonus;    // 공격력 보너스 
    public float defenseBonus;   // 방어력 보너스
    public float healthBonus;    // 체력 보너스
    public float speedBonus;     // 이동속도 보너스

    // 특수 속성들
    public bool isResonated;         // 공명 상태 여부
    public string associatedBossID;  // 연계된 보스 ID
    public FragmentType fragType;    // 파편 타입

    // 파편 타입 열거형
    public enum FragmentType
    {
        Human,    // 인간족
        Beast,    // 야수족
        Dragon,   // 용족
        Undead,   // 언데드족
        Fusion    // 융합(최종 보스)
    }

    public FragmentItem()
    {
        itemType = ItemType.Fragment;
        isStackable = false; // 파편은 중첩 불가
    }

    // 파편 사용(장착) 메서드 오버라이드
    public override bool Use()
    {
        Debug.Log($"Equipping fragment: {itemName}");

        // 파편 장착 처리 - 나중에 구현할 FragmentManager에 연결
        return true;
    }
}
