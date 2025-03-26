// VillageInteractionManager.cs - 마을에서의 상호작용 관리
using UnityEngine;

public class VillageInteractionManager : MonoBehaviour
{
    [SerializeField] private GameObject statUpgradeUIPanel;

    // 다른 상호작용 UI들도 여기에 추가할 수 있음

    // 스탯 강화소 NPC와 상호작용 시 호출
    public void OpenStatUpgradeShop()
    {
        if (statUpgradeUIPanel != null)
        {
            StatUpgradeUI upgradeUI = statUpgradeUIPanel.GetComponent<StatUpgradeUI>();
            if (upgradeUI != null)
            {
                upgradeUI.ToggleUpgradePanel();
            }
        }
    }
}