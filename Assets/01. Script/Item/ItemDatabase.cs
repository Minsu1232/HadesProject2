// ItemDatabase.cs - ������ �����ͺ��̽� ScriptableObject
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<Item> allItems = new List<Item>();

    // ID�� ������ ã��
    public Item GetItemByID(string itemID)
    {
        return allItems.Find(item => item.itemID == itemID);
    }

    // ���� ID�� ���� ã��
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

    // ������ Ÿ�Ժ��� ���͸�
    public List<Item> GetItemsByType(Item.ItemType type)
    {
        return allItems.FindAll(item => item.itemType == type);
    }
}