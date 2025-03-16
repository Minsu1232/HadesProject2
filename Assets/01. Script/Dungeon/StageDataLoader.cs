using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class StageDataLoader : MonoBehaviour
{
    private const string STAGES_CSV = "Stages.csv";
    private const string STAGE_MONSTERS_CSV = "StageMonsters.csv";

    private Dictionary<string, StageData> stageDataCache = new Dictionary<string, StageData>();

    public async Task Initialize()
    {
        try
        {
            // CSV ���� �ε� �� �Ľ�
            Dictionary<string, StageData> stages = await LoadStagesFromCSV();
            Dictionary<string, List<MonsterSpawnInfo>> monsterSpawns = await LoadMonsterSpawnsFromCSV();

            // ������ ����
            foreach (var stageEntry in stages)
            {
                string stageID = stageEntry.Key;
                StageData stageData = stageEntry.Value;

                // ���� ���� ���� �߰�
                if (monsterSpawns.TryGetValue(stageID, out List<MonsterSpawnInfo> monsters))
                {
                    foreach (var monster in monsters)
                    {
                        // ID�� 1000 �̻��̸� ������ �����ϰ� �׻� ���� ��ġ
                        if (monster.monsterID >= 1000 || monster.isFixedPosition)
                        {
                            stageData.fixedSpawns.Add(monster);
                        }
                        else
                        {
                            stageData.randomMonsters.Add(monster);
                        }
                    }
                }

                // �⺻ ���� ����Ʈ �߰� (���� ȯ�濡���� ���� �����ο� �°� ���� �ʿ�)
                for (int i = 0; i < 10; i++)
                {
                    float x = Random.Range(-10f, 10f);
                    float z = Random.Range(-10f, 10f);
                    stageData.spawnPoints.Add(new Vector3(x, 0f, z));
                }

                // ĳ�ÿ� ����
                stageDataCache[stageID] = stageData;
            }

            Debug.Log($"�������� ������ �ε� �Ϸ�: {stageDataCache.Count}�� ��������");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"�������� ������ �ε� ����: {e.Message}");
        }
    }

    public StageData GetStageData(string stageID)
    {
        if (stageDataCache.TryGetValue(stageID, out StageData stageData))
            return stageData;

        Debug.LogWarning($"�������� �����͸� ã�� �� ����: {stageID}");
        return null;
    }

    // CSV���� �������� �⺻ ���� �ε�
    private async Task<Dictionary<string, StageData>> LoadStagesFromCSV()
    {
        Dictionary<string, StageData> result = new Dictionary<string, StageData>();

        // CSV ���� ���
        string path = Path.Combine(Application.streamingAssetsPath, STAGES_CSV);

        // CSV ���� ���� ���� (�ʿ��� ���)
        if (!File.Exists(path))
        {
            Debug.LogError($"�������� CSV ������ ã�� �� ����: {path}");
            return result;
        }

        // CSV ���� �б�
        string csv = await File.ReadAllTextAsync(path);
        string[] lines = csv.Split('\n');

        // ��� �ǳʶٱ�
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 6) continue;

            // �������� ������ ����
            StageData stageData = ScriptableObject.CreateInstance<StageData>();
            stageData.stageID = values[0];
            stageData.chapterID = int.Parse(values[1]);
            stageData.stageName = values[2];
            stageData.nextStageID = values[3];
            stageData.isBossStage = bool.Parse(values[4]);
            stageData.isMidBossStage = bool.Parse(values[5]);

            result[stageData.stageID] = stageData;
        }

        return result;
    }

    // CSV���� ���� ���� ���� �ε�
    private async Task<Dictionary<string, List<MonsterSpawnInfo>>> LoadMonsterSpawnsFromCSV()
    {
        Dictionary<string, List<MonsterSpawnInfo>> result = new Dictionary<string, List<MonsterSpawnInfo>>();

        // CSV ���� ���
        string path = Path.Combine(Application.streamingAssetsPath, STAGE_MONSTERS_CSV);

        // CSV ���� ���� Ȯ��
        if (!File.Exists(path))
        {
            Debug.LogError($"���� ���� CSV ������ ã�� �� ����: {path}");
            return result;
        }

        // CSV ���� �б�
        string csv = await File.ReadAllTextAsync(path);
        string[] lines = csv.Split('\n');

        // ��� �ǳʶٱ�
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 7) continue;

            string stageID = values[0];

            // ���� ���� ���� ����
            MonsterSpawnInfo spawnInfo = new MonsterSpawnInfo();
            spawnInfo.monsterID = int.Parse(values[1]);
            spawnInfo.isFixedPosition = bool.Parse(values[2]);

            // ��ġ ����
            float x = float.Parse(values[3]);
            float y = float.Parse(values[4]);
            float z = float.Parse(values[5]);
            spawnInfo.position = new Vector3(x, y, z);

            // ����ġ
            spawnInfo.spawnWeight = float.Parse(values[6]);

            // ����� �߰�
            if (!result.ContainsKey(stageID))
            {
                result[stageID] = new List<MonsterSpawnInfo>();
            }
            result[stageID].Add(spawnInfo);
        }

        return result;
    }

    // ���߿�: �������� ������ ���
    public void DebugPrintStageData()
    {
        foreach (var entry in stageDataCache)
        {
            StageData stage = entry.Value;
            Debug.Log($"��������: {stage.stageID} - {stage.stageName}");
            Debug.Log($"  ���� ����: {stage.fixedSpawns.Count}��");
            Debug.Log($"  ���� ���� Ǯ: {stage.randomMonsters.Count}��");
            Debug.Log($"  ���� ����Ʈ: {stage.spawnPoints.Count}��");
        }
    }
}