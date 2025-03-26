// StatUpgradeUI.cs - 스탯 강화 UI
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatUpgradeUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Transform statListContainer;
    [SerializeField] private GameObject statItemPrefab;

    [Header("상세 정보 패널")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private TextMeshProUGUI statNameText;
    [SerializeField] private TextMeshProUGUI statLevelText;
    [SerializeField] private TextMeshProUGUI statEffectText;
    [SerializeField] private Transform materialsContainer;
    [SerializeField] private GameObject materialItemPrefab;
    [SerializeField] private Button upgradeButton;

    private StatUpgradeManager upgradeManager;
    private StatUpgradeManager.UpgradeableStatType selectedStat;
    private List<StatItemUI> statItems = new List<StatItemUI>();

    private void Start()
    {
        upgradeManager = StatUpgradeManager.Instance;

        if (upgradeManager == null)
        {
            Debug.LogError("StatUpgradeManager를 찾을 수 없습니다!");
            return;
        }

        // 이벤트 구독
        upgradeManager.OnStatUpgraded += OnStatUpgraded;

        // 초기 설정
        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        if (detailPanel != null)
            detailPanel.SetActive(false);

        // 업그레이드 버튼 이벤트
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeSelectedStat);

        // 스탯 목록 초기화
        InitializeStatList();
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (upgradeManager != null)
            upgradeManager.OnStatUpgraded -= OnStatUpgraded;
    }

    // 업그레이드 패널 토글
    public void ToggleUpgradePanel()
    {
        if (upgradePanel != null)
        {
            bool isActive = !upgradePanel.activeSelf;
            upgradePanel.SetActive(isActive);

            if (isActive)
            {
                // 패널 열 때 스탯 목록 업데이트
                UpdateStatItems();

                // 상세 패널 닫기
                if (detailPanel != null)
                    detailPanel.SetActive(false);
            }
        }
    }

    // 스탯 목록 초기화
    private void InitializeStatList()
    {
        foreach (StatUpgradeManager.UpgradeableStatType statType in System.Enum.GetValues(typeof(StatUpgradeManager.UpgradeableStatType)))
        {
            // 방법 1: 부모를 한 번만 설정
            GameObject statItemObj = Instantiate(statItemPrefab, statListContainer);

            // 또는 방법 2: 부모를 명시적으로 설정
            // GameObject statItemObj = Instantiate(statItemPrefab);
            // statItemObj.transform.SetParent(statListContainer, false); // false: 로컬 스케일 유지

            StatItemUI statItem = statItemObj.GetComponent<StatItemUI>();
            if (statItem != null)
            {
                statItem.Initialize(statType, GetStatName(statType), () => OnStatSelected(statType));
                statItems.Add(statItem);

                // 초기 상태 업데이트
                UpdateStatItem(statItem);
            }
        }
    }

    // 스탯 업그레이드 시 호출되는 메서드
    private void OnStatUpgraded(StatUpgradeManager.UpgradeableStatType statType, int newLevel)
    {
        // 스탯 목록 UI 업데이트
        UpdateStatItems();

        // 현재 선택된 스탯이 업그레이드된 스탯이면 상세 정보도 업데이트
        if (statType == selectedStat)
        {
            OnStatSelected(statType);
        }

        // 효과음 및 파티클 재생
        PlayUpgradeEffect();
    }

    // 모든 스탯 항목 업데이트
    private void UpdateStatItems()
    {
        foreach (var statItem in statItems)
        {
            UpdateStatItem(statItem);
        }
    }

    // 개별 스탯 항목 업데이트
    private void UpdateStatItem(StatItemUI statItem)
    {
        var statType = statItem.StatType;
        int currentLevel = upgradeManager.GetCurrentLevel(statType);
        int maxLevel = upgradeManager.GetMaxLevel(statType);
        bool canUpgrade = upgradeManager.CanUpgradeStat(statType);

        statItem.UpdateUI(currentLevel, maxLevel, canUpgrade);
    }

    // 스탯 선택 시 호출
    private void OnStatSelected(StatUpgradeManager.UpgradeableStatType statType)
    {
        selectedStat = statType;

        if (detailPanel != null)
        {
            detailPanel.SetActive(true);

            // 스탯 이름
            if (statNameText != null)
                statNameText.text = GetStatName(statType);

            // 스탯 레벨
            int currentLevel = upgradeManager.GetCurrentLevel(statType);
            int maxLevel = upgradeManager.GetMaxLevel(statType);
            if (statLevelText != null)
                statLevelText.text = $"레벨: {currentLevel}/{maxLevel}";

            // 스탯 효과 설명
            if (statEffectText != null)
                statEffectText.text = upgradeManager.GetStatUpgradeDescription(statType);

            // 필요 재료 표시
            UpdateMaterialsList(statType);

            // 업그레이드 버튼 상태 업데이트
            if (upgradeButton != null)
                upgradeButton.interactable = upgradeManager.CanUpgradeStat(statType);
        }
    }

    // 필요 재료 목록 업데이트
    private void UpdateMaterialsList(StatUpgradeManager.UpgradeableStatType statType)
    {
        if (materialsContainer == null) return;

        // 기존 목록 지우기
        foreach (Transform child in materialsContainer)
        {
            Destroy(child.gameObject);
        }

        // 최대 레벨인지 확인
        int currentLevel = upgradeManager.GetCurrentLevel(statType);
        int maxLevel = upgradeManager.GetMaxLevel(statType);

        if (currentLevel >= maxLevel)
        {
            GameObject maxLevelObj = Instantiate(materialItemPrefab, materialsContainer);
            MaterialItemUI materialItem = maxLevelObj.GetComponent<MaterialItemUI>();

            if (materialItem != null)
            {
                materialItem.SetMaxLevel();
            }
            return;
        }

        // 필요 재료 표시
        List<MaterialCost> costs = upgradeManager.GetUpgradeCost(statType);

        foreach (var cost in costs)
        {
            GameObject materialObj = Instantiate(materialItemPrefab, materialsContainer);
            MaterialItemUI materialItem = materialObj.GetComponent<MaterialItemUI>();

            if (materialItem != null)
            {
                // 아이템 정보 가져오기
                Item item = ItemDataManager.Instance.GetItem(cost.ItemID);

                if (item != null)
                {
                    // 보유 수량 확인
                    int ownedQuantity = GetOwnedItemQuantity(cost.ItemID);

                    // UI 업데이트
                    materialItem.SetMaterial(item, cost.RequiredQuantity, ownedQuantity);
                }
                else
                {
                    Debug.LogWarning($"아이템 ID {cost.ItemID}를 찾을 수 없습니다.");
                }
            }
        }
    }

    // 보유한 아이템 수량 확인
    private int GetOwnedItemQuantity(int itemId)
    {
        InventorySystem.ItemSlot slot = InventorySystem.Instance.FindItemSlot(itemId);
        return slot != null ? slot.quantity : 0;
    }

    // 선택한 스탯 업그레이드
    private void UpgradeSelectedStat()
    {
        upgradeManager.UpgradeStat(selectedStat);
    }

    // 스탯 이름 가져오기
    private string GetStatName(StatUpgradeManager.UpgradeableStatType statType)
    {
        switch (statType)
        {
            case StatUpgradeManager.UpgradeableStatType.AttackPower:
                return "파괴의 파편";
            case StatUpgradeManager.UpgradeableStatType.Health:
                return "불멸의 심장";
            case StatUpgradeManager.UpgradeableStatType.Speed:
                return "시간의 가속";
            default:
                return statType.ToString();
        }
    }

    // 업그레이드 효과 재생
    private void PlayUpgradeEffect()
    {
        // TODO: 업그레이드 효과 (사운드, 파티클 등)
        Debug.Log("업그레이드 효과 재생");
    }
}