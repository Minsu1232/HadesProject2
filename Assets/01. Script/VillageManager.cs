using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VillageManager : MonoBehaviour
{
   [SerializeField] GameObject timekeeperWorkshop;
    [SerializeField]
    GameObject timekeeperNPC;
    // VillageManager의 Start 메서드
    private void Start()
    {
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
    }
}
