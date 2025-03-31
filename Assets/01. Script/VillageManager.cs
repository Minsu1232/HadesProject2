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
    // VillageManager의 Start 메서드
    private void Start()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        // 보스 프래그먼트 소유 여부로 작업실 해금 상태 확인
        bool hasFragment = false;

        // SaveManager를 통해 확인
        if (SaveManager.Instance != null)
        {
            PlayerSaveData playerData = SaveManager.Instance.GetPlayerData();
            // 장착된 프래그먼트 확인
            hasFragment = playerData.equippedFragments.Contains(1001);

            // 장착 안된 경우 인벤토리도 확인
            if (!hasFragment)
            {
                hasFragment = playerData.inventory.Any(item => item.itemID == 1001);
            }
        }
   

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
