using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// 마을 관련 관리
/// </summary>
public class VillageManager : MonoBehaviour
{

    PlayerClass playerClass;

    [Header("튜토리얼 관련 오브젝트")]
    [SerializeField] GameObject weaponTutorialTrigger; // 무기 튜토리얼 트리거
    [SerializeField] GameObject combatTutorialTrigger; // 전투 튜토리얼 트리거

    [Header("해금 컨텐츠 오브젝트")]
    [SerializeField] GameObject timekeeperWorkshop;
    [SerializeField] GameObject timekeeperNPC;
    [SerializeField] GameObject StatsUpgradeNPC;
    
    // VillageManager의 Start 메서드
    private void Start()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        // 보스 프래그먼트 소유 여부로 작업실 해금 상태 확인
        bool hasFragment = false;

        // SaveManager를 통해 확인
        if (SaveManager.Instance != null)
        {
            GameProgressManager.Instance.LoadProgress();
            PlayerSaveData playerData = SaveManager.Instance.GetPlayerData();
            // 장착된 프래그먼트 확인
            hasFragment = playerData.equippedFragments.Contains(1001);

            // 장착 안된 경우 인벤토리도 확인
            if (!hasFragment)
            {
                hasFragment = playerData.inventory.Any(item => item.itemID == 1001);
            }
            // 표시된 다이얼로그에 따라 튜토리얼 트리거 비활성화
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

            // 해금 상태에 따라 활성화/비활성화
        if (timekeeperWorkshop != null)
        {
            timekeeperWorkshop.SetActive(hasFragment);
        }

        if (timekeeperNPC != null)
        {
            timekeeperNPC.SetActive(hasFragment);
        }

        Debug.Log($"타임키퍼 작업실 해금 상태: {hasFragment} (보스 프래그먼트 보유 여부)");

        // 던전에서 귀환했는지 확인
        if (PlayerPrefs.GetInt("ReturnFromDungeon", 0) == 1)
        {
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
            }
            
           

            // 사용 후 플래그 초기화
            PlayerPrefs.SetInt("ReturnFromDungeon", 0);
        }

    }
    // 모든 무기 해제 메서드
    private void UnequipAllWeapons()
    {
        WeaponService weaponService = GameInitializer.Instance.GetWeaponService();

        if (weaponService != null && weaponService.HasWeaponEquipped())
        {
            // 무기가 있으면 해제
            // 참고: WeaponService에 무기 해제 메서드가 있어야 함
            weaponService.UnequipCurrentWeapon();
            Debug.Log("사망 후 귀환: 모든 무기 해제됨");
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

        // 필요에 따라 다른 튜토리얼 트리거도 처리
        // ...
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
