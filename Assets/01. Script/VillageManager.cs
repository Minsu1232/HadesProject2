using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VillageManager : MonoBehaviour
{
   [SerializeField] GameObject timekeeperWorkshop;
    [SerializeField]
    GameObject timekeeperNPC;
    // VillageManager�� Start �޼���
    private void Start()
    {
        // ���� �����׸�Ʈ ���� ���η� �۾��� �ر� ���� Ȯ��
        bool hasFragment = false;

        // SaveManager�� ���� Ȯ��
        if (SaveManager.Instance != null)
        {
            PlayerSaveData playerData = SaveManager.Instance.GetPlayerData();
            // ������ �����׸�Ʈ Ȯ��
            hasFragment = playerData.equippedFragments.Contains(1001);

            // ���� �ȵ� ��� �κ��丮�� Ȯ��
            if (!hasFragment)
            {
                hasFragment = playerData.inventory.Any(item => item.itemID == 1001);
            }
        }
   

        // �ر� ���¿� ���� Ȱ��ȭ/��Ȱ��ȭ
        if (timekeeperWorkshop != null)
        {
            timekeeperWorkshop.SetActive(hasFragment);
        }

        if (timekeeperNPC != null)
        {
            timekeeperNPC.SetActive(hasFragment);
        }

        Debug.Log($"Ÿ��Ű�� �۾��� �ر� ����: {hasFragment} (���� �����׸�Ʈ ���� ����)");
    }
}
