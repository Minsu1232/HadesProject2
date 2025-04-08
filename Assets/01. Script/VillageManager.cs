using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// ���� ���� ����
/// </summary>
public class VillageManager : MonoBehaviour
{

    PlayerClass playerClass;

    [Header("Ʃ�丮�� ���� ������Ʈ")]
    [SerializeField] GameObject weaponTutorialTrigger; // ���� Ʃ�丮�� Ʈ����
    [SerializeField] GameObject combatTutorialTrigger; // ���� Ʃ�丮�� Ʈ����

    [Header("�ر� ������ ������Ʈ")]
    [SerializeField] GameObject timekeeperWorkshop;
    [SerializeField] GameObject timekeeperNPC;
    [SerializeField] GameObject StatsUpgradeNPC;
    
    // VillageManager�� Start �޼���
    private void Start()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        // ���� �����׸�Ʈ ���� ���η� �۾��� �ر� ���� Ȯ��
        bool hasFragment = false;

        // SaveManager�� ���� Ȯ��
        if (SaveManager.Instance != null)
        {
            GameProgressManager.Instance.LoadProgress();
            PlayerSaveData playerData = SaveManager.Instance.GetPlayerData();
            // ������ �����׸�Ʈ Ȯ��
            hasFragment = playerData.equippedFragments.Contains(1001);

            // ���� �ȵ� ��� �κ��丮�� Ȯ��
            if (!hasFragment)
            {
                hasFragment = playerData.inventory.Any(item => item.itemID == 1001);
            }
            // ǥ�õ� ���̾�α׿� ���� Ʃ�丮�� Ʈ���� ��Ȱ��ȭ
            DisableTutorialTriggersByShownDialogs(playerData.shownDialogs);
            if(playerData.deathCount > 2)
            {
                StatsUpgradeNPC.gameObject.SetActive(true);
            }
            else
            {
                StatsUpgradeNPC.gameObject.SetActive(false);
            }
        }

        UnequipAllWeapons();

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
    // ��� ���� ���� �޼���
    private void UnequipAllWeapons()
    {
        WeaponService weaponService = GameInitializer.Instance.GetWeaponService();

        if (weaponService != null && weaponService.HasWeaponEquipped())
        {
            // ���Ⱑ ������ ����
            // ����: WeaponService�� ���� ���� �޼��尡 �־�� ��
            weaponService.UnequipCurrentWeapon();
            Debug.Log("��� �� ��ȯ: ��� ���� ������");
        }
    }
    // ǥ�õ� ���̾�α׿� ���� Ʃ�丮�� Ʈ���� ��Ȱ��ȭ
    private void DisableTutorialTriggersByShownDialogs(List<string> shownDialogs)
    {
        if (shownDialogs == null) return;

        // ���� Ʃ�丮���� �̹� ������ �ش� Ʈ���� ��Ȱ��ȭ
        if (shownDialogs.Contains("weapon_tutorial") && weaponTutorialTrigger != null)
        {
            weaponTutorialTrigger.SetActive(false);
            Debug.Log("���� Ʃ�丮�� Ʈ���� ��Ȱ��ȭ (�̹� ǥ�õ�)");
        }

        // ���� Ʃ�丮���� �̹� ������ �ش� Ʈ���� ��Ȱ��ȭ
        if (shownDialogs.Contains("combat_tutorial") && combatTutorialTrigger != null)
        {
            combatTutorialTrigger.SetActive(false);
            Debug.Log("���� Ʃ�丮�� Ʈ���� ��Ȱ��ȭ (�̹� ǥ�õ�)");
        }

        // �ʿ信 ���� �ٸ� Ʃ�丮�� Ʈ���ŵ� ó��
        // ...
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
