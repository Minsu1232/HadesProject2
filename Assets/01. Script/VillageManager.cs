using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VillageManager : MonoBehaviour
{
    [Header("튜토리얼 관련 오브젝트")]
    [SerializeField] GameObject weaponTutorialTrigger;
    [SerializeField] GameObject combatTutorialTrigger;

    [Header("해금 컨텐츠 오브젝트")]
    [SerializeField] GameObject timekeeperWorkshop;
    [SerializeField] GameObject timekeeperNPC;
    [SerializeField] GameObject statsUpgradeNPC;

    [Header("해금 무기 오브젝트")]
    [SerializeField] GameObject chronofactuerWeapon;

    [Header("다이얼로그 매니저")]
    [SerializeField] private VillageDialogManager dialogManager;

    private PlayerClass playerClass;

    private void Start()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        playerClass.SetInvicibleToFalse();
        // 다이얼로그 매니저 참조 확인
        if (dialogManager == null)
        {
            dialogManager = GetComponent<VillageDialogManager>();
            if (dialogManager == null)
            {
                dialogManager = gameObject.AddComponent<VillageDialogManager>();
                Debug.Log("VillageDialogManager 컴포넌트가 자동 생성되었습니다.");
            }
        }

        // SaveManager를 통해 진행 상태 확인
        if (SaveManager.Instance != null)
        {
            GameProgressManager.Instance.LoadProgress();
            PlayerSaveData playerData = SaveManager.Instance.GetPlayerData();

            // 보스 프래그먼트 소유 여부로 작업실 해금 상태 확인
            bool hasFragment = CheckBossFragmentOwnership(playerData);

            // 다이얼로그 표시 여부에 따라 튜토리얼 트리거 비활성화
            DisableTutorialTriggersByShownDialogs(playerData.shownDialogs);

            // 사망 횟수에 따라 스탯 업그레이드 NPC 활성화
            UpdateStatsUpgradeNPC(playerData.deathCount);

            // 해금 상태에 따라 컨텐츠 활성화/비활성화
            UpdateTimekeeperWorkshop(hasFragment);
        }

        // 보스 클리어에 따른 무기 해금 처리
        UnlockWeaponsByBossDefeat();

        // 던전에서 귀환했는지 확인
        if (PlayerPrefs.GetInt("ReturnFromDungeon", 0) == 1)
        {
            HandleReturnFromDungeon();
        }

        // 다이얼로그 매니저에게 해금된 컨텐츠 정보 전달
        if (dialogManager != null)
        {
            dialogManager.SetUnlockedContent(
                chronofactuerWeapon != null && chronofactuerWeapon.activeInHierarchy,
                statsUpgradeNPC != null && statsUpgradeNPC.activeInHierarchy               
            );
        }

        // 마을에선 항상 능력 초기화
        playerClass.ResetPower(true, true, true, true, true, true, true, true);
        DungeonAbilityManager.Instance.ResetAllAbilities(); 
    }

    // 보스 프래그먼트 소유 여부 확인
    private bool CheckBossFragmentOwnership(PlayerSaveData playerData)
    {
        if (playerData == null) return false;

        // 장착된 프래그먼트 확인
        bool hasFragment = playerData.equippedFragments.Contains(1001);

        // 장착 안된 경우 인벤토리도 확인
        if (!hasFragment)
        {
            hasFragment = playerData.inventory.Any(item => item.itemID == 1001);
        }

        return hasFragment;
    }

    // 타임키퍼 작업실 및 NPC 업데이트
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

        Debug.Log($"타임키퍼 작업실 해금 상태: {hasFragment} (보스 프래그먼트 보유 여부)");
    }

    // 스탯 업그레이드 NPC 업데이트
    private void UpdateStatsUpgradeNPC(int deathCount)
    {
        if (statsUpgradeNPC != null)
        {
            bool shouldActivate = deathCount > 2;
            statsUpgradeNPC.SetActive(shouldActivate);           
            if (shouldActivate)
            {
                Debug.Log($"사망 횟수({deathCount})에 따라 스탯 업그레이드 NPC 활성화");
            }
        }
    }

    // 표시된 다이얼로그에 따라 튜토리얼 트리거 비활성화
    private void DisableTutorialTriggersByShownDialogs(List<string> shownDialogs)
    {
        if (shownDialogs == null) return;

        // 무기 튜토리얼을 이미 봤으면 해당 트리거 비활성화
        if (shownDialogs.Contains("weapon_tutorial") && weaponTutorialTrigger != null)
        {
            weaponTutorialTrigger.SetActive(false);
            Debug.Log("무기 튜토리얼 트리거 비활성화 (이미 표시됨)");
        }

        // 전투 튜토리얼을 이미 봤으면 해당 트리거 비활성화
        if (shownDialogs.Contains("combat_tutorial") && combatTutorialTrigger != null)
        {
            combatTutorialTrigger.SetActive(false);
            Debug.Log("전투 튜토리얼 트리거 비활성화 (이미 표시됨)");
        }
    }

    // 보스 처치에 따른 무기 해금
    private void UnlockWeaponsByBossDefeat()
    {
        // GameProgressManager에서 보스 처치 플래그 확인
        if (GameProgressManager.Instance != null)
        {
            // 챕터 1 보스 처치 확인 => 플래그가 현재 작동x
            bool chapter1BossDefeated = GameProgressManager.Instance.GetFlag("boss_defeated_chapter1");
            Debug.Log($"알렉산더 처치{chapter1BossDefeated}");

            // 첫 보스 처치 시 크로노팩처 무기 해금
            if (chapter1BossDefeated && chronofactuerWeapon != null)
            {
                chronofactuerWeapon.SetActive(true);
                Debug.Log("첫 보스 처치 보상: 크로노팩처 무기 해금됨");
            }
            //임시로 용족챕터가 해금되어있으면 무기 오픈
            if (SaveManager.Instance.GetChapterData().IsChapterUnlocked("YongzokChapter"))
            {
                chronofactuerWeapon.SetActive(true);
                Debug.Log("첫 보스 처치 보상: 크로노팩처 무기 해금됨[임시방편]");
            }

            // 추후 확장: 추가 챕터 보스 보상 처리
            // bool chapter2BossDefeated = GameProgressManager.Instance.GetFlag("boss_defeated_chapter2");
            // if (chapter2BossDefeated) { ... }
        }
    }

    // 던전에서 귀환 처리
    private void HandleReturnFromDungeon()
    {
        Debug.Log("던전에서 마을로 귀환: 초기화 작업 시작");
        List<FragmentItem> equippedFragments = new List<FragmentItem>();
        FragmentManager fragmentManager = FragmentManager.Instance;
        if (fragmentManager != null)
        {
            equippedFragments = fragmentManager.GetEquippedFragments();
        }
        // 무기 해제 및 참조 정리
        ResetPlayerWeapon();

        // 플레이어 찾기
        GameObject player = GameInitializer.Instance.gameObject;
        
        if (player != null)
        {
            // 저장된 위치로 플레이어 이동
            player.transform.position = new Vector3(-6.6f, 0.1f, -20f);

            // 물리 엔진이 활성화되어 있다면 잠시 끄고 위치 설정
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                StartCoroutine(SetPlayerPosition(rb));
            }

            // 애니메이션 초기화
            ResetPlayerAnimations(player);

            // CharacterAttackBase 초기화
            ResetCharacterAttack(player);
        }

      // 사망 여부 확인
    bool isDeath = PlayerPrefs.GetInt("ReturnByDeath", 0) == 1;
    
    // 사용 후 플래그 초기화
    PlayerPrefs.SetInt("ReturnFromDungeon", 0);
    PlayerPrefs.SetInt("ReturnByDeath", 0);

    // 사망한 경우에만 다이얼로그 표시
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

    // 플레이어 무기 초기화
    private void ResetPlayerWeapon()
    {
        if (GameInitializer.Instance == null) return;

        WeaponService weaponService = GameInitializer.Instance.GetWeaponService();
        if (weaponService != null)
        {
            // 무기 해제
            if (weaponService.HasWeaponEquipped())
            {
                weaponService.UnequipCurrentWeapon();
                Debug.Log("플레이어 무기 해제 완료");
            }

            // WeaponService에서 ClearWeaponReferences 메서드 호출
            if (weaponService.GetType().GetMethod("ClearWeaponReferences") != null)
            {
                weaponService.GetType().GetMethod("ClearWeaponReferences").Invoke(weaponService, null);
                Debug.Log("무기 참조 초기화 완료");
            }
        }
    }

    // CharacterAttackBase 초기화
    private void ResetCharacterAttack(GameObject player)
    {
        CharacterAttackBase attackBase = player.GetComponent<CharacterAttackBase>();
        if (attackBase != null)
        {
            // CharacterAttackBase에서 UnequipWeapon 메서드 호출
            if (attackBase.GetType().GetMethod("UnequipWeapon") != null)
            {
                attackBase.GetType().GetMethod("UnequipWeapon").Invoke(attackBase, null);
                Debug.Log("CharacterAttackBase 초기화 완료");
            }
        }
    }

    // 애니메이션 초기화
    private void ResetPlayerAnimations(GameObject player)
    {
        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            // 모든 트리거 리셋
            playerAnimator.ResetTrigger("Attack");
            playerAnimator.ResetTrigger("ChargingAttack");
            playerAnimator.ResetTrigger("ReleaseCharge");
            playerAnimator.ResetTrigger("SpecialAttack");
            playerAnimator.ResetTrigger("Hit");
            playerAnimator.ResetTrigger("Die");

            // 기본 상태로 전환
            playerAnimator.SetInteger("AttackCount", 0);
            playerAnimator.SetBool("IsStun", false);
            playerAnimator.Play("Idle", 0, 0f);

            Debug.Log("플레이어 애니메이션 상태 초기화 완료");
        }
    }

    private IEnumerator SetPlayerPosition(Rigidbody rb)
    {
        // 물리 엔진 임시 비활성화
        bool wasKinematic = rb.isKinematic;
        rb.isKinematic = true;

        yield return new WaitForSeconds(0.2f);

        // 원래 상태로 복원
        rb.isKinematic = wasKinematic;
    }
} 