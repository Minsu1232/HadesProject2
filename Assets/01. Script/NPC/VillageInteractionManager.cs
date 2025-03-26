// VillageInteractionManager.cs - ���������� ��ȣ�ۿ� ����
using UnityEngine;

public class VillageInteractionManager : MonoBehaviour
{
    [SerializeField] private GameObject statUpgradeUIPanel;

    // �ٸ� ��ȣ�ۿ� UI�鵵 ���⿡ �߰��� �� ����

    // ���� ��ȭ�� NPC�� ��ȣ�ۿ� �� ȣ��
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