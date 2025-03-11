// ������ ��� ���̺� Ŭ����
[System.Serializable]
public class ItemDropEntry
{
    public int itemId;           // ������ ID
    public float dropChance;     // ��� Ȯ�� (0-100%)
    public int minQuantity = 1;  // �ּ� ����
    public int maxQuantity = 1;  // �ִ� ����

    // ���� ����� �ʵ� (���� ��꿡 ������ ����)
    public string itemName;      // ������ �̸� (�ν����Ϳ��� Ȯ�ο�)
}