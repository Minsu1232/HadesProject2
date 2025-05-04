using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VillageManager : MonoBehaviour
{
    [Header("Ʃ�丮�� ���� ������Ʈ")]
    [SerializeField] GameObject weaponTutorialTrigger;
    [SerializeField] GameObject combatTutorialTrigger;

    [Header("�ر� ������ ������Ʈ")]
    [SerializeField] GameObject timekeeperWorkshop;
    [SerializeField] GameObject timekeeperNPC;
    [SerializeField] GameObject statsUpgradeNPC;

    [Header("�ر� ���� ������Ʈ")]
    [SerializeField] GameObject chronofactuerWeapon;

    [Header("���̾�α� �Ŵ���")]
    [SerializeField] private VillageDialogManager dialogManager;

    private PlayerClass playerClass;

    private void Start()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        playerClass.SetInvicibleToFalse();
        // ���̾�α� �Ŵ��� ���� Ȯ��
        if (dialogManager == null)
        {
            dialogManager = GetComponent<VillageDialogManager>();
            if (dialogManager == null)
            {
                dialogManager = gameObject.AddComponent<VillageDialogManager>();
                Debug.Log("VillageDialogManager ������Ʈ�� �ڵ� �����Ǿ����ϴ�.");
            }
        }

        // SaveManager�� ���� ���� ���� Ȯ��
        if (SaveManager.Instance != null)
        {
            GameProgressManager.Instance.LoadProgress();
            PlayerSaveData playerData = SaveManager.Instance.GetPlayerData();

            // ���� �����׸�Ʈ ���� ���η� �۾��� �ر� ���� Ȯ��
            bool hasFragment = CheckBossFragmentOwnership(playerData);

            // ���̾�α� ǥ�� ���ο� ���� Ʃ�丮�� Ʈ���� ��Ȱ��ȭ
            DisableTutorialTriggersByShownDialogs(playerData.shownDialogs);

            // ��� Ƚ���� ���� ���� ���׷��̵� NPC Ȱ��ȭ
            UpdateStatsUpgradeNPC(playerData.deathCount);

            // �ر� ���¿� ���� ������ Ȱ��ȭ/��Ȱ��ȭ
            UpdateTimekeeperWorkshop(hasFragment);
        }

        // ���� Ŭ��� ���� ���� �ر� ó��
        UnlockWeaponsByBossDefeat();

        // �������� ��ȯ�ߴ��� Ȯ��
        if (PlayerPrefs.GetInt("ReturnFromDungeon", 0) == 1)
        {
            HandleReturnFromDungeon();
        }

        // ���̾�α� �Ŵ������� �رݵ� ������ ���� ����
        if (dialogManager != null)
        {
            dialogManager.SetUnlockedContent(
                chronofactuerWeapon != null && chronofactuerWeapon.activeInHierarchy,
                statsUpgradeNPC != null && statsUpgradeNPC.activeInHierarchy               
            );
        }

        // �������� �׻� �ɷ� �ʱ�ȭ
        playerClass.ResetPower(true, true, true, true, true, true, true, true);
        DungeonAbilityManager.Instance.ResetAllAbilities(); 
    }

    // ���� �����׸�Ʈ ���� ���� Ȯ��
    private bool CheckBossFragmentOwnership(PlayerSaveData playerData)
    {
        if (playerData == null) return false;

        // ������ �����׸�Ʈ Ȯ��
        bool hasFragment = playerData.equippedFragments.Contains(1001);

        // ���� �ȵ� ��� �κ��丮�� Ȯ��
        if (!hasFragment)
        {
            hasFragment = playerData.inventory.Any(item => item.itemID == 1001);
        }

        return hasFragment;
    }

    // Ÿ��Ű�� �۾��� �� NPC ������Ʈ
    private void UpdateTimekeeperWorkshop(bool hasFragment)
    {
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

    // ���� ���׷��̵� NPC ������Ʈ
    private void UpdateStatsUpgradeNPC(int deathCount)
    {
        if (statsUpgradeNPC != null)
        {
            bool shouldActivate = deathCount > 2;
            statsUpgradeNPC.SetActive(shouldActivate);           
            if (shouldActivate)
            {
                Debug.Log($"��� Ƚ��({deathCount})�� ���� ���� ���׷��̵� NPC Ȱ��ȭ");
            }
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
    }

    // ���� óġ�� ���� ���� �ر�
    private void UnlockWeaponsByBossDefeat()
    {
        // GameProgressManager���� ���� óġ �÷��� Ȯ��
        if (GameProgressManager.Instance != null)
        {
            // é�� 1 ���� óġ Ȯ�� => �÷��װ� ���� �۵�x
            bool chapter1BossDefeated = GameProgressManager.Instance.GetFlag("boss_defeated_chapter1");
            Debug.Log($"�˷���� óġ{chapter1BossDefeated}");

            // ù ���� óġ �� ũ�γ���ó ���� �ر�
            if (chapter1BossDefeated && chronofactuerWeapon != null)
            {
                chronofactuerWeapon.SetActive(true);
                Debug.Log("ù ���� óġ ����: ũ�γ���ó ���� �رݵ�");
            }
            //�ӽ÷� ����é�Ͱ� �رݵǾ������� ���� ����
            if (SaveManager.Instance.GetChapterData().IsChapterUnlocked("YongzokChapter"))
            {
                chronofactuerWeapon.SetActive(true);
                Debug.Log("ù ���� óġ ����: ũ�γ���ó ���� �رݵ�[�ӽù���]");
            }

            // ���� Ȯ��: �߰� é�� ���� ���� ó��
            // bool chapter2BossDefeated = GameProgressManager.Instance.GetFlag("boss_defeated_chapter2");
            // if (chapter2BossDefeated) { ... }
        }
    }

    // �������� ��ȯ ó��
    private void HandleReturnFromDungeon()
    {
        Debug.Log("�������� ������ ��ȯ: �ʱ�ȭ �۾� ����");
        List<FragmentItem> equippedFragments = new List<FragmentItem>();
        FragmentManager fragmentManager = FragmentManager.Instance;
        if (fragmentManager != null)
        {
            equippedFragments = fragmentManager.GetEquippedFragments();
        }
        // ���� ���� �� ���� ����
        ResetPlayerWeapon();

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

            // �ִϸ��̼� �ʱ�ȭ
            ResetPlayerAnimations(player);

            // CharacterAttackBase �ʱ�ȭ
            ResetCharacterAttack(player);
        }

      // ��� ���� Ȯ��
    bool isDeath = PlayerPrefs.GetInt("ReturnByDeath", 0) == 1;
    
    // ��� �� �÷��� �ʱ�ȭ
    PlayerPrefs.SetInt("ReturnFromDungeon", 0);
    PlayerPrefs.SetInt("ReturnByDeath", 0);

    // ����� ��쿡�� ���̾�α� ǥ��
        if (isDeath && SaveManager.Instance != null && dialogManager != null)
         {
        int deathCount = SaveManager.Instance.GetPlayerData().deathCount;
         dialogManager.ShowDeathDialog(deathCount);
     }
        if (fragmentManager != null && equippedFragments.Count > 0)
        {
            foreach (var fragment in equippedFragments)
            {
                fragmentManager.ApplyFragmentEffects(fragment);
            }
        }
    }

    // �÷��̾� ���� �ʱ�ȭ
    private void ResetPlayerWeapon()
    {
        if (GameInitializer.Instance == null) return;

        WeaponService weaponService = GameInitializer.Instance.GetWeaponService();
        if (weaponService != null)
        {
            // ���� ����
            if (weaponService.HasWeaponEquipped())
            {
                weaponService.UnequipCurrentWeapon();
                Debug.Log("�÷��̾� ���� ���� �Ϸ�");
            }

            // WeaponService���� ClearWeaponReferences �޼��� ȣ��
            if (weaponService.GetType().GetMethod("ClearWeaponReferences") != null)
            {
                weaponService.GetType().GetMethod("ClearWeaponReferences").Invoke(weaponService, null);
                Debug.Log("���� ���� �ʱ�ȭ �Ϸ�");
            }
        }
    }

    // CharacterAttackBase �ʱ�ȭ
    private void ResetCharacterAttack(GameObject player)
    {
        CharacterAttackBase attackBase = player.GetComponent<CharacterAttackBase>();
        if (attackBase != null)
        {
            // CharacterAttackBase���� UnequipWeapon �޼��� ȣ��
            if (attackBase.GetType().GetMethod("UnequipWeapon") != null)
            {
                attackBase.GetType().GetMethod("UnequipWeapon").Invoke(attackBase, null);
                Debug.Log("CharacterAttackBase �ʱ�ȭ �Ϸ�");
            }
        }
    }

    // �ִϸ��̼� �ʱ�ȭ
    private void ResetPlayerAnimations(GameObject player)
    {
        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            // ��� Ʈ���� ����
            playerAnimator.ResetTrigger("Attack");
            playerAnimator.ResetTrigger("ChargingAttack");
            playerAnimator.ResetTrigger("ReleaseCharge");
            playerAnimator.ResetTrigger("SpecialAttack");
            playerAnimator.ResetTrigger("Hit");
            playerAnimator.ResetTrigger("Die");

            // �⺻ ���·� ��ȯ
            playerAnimator.SetInteger("AttackCount", 0);
            playerAnimator.SetBool("IsStun", false);
            playerAnimator.Play("Idle", 0, 0f);

            Debug.Log("�÷��̾� �ִϸ��̼� ���� �ʱ�ȭ �Ϸ�");
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