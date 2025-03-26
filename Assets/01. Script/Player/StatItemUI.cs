// StatItemUI.cs - 스탯 목록 항목 UI
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image upgradeAvailableIcon;
    [SerializeField] private Button button;

    private StatUpgradeManager.UpgradeableStatType statType;
    private Action onSelect;

    public StatUpgradeManager.UpgradeableStatType StatType => statType;

    public void Initialize(StatUpgradeManager.UpgradeableStatType type, string name, Action onSelectCallback)
    {
        statType = type;
        onSelect = onSelectCallback;

        if (statNameText != null)
            statNameText.text = name;

        if (button != null)
            button.onClick.AddListener(() => onSelect?.Invoke());
    }

    public void UpdateUI(int currentLevel, int maxLevel, bool canUpgrade)
    {
        if (levelText != null)
            levelText.text = $"{currentLevel}/{maxLevel}";

        if (upgradeAvailableIcon != null)
            upgradeAvailableIcon.gameObject.SetActive(canUpgrade);
    }
}