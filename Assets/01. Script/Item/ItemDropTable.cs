// 아이템 드롭 테이블 클래스
[System.Serializable]
public class ItemDropEntry
{
    public int itemId;           // 아이템 ID
    public float dropChance;     // 드롭 확률 (0-100%)
    public int minQuantity = 1;  // 최소 수량
    public int maxQuantity = 1;  // 최대 수량

    // 편집 도우미 필드 (실제 계산에 사용되지 않음)
    public string itemName;      // 아이템 이름 (인스펙터에서 확인용)
}