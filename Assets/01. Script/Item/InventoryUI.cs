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
    [SerializeField] private Button fragmentTabButton; // 파편 탭으로 전환하는 버튼
    [SerializeField] private Button inventoryTabButton; // 인벤토리 탭으로 전환하는 버튼

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
    {// 인벤토리 시스템의 변경 이벤트 구독
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged += RefreshAllUI;
        }
        // 패널 초기 설정
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
        if (fragmentPanel != null) fragmentPanel.SetActive(false);
        if (itemDetailPanel != null) itemDetailPanel.SetActive(false);

        // 이벤트 구독
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged += RefreshAllUI;
            InventorySystem.Instance.OnFirstFragmentFound += OnFirstFragmentFound;
        }
        // 닫기 버튼에 이벤트 연결
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseDetailPanel);
        }
        // 버튼 이벤트 등록
        if (fragmentTabButton != null)
        {
            fragmentTabButton.onClick.AddListener(OnFragmentTabClick);
        }

        if (inventoryTabButton != null)
        {
            inventoryTabButton.onClick.AddListener(OnInventoryTabClick);
        }

        // 초기 UI 새로고침
        RefreshAllUI();
    }

    // 모든 UI 새로고침
    private void RefreshAllUI()
    {
        RefreshInventory();
        RefreshFragments();
    }

    // 인벤토리 탭 클릭
    public void OnInventoryTabClick()
    {
        fragmentPanel.SetActive(false);
        inventoryPanel.SetActive(true);
        if (itemDetailPanel != null) itemDetailPanel.SetActive(false);
    }

    // 파편 탭 클릭
    public void OnFragmentTabClick()
    {
        fragmentPanel.SetActive(true);
        inventoryPanel.SetActive(false);
       
        if (itemDetailPanel != null) itemDetailPanel.SetActive(false);
    }

    // 인벤토리 새로고침 - 파편 제외한 아이템만
    private void RefreshInventory()
    {
        if (InventorySystem.Instance == null) return;

        // 재료와 기타 아이템만 가져오기
        List<InventorySystem.ItemSlot> materialItems = InventorySystem.Instance.GetItemsByType(Item.ItemType.Material);
        List<InventorySystem.ItemSlot> miscItems = InventorySystem.Instance.GetItemsByType(Item.ItemType.Misc);

        List<InventorySystem.ItemSlot> allItems = new List<InventorySystem.ItemSlot>();
        allItems.AddRange(materialItems);
        allItems.AddRange(miscItems);

        // 슬롯 업데이트
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

    // 파편 패널 새로고침
    private void RefreshFragments()
    {
        if (InventorySystem.Instance == null) return;

        // 파편 아이템만 가져오기
        List<InventorySystem.ItemSlot> fragments = InventorySystem.Instance.GetItemsByType(Item.ItemType.Fragment);

        // 슬롯 업데이트
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

    // 일반 아이템 상세 정보 표시
    private void ShowItemDetails(Item item)
    {
        if (item == null || itemDetailPanel == null) return;

        selectedItem = item;

        if (itemIconImage != null) itemIconImage.sprite = item.icon;
        if (itemNameText != null) itemNameText.text = item.itemName;
        if (itemDescriptionText != null) itemDescriptionText.text = item.description;

        // 아이템 타입별 정보 표시
        if (itemStatsText != null)
        {
            if (item is MaterialItem material)
            {
                itemStatsText.text = $"재료 분류: {material.materialCategory}";
            }
            else
            {
                itemStatsText.text = $"종류: {item.itemType}";
            }
        }
        // 희귀도 표시 추가
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
            case Item.ItemRarity.Common: return "일반";
            case Item.ItemRarity.Uncommon: return "고급";
            case Item.ItemRarity.Rare: return "희귀";
            case Item.ItemRarity.Epic: return "에픽";
            case Item.ItemRarity.Legendary: return "전설";
            default: return "";
        }
    }

    // 희귀도별 색상 반환 메서드 추가
    private Color GetRarityColor(Item.ItemRarity rarity)
    {
        switch (rarity)
        {
            case Item.ItemRarity.Common: return new Color(0.7f, 0.7f, 0.7f); // 회색
            case Item.ItemRarity.Uncommon: return new Color(0.3f, 0.7f, 0.3f); // 녹색
            case Item.ItemRarity.Rare: return new Color(0.3f, 0.3f, 0.9f); // 파란색
            case Item.ItemRarity.Epic: return new Color(0.7f, 0.3f, 0.9f); // 보라색
            case Item.ItemRarity.Legendary: return new Color(1.0f, 0.8f, 0.0f); // 금색
            default: return Color.white;
        }
    }
    // 파편 상세 정보 표시
    private void ShowFragmentDetails(FragmentItem fragment)
    {
        if (fragment == null || itemDetailPanel == null) return;

        selectedItem = fragment;

        if (itemIconImage != null) itemIconImage.sprite = fragment.icon;
        if (itemNameText != null) itemNameText.text = fragment.itemName;
        if (itemDescriptionText != null) itemDescriptionText.text = fragment.description;

        // 희귀도 표시 추가 (if문 위치 수정)
        if (itemRarityText != null)
        {
            itemRarityText.text = GetRarityText(fragment.rarity);
            itemRarityText.color = GetRarityColor(fragment.rarity);
        }

        // 파편 스탯 정보 표시
        if (itemStatsText != null)
        {
            string statsText = "";

            // 공격력: 빨간색
            if (fragment.attackBonus > 0)
                statsText += $"<color=#FF5555>공격력: +{fragment.attackBonus}%</color>\n";

            // 방어력: 파란색
            if (fragment.defenseBonus > 0)
                statsText += $"<color=#5555FF>방어력: +{fragment.defenseBonus}%</color>\n";

            // 체력: 초록색
            if (fragment.healthBonus > 0)
                statsText += $"<color=#55FF55>체력: +{fragment.healthBonus}%</color>\n";

            // 이동속도: 노란색
            if (fragment.speedBonus > 0)
                statsText += $"<color=#FFFF55>이동속도: +{fragment.speedBonus}%</color>\n";

            // 공명: 보라색
            if (fragment.isResonated)
                statsText += "<color=#FF55FF>공명: 활성화</color>";
            else
                statsText += "<color=#AAAAAA>공명: 비활성화</color>";

            itemStatsText.text = statsText;
        }

        itemDetailPanel.SetActive(true);
    }

    // 첫 파편 발견 시 호출
    private void OnFirstFragmentFound()
    {
        if (fragmentTabButton != null && !fragmentTabButton.gameObject.activeSelf)
        {
            fragmentTabButton.gameObject.SetActive(true);
        }
    }

    // 상세 패널 닫기
    public void CloseDetailPanel()
    {
        if (itemDetailPanel != null)
        {
            itemDetailPanel.SetActive(false);
        }
    }
}