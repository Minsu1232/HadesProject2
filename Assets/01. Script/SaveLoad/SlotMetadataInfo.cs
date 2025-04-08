using System;

// ���̺� ���� ��Ÿ������ ���� (UI ǥ�ÿ�)
public class SlotMetadataInfo
{
    public int slotIndex;                // ���� �ε��� (0, 1, 2)
    public bool hasData;                 // ���Կ� �����Ͱ� �ִ��� ����
    public string playerName;            // �÷��̾� �̸�
    public int chapterProgress;          // é�� ���൵
    public DateTime lastSaveTime;        // ������ ���� �ð�
    public int totalPlayTime;            // �� �÷��� �ð� (��)

    // ������ ���� �ð��� ���ڿ��� ��ȯ
    public string GetLastSaveTimeString()
    {
        if (lastSaveTime == DateTime.MinValue)
            return "����";

        return lastSaveTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    // �� �÷��� �ð��� ����ȭ�� ���ڿ��� ��ȯ
    public string GetFormattedPlayTime()
    {
        if (totalPlayTime <= 0)
            return "0��";

        int hours = totalPlayTime / 3600;
        int minutes = (totalPlayTime % 3600) / 60;

        if (hours > 0)
            return $"{hours}�ð� {minutes}��";
        else
            return $"{minutes}��";
    }
}