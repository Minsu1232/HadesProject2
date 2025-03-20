// GlobalItemFindManager.cs - ���� ������ ã�� ���ʽ� ������
using UnityEngine;

public class GlobalItemFindManager : Singleton<GlobalItemFindManager>
{
  

    // ���� ������ ã�� ���ʽ� (0.1 = 10% ����)
    private float globalItemFindBonus = 0f;


    // ���ʽ� �߰�
    public void AddItemFindBonus(float bonus)
    {
        globalItemFindBonus += bonus;
        Debug.Log($"���� ������ ã�� ���ʽ� �߰�: +{bonus * 100}%, ���� �� ���ʽ�: +{globalItemFindBonus * 100}%");
    }

    // ���ʽ� ����
    public void RemoveItemFindBonus(float bonus)
    {
        globalItemFindBonus -= bonus;
        globalItemFindBonus = Mathf.Max(0f, globalItemFindBonus); // ���� ����
        Debug.Log($"���� ������ ã�� ���ʽ� ����: -{bonus * 100}%, ���� �� ���ʽ�: +{globalItemFindBonus * 100}%");
    }

    // ���� ���ʽ� ��������
    public float GetGlobalItemFindBonus()
    {
        return globalItemFindBonus;
    }
}