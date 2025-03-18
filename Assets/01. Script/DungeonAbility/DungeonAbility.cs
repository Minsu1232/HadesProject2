// 1. 패시브 능력 기본 클래스 (던전 내에서만 적용되는 능력)
using UnityEngine;

[System.Serializable]
public abstract class DungeonAbility
{
    public string id;             // 고유 식별자
    public string name;           // 이름
    public string description;    // 설명
    public Sprite icon;           // 아이콘
    public Rarity rarity;         // 희귀도

    public int level = 1;         // 능력 레벨

    // 이 능력이 처음 획득될 때 호출
    public abstract void OnAcquire(PlayerClass player);

    // 이 능력이 레벨업될 때 호출
    public abstract void OnLevelUp(PlayerClass player);

    // 던전에서 나갈 때 호출 (능력 초기화)
    public abstract void OnReset(PlayerClass player);
}

// 희귀도 enum
public enum Rarity
{
    Common,     // 일반
    Uncommon,   // 고급
    Rare,       // 희귀
    Epic,       // 에픽
    Legendary   // 전설
}