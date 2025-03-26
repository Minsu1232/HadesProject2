// StatUpgradeUI.cs - ���� ��ȭ UI
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatUpgradeUI : MonoBehaviour
{
    [Header("UI ����")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Transform statListContainer;
    [SerializeField] private GameObject statItemPrefab;

    [Header("�� ���� �г�")]
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
            Debug.LogError("StatUpgradeManager�� ã�� �� �����ϴ�!");
            return;
        }

        // �̺�Ʈ ����
        upgradeManager.OnStatUpgraded += OnStatUpgraded;

        // �ʱ� ����
        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        if (detailPanel != null)
            detailPanel.SetActive(false);

        // ���׷��̵� ��ư �̺�Ʈ
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeSelectedStat);

        // ���� ��� �ʱ�ȭ
        InitializeStatList();
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        if (upgradeManager != null)
            upgradeManager.OnStatUpgraded -= OnStatUpgraded;
    }

    // ���׷��̵� �г� ���
    public void ToggleUpgradePanel()
    {
        if (upgradePanel != null)
        {
            bool isActive = !upgradePanel.activeSelf;
            upgradePanel.SetActive(isActive);

            if (isActive)
            {
                // �г� �� �� ���� ��� ������Ʈ
                UpdateStatItems();

                // �� �г� �ݱ�
                if (detailPanel != null)
                    detailPanel.SetActive(false);
            }
        }
    }

    // ���� ��� �ʱ�ȭ
    private void InitializeStatList()
    {
        foreach (StatUpgradeManager.UpgradeableStatType statType in System.Enum.GetValues(typeof(StatUpgradeManager.UpgradeableStatType)))
        {
            // ��� 1: �θ� �� ���� ����
            GameObject statItemObj = Instantiate(statItemPrefab, statListContainer);

            // �Ǵ� ��� 2: �θ� ��������� ����
            // GameObject statItemObj = Instantiate(statItemPrefab);
            // statItemObj.transform.SetParent(statListContainer, false); // false: ���� ������ ����

            StatItemUI statItem = statItemObj.GetComponent<StatItemUI>();
            if (statItem != null)
            {
                statItem.Initialize(statType, GetStatName(statType), () => OnStatSelected(statType));
                statItems.Add(statItem);

                // �ʱ� ���� ������Ʈ
                UpdateStatItem(statItem);
            }
        }
    }

    // ���� ���׷��̵� �� ȣ��Ǵ� �޼���
    private void OnStatUpgraded(StatUpgradeManager.UpgradeableStatType statType, int newLevel)
    {
        // ���� ��� UI ������Ʈ
        UpdateStatItems();

        // ���� ���õ� ������ ���׷��̵�� �����̸� �� ������ ������Ʈ
        if (statType == selectedStat)
        {
            OnStatSelected(statType);
        }

        // ȿ���� �� ��ƼŬ ���
        PlayUpgradeEffect();
    }

    // ��� ���� �׸� ������Ʈ
    private void UpdateStatItems()
    {
        foreach (var statItem in statItems)
        {
            UpdateStatItem(statItem);
        }
    }

    // ���� ���� �׸� ������Ʈ
    private void UpdateStatItem(StatItemUI statItem)
    {
        var statType = statItem.StatType;
        int currentLevel = upgradeManager.GetCurrentLevel(statType);
        int maxLevel = upgradeManager.GetMaxLevel(statType);
        bool canUpgrade = upgradeManager.CanUpgradeStat(statType);

        statItem.UpdateUI(currentLevel, maxLevel, canUpgrade);
    }

    // ���� ���� �� ȣ��
    private void OnStatSelected(StatUpgradeManager.UpgradeableStatType statType)
    {
        selectedStat = statType;

        if (detailPanel != null)
        {
            detailPanel.SetActive(true);

            // ���� �̸�
            if (statNameText != null)
                statNameText.text = GetStatName(statType);

            // ���� ����
            int currentLevel = upgradeManager.GetCurrentLevel(statType);
            int maxLevel = upgradeManager.GetMaxLevel(statType);
            if (statLevelText != null)
                statLevelText.text = $"����: {currentLevel}/{maxLevel}";

            // ���� ȿ�� ����
            if (statEffectText != null)
                statEffectText.text = upgradeManager.GetStatUpgradeDescription(statType);

            // �ʿ� ��� ǥ��
            UpdateMaterialsList(statType);

            // ���׷��̵� ��ư ���� ������Ʈ
            if (upgradeButton != null)
                upgradeButton.interactable = upgradeManager.CanUpgradeStat(statType);
        }
    }

    // �ʿ� ��� ��� ������Ʈ
    private void UpdateMaterialsList(StatUpgradeManager.UpgradeableStatType statType)
    {
        if (materialsContainer == null) return;

        // ���� ��� �����
        foreach (Transform child in materialsContainer)
        {
            Destroy(child.gameObject);
        }

        // �ִ� �������� Ȯ��
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

        // �ʿ� ��� ǥ��
        List<MaterialCost> costs = upgradeManager.GetUpgradeCost(statType);

        foreach (var cost in costs)
        {
            GameObject materialObj = Instantiate(materialItemPrefab, materialsContainer);
            MaterialItemUI materialItem = materialObj.GetComponent<MaterialItemUI>();

            if (materialItem != null)
            {
                // ������ ���� ��������
                Item item = ItemDataManager.Instance.GetItem(cost.ItemID);

                if (item != null)
                {
                    // ���� ���� Ȯ��
                    int ownedQuantity = GetOwnedItemQuantity(cost.ItemID);

                    // UI ������Ʈ
                    materialItem.SetMaterial(item, cost.RequiredQuantity, ownedQuantity);
                }
                else
                {
                    Debug.LogWarning($"������ ID {cost.ItemID}�� ã�� �� �����ϴ�.");
                }
            }
        }
    }

    // ������ ������ ���� Ȯ��
    private int GetOwnedItemQuantity(int itemId)
    {
        InventorySystem.ItemSlot slot = InventorySystem.Instance.FindItemSlot(itemId);
        return slot != null ? slot.quantity : 0;
    }

    // ������ ���� ���׷��̵�
    private void UpgradeSelectedStat()
    {
        upgradeManager.UpgradeStat(selectedStat);
    }

    // ���� �̸� ��������
    private string GetStatName(StatUpgradeManager.UpgradeableStatType statType)
    {
        switch (statType)
        {
            case StatUpgradeManager.UpgradeableStatType.AttackPower:
                return "�ı��� ����";
            case StatUpgradeManager.UpgradeableStatType.Health:
                return "�Ҹ��� ����";
            case StatUpgradeManager.UpgradeableStatType.Speed:
                return "�ð��� ����";
            default:
                return statType.ToString();
        }
    }

    // ���׷��̵� ȿ�� ���
    private void PlayUpgradeEffect()
    {
        // TODO: ���׷��̵� ȿ�� (����, ��ƼŬ ��)
        Debug.Log("���׷��̵� ȿ�� ���");
    }
}