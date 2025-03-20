// ItemFindComponent.cs - 아이템 찾기 확률 증가 컴포넌트
using UnityEngine;

public class ItemFindComponent : MonoBehaviour
{
    private float itemFindBonus = 0f;

    private void Awake()
    {
        // 전역 인스턴스에 아이템 찾기 보너스 등록
        RegisterGlobalBonus();
    }

    private void OnDestroy()
    {
        // 전역 인스턴스에서 보너스 제거
        UnregisterGlobalBonus();
    }

    private void RegisterGlobalBonus()
    {
        if (GlobalItemFindManager.Instance != null)
        {
            GlobalItemFindManager.Instance.AddItemFindBonus(itemFindBonus);
        }
        else
        {
            Debug.LogWarning("GlobalItemFindManager가 존재하지 않습니다.");

            // 매니저가 없을 경우 자동으로 생성
            GameObject managerObj = new GameObject("GlobalItemFindManager");
            managerObj.AddComponent<GlobalItemFindManager>();

            // 생성 후 다시 시도
            GlobalItemFindManager.Instance.AddItemFindBonus(itemFindBonus);
        }
    }

    private void UnregisterGlobalBonus()
    {
        if (GlobalItemFindManager.Instance != null)
        {
            GlobalItemFindManager.Instance.RemoveItemFindBonus(itemFindBonus);
        }
    }

    // 아이템 찾기 보너스 증가
    public void AddItemFindBonus(float bonus)
    {
        // 기존 값 저장
        float oldBonus = itemFindBonus;

        // 새 값 적용
        itemFindBonus += bonus;

        // 전역 매니저에 변경사항 반영
        if (GlobalItemFindManager.Instance != null)
        {
            GlobalItemFindManager.Instance.RemoveItemFindBonus(oldBonus); // 기존 보너스 제거
            GlobalItemFindManager.Instance.AddItemFindBonus(itemFindBonus); // 새 보너스 추가
        }

        Debug.Log($"아이템 찾기 보너스 추가: +{bonus * 100}%, 현재: +{itemFindBonus * 100}%");
    }

    // 아이템 찾기 보너스 감소
    public void RemoveItemFindBonus(float bonus)
    {
        // 기존 값 저장
        float oldBonus = itemFindBonus;

        // 새 값 적용
        itemFindBonus -= bonus;
        itemFindBonus = Mathf.Max(0f, itemFindBonus);

        // 전역 매니저에 변경사항 반영
        if (GlobalItemFindManager.Instance != null)
        {
            GlobalItemFindManager.Instance.RemoveItemFindBonus(oldBonus); // 기존 보너스 제거
            GlobalItemFindManager.Instance.AddItemFindBonus(itemFindBonus); // 새 보너스 추가
        }

        Debug.Log($"아이템 찾기 보너스 감소: -{bonus * 100}%, 현재: +{itemFindBonus * 100}%");
    }

    // 현재 아이템 찾기 보너스 반환
    public float GetItemFindBonus()
    {
        return itemFindBonus;
    }
}