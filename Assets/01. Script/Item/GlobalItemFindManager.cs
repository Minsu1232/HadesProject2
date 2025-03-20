// GlobalItemFindManager.cs - 전역 아이템 찾기 보너스 관리자
using UnityEngine;

public class GlobalItemFindManager : Singleton<GlobalItemFindManager>
{
  

    // 전역 아이템 찾기 보너스 (0.1 = 10% 증가)
    private float globalItemFindBonus = 0f;


    // 보너스 추가
    public void AddItemFindBonus(float bonus)
    {
        globalItemFindBonus += bonus;
        Debug.Log($"전역 아이템 찾기 보너스 추가: +{bonus * 100}%, 현재 총 보너스: +{globalItemFindBonus * 100}%");
    }

    // 보너스 제거
    public void RemoveItemFindBonus(float bonus)
    {
        globalItemFindBonus -= bonus;
        globalItemFindBonus = Mathf.Max(0f, globalItemFindBonus); // 음수 방지
        Debug.Log($"전역 아이템 찾기 보너스 제거: -{bonus * 100}%, 현재 총 보너스: +{globalItemFindBonus * 100}%");
    }

    // 현재 보너스 가져오기
    public float GetGlobalItemFindBonus()
    {
        return globalItemFindBonus;
    }
}