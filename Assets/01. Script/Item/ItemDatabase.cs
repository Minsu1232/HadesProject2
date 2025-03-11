// ItemDatabase.cs - 아이템 데이터베이스 ScriptableObject
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<Item> allItems = new List<Item>();

    // ID로 아이템 찾기
    public Item GetItemByID(string itemID)
    {
        return allItems.Find(item => item.itemID == itemID);
    }

    // 보스 ID로 파편 찾기
    public FragmentItem GetFragmentByBossID(string bossID)
    {
        foreach (Item item in allItems)
        {
            if (item is FragmentItem)
            {
                FragmentItem fragment = item as FragmentItem;
                if (fragment.associatedBossID == bossID)
                {
                    return fragment;
                }
            }
        }
        return null;
    }

    // 아이템 타입별로 필터링
    public List<Item> GetItemsByType(Item.ItemType type)
    {
        return allItems.FindAll(item => item.itemType == type);
    }
}