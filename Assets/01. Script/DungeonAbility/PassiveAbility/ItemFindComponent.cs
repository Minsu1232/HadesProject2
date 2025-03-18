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
        // 전역 인스턴스/이벤트에 보너스 등록
        // 여기서는 ItemDropSystem의 드롭 확률 계산 시 참조할 수 있도록 함
    }

    private void UnregisterGlobalBonus()
    {
        // 전역 인스턴스/이벤트에서 보너스 제거
    }

    // 아이템 찾기 보너스 증가
    public void AddItemFindBonus(float bonus)
    {
        itemFindBonus += bonus;
    }

    // 아이템 찾기 보너스 감소
    public void RemoveItemFindBonus(float bonus)
    {
        itemFindBonus -= bonus;
        itemFindBonus = Mathf.Max(0f, itemFindBonus);
    }

    // 현재 아이템 찾기 보너스 반환
    public float GetItemFindBonus()
    {
        return itemFindBonus;
    }
}