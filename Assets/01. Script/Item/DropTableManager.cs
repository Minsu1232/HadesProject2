// DropTableManager.cs - CSV���� ��� ���̺� �ε� �� ����
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DropTableManager : MonoBehaviour
{
    public static DropTableManager Instance { get; private set; }

    // ��� ���̺� ������ �����
    private Dictionary<int, List<ItemDropEntry>> monsterDropTables = new Dictionary<int, List<ItemDropEntry>>();
    private Dictionary<int, List<ItemDropEntry>> bossDropTables = new Dictionary<int, List<ItemDropEntry>>();

    private string dropTablePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // CSV ���� ��� ����
        dropTablePath = Path.Combine(Application.persistentDataPath, "DropTables.csv");

        // StreamingAssets���� CSV ���� ����
        CopyCSVFromStreamingAssets();

        // ��� ���̺� �ε�
        LoadDropTablesFromCSV();

        Debug.Log("==== ���� ��� ���̺� ====");
        foreach (var kvp in monsterDropTables)
        {
            Debug.Log($"���� ID: {kvp.Key}, ������ ��: {kvp.Value.Count}");
        }
    }

    // StreamingAssets���� CSV ���� ����
    private void CopyCSVFromStreamingAssets()
    {
        string streamingPath = Path.Combine(Application.streamingAssetsPath, "DropTables.csv");

        if (File.Exists(streamingPath))
        {
            File.Copy(streamingPath, dropTablePath, true);
            Debug.Log("��� ���̺� CSV ���� ���� �Ϸ�");
        }
        else
        {
            Debug.LogWarning("StreamingAssets���� ��� ���̺� CSV ������ ã�� �� �����ϴ�.");
        }
    }

    // CSV���� ��� ���̺� �ε�
    private void LoadDropTablesFromCSV()
    {
        if (!File.Exists(dropTablePath))
        {
            Debug.LogError("��� ���̺� CSV ������ ã�� �� �����ϴ�: " + dropTablePath);
            return;
        }

        // CSV ���� �б�
        string[] lines = File.ReadAllLines(dropTablePath);

        // ��� �˻�
        if (lines.Length == 0)
        {
            Debug.LogError("��� ���̺� CSV ������ ��� �ֽ��ϴ�.");
            return;
        }

        string[] headers = lines[0].Split(',');

        // �ʼ� �� �ε��� ã��
        int entityIdIndex = FindColumnIndex(headers, "EntityID");
        int isBossIndex = FindColumnIndex(headers, "IsBoss");
        int itemIdIndex = FindColumnIndex(headers, "ItemID");
        int dropChanceIndex = FindColumnIndex(headers, "DropChance");
        int minQuantityIndex = FindColumnIndex(headers, "MinQuantity");
        int maxQuantityIndex = FindColumnIndex(headers, "MaxQuantity");
        int itemNameIndex = FindColumnIndex(headers, "ItemName");

        // �ʼ� ���� ������ ���� �޽��� ǥ��
        if (entityIdIndex == -1 || isBossIndex == -1 || itemIdIndex == -1 || dropChanceIndex == -1)
        {
            Debug.LogError("��� ���̺� CSV ���Ͽ� �ʼ� ���� �����ϴ�. �ʼ� ��: EntityID, IsBoss, ItemID, DropChance");
            return;
        }

        // �� �� ó��
        for (int i = 1; i < lines.Length; i++)
        {
            // �� �� �ǳʶٱ�
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');

            // ���� ������� Ȯ��
            if (values.Length <= System.Math.Max(entityIdIndex, System.Math.Max(isBossIndex, System.Math.Max(itemIdIndex, dropChanceIndex))))
            {
                Debug.LogWarning($"��� ���̺� CSV �� {i + 1}�� ���� �����մϴ�. �ǳʶݴϴ�.");
                continue;
            }

            try
            {
                // �⺻ ������ �Ľ�
                int entityId = int.Parse(values[entityIdIndex]);
                bool isBoss = bool.Parse(values[isBossIndex]);
                int itemId = int.Parse(values[itemIdIndex]);
                float dropChance = float.Parse(values[dropChanceIndex]);

                // ������ ������ �Ľ� (�⺻�� ����)
                int minQuantity = 1;
                int maxQuantity = 1;
                string itemName = "";

                // ���� �Ľ� (�ִ� ���)
                if (minQuantityIndex != -1 && values.Length > minQuantityIndex)
                {
                    int.TryParse(values[minQuantityIndex], out minQuantity);
                }

                if (maxQuantityIndex != -1 && values.Length > maxQuantityIndex)
                {
                    int.TryParse(values[maxQuantityIndex], out maxQuantity);
                    // �ּҰ����� ������ �ּҰ����� ����
                    maxQuantity = Mathf.Max(minQuantity, maxQuantity);
                }

                // ������ �̸� �Ľ� (�ִ� ���)
                if (itemNameIndex != -1 && values.Length > itemNameIndex)
                {
                    itemName = values[itemNameIndex];
                }

                // ��� ��Ʈ�� ����
                ItemDropEntry entry = new ItemDropEntry
                {
                    itemId = itemId,
                    dropChance = dropChance,
                    minQuantity = minQuantity,
                    maxQuantity = maxQuantity,
                    itemName = itemName
                };

                // ������ ���̺� �߰�
                if (isBoss)
                {
                    if (!bossDropTables.ContainsKey(entityId))
                    {
                        bossDropTables[entityId] = new List<ItemDropEntry>();
                    }
                    bossDropTables[entityId].Add(entry);
                }
                else
                {
                    if (!monsterDropTables.ContainsKey(entityId))
                    {
                        monsterDropTables[entityId] = new List<ItemDropEntry>();
                    }
                    monsterDropTables[entityId].Add(entry);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"��� ���̺� CSV �� {i + 1} ó�� �� ���� �߻�: {e.Message}");
            }
        }

        Debug.Log($"��� ���̺� �ε� �Ϸ�: {monsterDropTables.Count}�� ����, {bossDropTables.Count}�� ����");
    }

    // CSV �÷� �ε��� ã�� ���� �Լ�
    private int FindColumnIndex(string[] headers, string columnName)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            if (headers[i].Trim().Equals(columnName, System.StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return -1; // ã�� ����
    }

    // ���� ��� ���̺� ��������
    public List<ItemDropEntry> GetMonsterDropTable(int monsterId)
    {
        Debug.Log($"[DropTableManager] GetMonsterDropTable ȣ��: ID = {monsterId}");

        if (monsterDropTables.TryGetValue(monsterId, out List<ItemDropEntry> entries))
        {
            Debug.Log($"[DropTableManager] ��� ���̺� ã��: {entries.Count}�� �׸�");
            return entries;
        }

        Debug.LogWarning($"[DropTableManager] ID {monsterId}�� ���� ��� ���̺��� ã�� �� �����ϴ�.");
        return null;
    }

    // ���� ��� ���̺� ��������
    public List<ItemDropEntry> GetBossDropTable(int bossId)
    {
        if (bossDropTables.TryGetValue(bossId, out List<ItemDropEntry> entries))
        {
            return entries;
        }
        return null;
    }

    // ����׿� - ��� ��� ���̺� ���� ���
    public void DebugPrintAllDropTables()
    {
        Debug.Log("==== ���� ��� ���̺� ====");
        foreach (var kvp in monsterDropTables)
        {
            Debug.Log($"���� ID: {kvp.Key}, ������ ��: {kvp.Value.Count}");
            foreach (var entry in kvp.Value)
            {
                Debug.Log($"  ������ ID: {entry.itemId}, �̸�: {entry.itemName}, ��ӷ�: {entry.dropChance}%, ����: {entry.minQuantity}-{entry.maxQuantity}");
            }
        }

        Debug.Log("==== ���� ��� ���̺� ====");
        foreach (var kvp in bossDropTables)
        {
            Debug.Log($"���� ID: {kvp.Key}, ������ ��: {kvp.Value.Count}");
            foreach (var entry in kvp.Value)
            {
                Debug.Log($"  ������ ID: {entry.itemId}, �̸�: {entry.itemName}, ��ӷ�: {entry.dropChance}%, ����: {entry.minQuantity}-{entry.maxQuantity}");
            }
        }
    }
}