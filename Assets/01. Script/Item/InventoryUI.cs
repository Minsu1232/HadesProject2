// InventoryUI.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject fragmentPanel;

    [Header("Inventory Items")]
    [SerializeField] private List<ItemSlotUI> itemSlots = new List<ItemSlotUI>();

    [Header("Fragment Items")]
    [SerializeField] private List<FragmentSlotUI> fragmentSlots = new List<FragmentSlotUI>();

    [Header("Category Buttons")]
    [SerializeField] private Button fragmentTabButton; // ���� ������ ��ȯ�ϴ� ��ư
    [SerializeField] private Button inventoryTabButton; // �κ��丮 ������ ��ȯ�ϴ� ��ư

    [Header("Item Details")]
    [SerializeField] private GameObject itemDetailPanel;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemRarityText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI itemStatsText;
    [SerializeField] private Button closeButton;

    private Item selectedItem;

    private void Start()
    {// �κ��丮 �ý����� ���� �̺�Ʈ ����
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged += RefreshAllUI;
        }
        // �г� �ʱ� ����
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
        if (fragmentPanel != null) fragmentPanel.SetActive(false);
        if (itemDetailPanel != null) itemDetailPanel.SetActive(false);

        // �̺�Ʈ ����
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged += RefreshAllUI;
            InventorySystem.Instance.OnFirstFragmentFound += OnFirstFragmentFound;
        }
        // �ݱ� ��ư�� �̺�Ʈ ����
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseDetailPanel);
        }
        // ��ư �̺�Ʈ ���
        if (fragmentTabButton != null)
        {
            fragmentTabButton.onClick.AddListener(OnFragmentTabClick);
        }

        if (inventoryTabButton != null)
        {
            inventoryTabButton.onClick.AddListener(OnInventoryTabClick);
        }

        // �ʱ� UI ���ΰ�ħ
        RefreshAllUI();
    }

    // ��� UI ���ΰ�ħ
    private void RefreshAllUI()
    {
        RefreshInventory();
        RefreshFragments();
    }

    // �κ��丮 �� Ŭ��
    public void OnInventoryTabClick()
    {
        fragmentPanel.SetActive(false);
        inventoryPanel.SetActive(true);
        if (itemDetailPanel != null) itemDetailPanel.SetActive(false);
    }

    // ���� �� Ŭ��
    public void OnFragmentTabClick()
    {
        fragmentPanel.SetActive(true);
        inventoryPanel.SetActive(false);
       
        if (itemDetailPanel != null) itemDetailPanel.SetActive(false);
    }

    // �κ��丮 ���ΰ�ħ - ���� ������ �����۸�
    private void RefreshInventory()
    {
        if (InventorySystem.Instance == null) return;

        // ���� ��Ÿ �����۸� ��������
        List<InventorySystem.ItemSlot> materialItems = InventorySystem.Instance.GetItemsByType(Item.ItemType.Material);
        List<InventorySystem.ItemSlot> miscItems = InventorySystem.Instance.GetItemsByType(Item.ItemType.Misc);

        List<InventorySystem.ItemSlot> allItems = new List<InventorySystem.ItemSlot>();
        allItems.AddRange(materialItems);
        allItems.AddRange(miscItems);

        // ���� ������Ʈ
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (i < allItems.Count)
            {
                itemSlots[i].SetItem(allItems[i].item, allItems[i].quantity);
                int index = i;
                itemSlots[i].SetClickCallback(() => ShowItemDetails(allItems[index].item));
            }
            else
            {
                itemSlots[i].SetItem(null, 0);
                itemSlots[i].SetClickCallback(null);
            }
        }
    }

    // ���� �г� ���ΰ�ħ
    private void RefreshFragments()
    {
        if (InventorySystem.Instance == null) return;

        // ���� �����۸� ��������
        List<InventorySystem.ItemSlot> fragments = InventorySystem.Instance.GetItemsByType(Item.ItemType.Fragment);

        // ���� ������Ʈ
        for (int i = 0; i < fragmentSlots.Count; i++)
        {
            if (i < fragments.Count)
            {
                fragmentSlots[i].SetFragment(fragments[i].item as FragmentItem);
                int index = i;
                fragmentSlots[i].SetClickCallback(() => ShowFragmentDetails(fragments[index].item as FragmentItem));
            }
            else
            {
                fragmentSlots[i].SetFragment(null);
                fragmentSlots[i].SetClickCallback(null);
            }
        }
    }

    // �Ϲ� ������ �� ���� ǥ��
    private void ShowItemDetails(Item item)
    {
        if (item == null || itemDetailPanel == null) return;

        selectedItem = item;

        if (itemIconImage != null) itemIconImage.sprite = item.icon;
        if (itemNameText != null) itemNameText.text = item.itemName;
        if (itemDescriptionText != null) itemDescriptionText.text = item.description;

        // ������ Ÿ�Ժ� ���� ǥ��
        if (itemStatsText != null)
        {
            if (item is MaterialItem material)
            {
                itemStatsText.text = $"��� �з�: {material.materialCategory}";
            }
            else
            {
                itemStatsText.text = $"����: {item.itemType}";
            }
        }
        // ��͵� ǥ�� �߰�
        if (itemRarityText != null)
        {
            itemRarityText.text = GetRarityText(item.rarity);
            itemRarityText.color = GetRarityColor(item.rarity);
        }

        itemDetailPanel.SetActive(true);
    }
    private string GetRarityText(Item.ItemRarity rarity)
    {
        switch (rarity)
        {
            case Item.ItemRarity.Common: return "�Ϲ�";
            case Item.ItemRarity.Uncommon: return "���";
            case Item.ItemRarity.Rare: return "���";
            case Item.ItemRarity.Epic: return "����";
            case Item.ItemRarity.Legendary: return "����";
            default: return "";
        }
    }

    // ��͵��� ���� ��ȯ �޼��� �߰�
    private Color GetRarityColor(Item.ItemRarity rarity)
    {
        switch (rarity)
        {
            case Item.ItemRarity.Common: return new Color(0.7f, 0.7f, 0.7f); // ȸ��
            case Item.ItemRarity.Uncommon: return new Color(0.3f, 0.7f, 0.3f); // ���
            case Item.ItemRarity.Rare: return new Color(0.3f, 0.3f, 0.9f); // �Ķ���
            case Item.ItemRarity.Epic: return new Color(0.7f, 0.3f, 0.9f); // �����
            case Item.ItemRarity.Legendary: return new Color(1.0f, 0.8f, 0.0f); // �ݻ�
            default: return Color.white;
        }
    }
    // ���� �� ���� ǥ��
    private void ShowFragmentDetails(FragmentItem fragment)
    {
        if (fragment == null || itemDetailPanel == null) return;

        selectedItem = fragment;

        if (itemIconImage != null) itemIconImage.sprite = fragment.icon;
        if (itemNameText != null) itemNameText.text = fragment.itemName;
        if (itemDescriptionText != null) itemDescriptionText.text = fragment.description;

        // ��͵� ǥ�� �߰� (if�� ��ġ ����)
        if (itemRarityText != null)
        {
            itemRarityText.text = GetRarityText(fragment.rarity);
            itemRarityText.color = GetRarityColor(fragment.rarity);
        }

        // ���� ���� ���� ǥ��
        if (itemStatsText != null)
        {
            string statsText = "";

            // ���ݷ�: ������
            if (fragment.attackBonus > 0)
                statsText += $"<color=#FF5555>���ݷ�: +{fragment.attackBonus}%</color>\n";

            // ����: �Ķ���
            if (fragment.defenseBonus > 0)
                statsText += $"<color=#5555FF>����: +{fragment.defenseBonus}%</color>\n";

            // ü��: �ʷϻ�
            if (fragment.healthBonus > 0)
                statsText += $"<color=#55FF55>ü��: +{fragment.healthBonus}%</color>\n";

            // �̵��ӵ�: �����
            if (fragment.speedBonus > 0)
                statsText += $"<color=#FFFF55>�̵��ӵ�: +{fragment.speedBonus}%</color>\n";

            // ����: �����
            if (fragment.isResonated)
                statsText += "<color=#FF55FF>����: Ȱ��ȭ</color>";
            else
                statsText += "<color=#AAAAAA>����: ��Ȱ��ȭ</color>";

            itemStatsText.text = statsText;
        }

        itemDetailPanel.SetActive(true);
    }

    // ù ���� �߰� �� ȣ��
    private void OnFirstFragmentFound()
    {
        if (fragmentTabButton != null && !fragmentTabButton.gameObject.activeSelf)
        {
            fragmentTabButton.gameObject.SetActive(true);
        }
    }

    // �� �г� �ݱ�
    public void CloseDetailPanel()
    {
        if (itemDetailPanel != null)
        {
            itemDetailPanel.SetActive(false);
        }
    }
}