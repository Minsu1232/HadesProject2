using UnityEngine;

public class ItemFindComponent : MonoBehaviour
{
    private float itemFindBonus = 0f;

    private void Awake()
    {
        // ���� �ν��Ͻ��� ������ ã�� ���ʽ� ���
        RegisterGlobalBonus();
    }

    private void OnDestroy()
    {
        // ���� �ν��Ͻ����� ���ʽ� ����
        UnregisterGlobalBonus();
    }

    private void RegisterGlobalBonus()
    {
        // ���� �ν��Ͻ�/�̺�Ʈ�� ���ʽ� ���
        // ���⼭�� ItemDropSystem�� ��� Ȯ�� ��� �� ������ �� �ֵ��� ��
    }

    private void UnregisterGlobalBonus()
    {
        // ���� �ν��Ͻ�/�̺�Ʈ���� ���ʽ� ����
    }

    // ������ ã�� ���ʽ� ����
    public void AddItemFindBonus(float bonus)
    {
        itemFindBonus += bonus;
    }

    // ������ ã�� ���ʽ� ����
    public void RemoveItemFindBonus(float bonus)
    {
        itemFindBonus -= bonus;
        itemFindBonus = Mathf.Max(0f, itemFindBonus);
    }

    // ���� ������ ã�� ���ʽ� ��ȯ
    public float GetItemFindBonus()
    {
        return itemFindBonus;
    }
}