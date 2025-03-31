using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VillageManager : MonoBehaviour
{
   [SerializeField] GameObject timekeeperWorkshop;
    [SerializeField]
    GameObject timekeeperNPC;
    PlayerClass playerClass;
    // VillageManager�� Start �޼���
    private void Start()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
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

        // �������� ��ȯ�ߴ��� Ȯ��
        if (PlayerPrefs.GetInt("ReturnFromDungeon", 0) == 1)
        {
            // �÷��̾� ã��
            GameObject player = GameInitializer.Instance.gameObject;
            if (player != null)
            {
                // ����� ��ġ�� �÷��̾� �̵�
                player.transform.position = new Vector3(-6.6f, 0.1f, -20f);
                
                // ���� ������ Ȱ��ȭ�Ǿ� �ִٸ� ��� ���� ��ġ ����
                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    StartCoroutine(SetPlayerPosition(rb));
                }
            }
            
           

            // ��� �� �÷��� �ʱ�ȭ
            PlayerPrefs.SetInt("ReturnFromDungeon", 0);
        }

    }
    private IEnumerator SetPlayerPosition(Rigidbody rb)
    {
        // ���� ���� �ӽ� ��Ȱ��ȭ
        bool wasKinematic = rb.isKinematic;
        rb.isKinematic = true;

        yield return new WaitForSeconds(0.2f);

        // ���� ���·� ����
        rb.isKinematic = wasKinematic;
    }
}
