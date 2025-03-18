// 1. �нú� �ɷ� �⺻ Ŭ���� (���� �������� ����Ǵ� �ɷ�)
using UnityEngine;

[System.Serializable]
public abstract class DungeonAbility
{
    public string id;             // ���� �ĺ���
    public string name;           // �̸�
    public string description;    // ����
    public Sprite icon;           // ������
    public Rarity rarity;         // ��͵�

    public int level = 1;         // �ɷ� ����

    // �� �ɷ��� ó�� ȹ��� �� ȣ��
    public abstract void OnAcquire(PlayerClass player);

    // �� �ɷ��� �������� �� ȣ��
    public abstract void OnLevelUp(PlayerClass player);

    // �������� ���� �� ȣ�� (�ɷ� �ʱ�ȭ)
    public abstract void OnReset(PlayerClass player);
}

// ��͵� enum
public enum Rarity
{
    Common,     // �Ϲ�
    Uncommon,   // ���
    Rare,       // ���
    Epic,       // ����
    Legendary   // ����
}