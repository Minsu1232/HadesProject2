using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class StageDataLoader : MonoBehaviour
{
    [Header("���� ��� ����")]
    private const string STAGES_CSV = "Stage.csv";
    private const string STAGE_MONSTERS_CSV = "StageMonsters.csv";
    private const string STAGE_FOLDER = "Stages";  // ���������� CSV ���� ����

    private Dictionary<string, StageData> stageDataCache = new Dictionary<string, StageData>();

    public async Task Initialize()
    {
        try
        {
            // �������� ������ �ε� ��� 2����
            // 1. ���� ���� CSV ���� �ε�
            //Dictionary<string, StageData> stagesFromMainCSV = await LoadStagesFromMainCSV();
            //Dictionary<string, List<MonsterSpawnInfo>> monsterSpawnsFromMainCSV = await LoadMonsterSpawnsFromMainCSV();

            //// ���� ��� ������ ����
            //MergeBaseStageData(stagesFromMainCSV, monsterSpawnsFromMainCSV);

            // 2. ���������� ���� CSV ���� �ε� (������ ���)
            await LoadStageSpecificCSVFiles();

            Debug.Log($"�������� ������ �ε� �Ϸ�: {stageDataCache.Count}�� ��������");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"�������� ������ �ε� ����: {e.Message}");
        }
    }

    // ���� ���� Stage.csv���� �������� �⺻ ���� �ε�
    private async Task<Dictionary<string, StageData>> LoadStagesFromMainCSV()
    {
        Dictionary<string, StageData> result = new Dictionary<string, StageData>();

        // CSV ���� ���
        string path = Path.Combine(Application.streamingAssetsPath, STAGES_CSV);

        // CSV ���� ���� Ȯ��
        if (!File.Exists(path))
        {
            Debug.LogWarning($"�⺻ �������� CSV ������ ã�� �� ����: {path}");
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

            // �÷��̾� ���� ��ġ
            if (values.Length >= 9)
            {
                float spawnX = float.Parse(values[6]);
                float spawnY = float.Parse(values[7]);
                float spawnZ = float.Parse(values[8]);
                stageData.playerSpawnPosition = new Vector3(spawnX, spawnY, spawnZ);
            }
            else
            {
                stageData.playerSpawnPosition = Vector3.zero; // �⺻��
            }

            // ��Ż ���� ��ġ (���� ���)
            if (values.Length >= 12)
            {
                float portalX = float.Parse(values[9]);
                float portalY = float.Parse(values[10]);
                float portalZ = float.Parse(values[11]);
                stageData.portalSpawnPosition = new Vector3(portalX, portalY, portalZ);
            }
            else
            {
                stageData.portalSpawnPosition = Vector3.zero; // �⺻��
            }

            result[stageData.stageID] = stageData;
        }

        return result;
    }

    // ���� ���� StageMonsters.csv���� ���� ���� ���� �ε�
    private async Task<Dictionary<string, List<MonsterSpawnInfo>>> LoadMonsterSpawnsFromMainCSV()
    {
        Dictionary<string, List<MonsterSpawnInfo>> result = new Dictionary<string, List<MonsterSpawnInfo>>();

        // CSV ���� ���
        string path = Path.Combine(Application.streamingAssetsPath, STAGE_MONSTERS_CSV);

        // CSV ���� ���� Ȯ��
        if (!File.Exists(path))
        {
            Debug.LogWarning($"�⺻ ���� ���� CSV ������ ã�� �� ����: {path}");
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

    // ���� ��� ������ ����
    private void MergeBaseStageData(Dictionary<string, StageData> stages, Dictionary<string, List<MonsterSpawnInfo>> monsterSpawns)
    {
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

            // �⺻ ���� ���� ����Ʈ �߰� (���� ȯ�濡���� ���� �����ο� �°� ���� �ʿ�)
            if (stageData.spawnPoints.Count == 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    float x = Random.Range(-10f, 10f);
                    float z = Random.Range(-10f, 10f);
                    stageData.spawnPoints.Add(new Vector3(x, 0f, z));
                }
            }

            // ĳ�ÿ� ����
            stageDataCache[stageID] = stageData;
        }
    }

    // ���������� ���� CSV ���� �ε�
    private async Task LoadStageSpecificCSVFiles()
    {
        string stageFolder = Path.Combine(Application.streamingAssetsPath, STAGE_FOLDER);
        
        // ���� ���� Ȯ��
        if (!Directory.Exists(stageFolder))
        {
            Debug.LogWarning($"�������� ������ �����ϴ�: {stageFolder}");
            return;
        }

        // 1. �������� �⺻ ���� ���� �ε�
        string[] stageFiles = Directory.GetFiles(stageFolder, "*_stage.csv");
        foreach (string stageFile in stageFiles)
        {
            try
            {
                // ���� �̸����� �������� ID ����
                string fileName = Path.GetFileNameWithoutExtension(stageFile);
                string stageID = fileName.Substring(0, fileName.Length - 6); // "_stage" �κ� ����

                // �������� ���� �б�
                string csv = await File.ReadAllTextAsync(stageFile);
                string[] lines = csv.Split('\n');
                
                // ��� �ǳʶٱ� (�ּ� 2�� �ʿ�)
                if (lines.Length < 2) continue;
                
                // ù ��° ������ �� ó��
                string line = lines[1].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] values = line.Split(',');
                if (values.Length < 12) continue; // ��Ż ��ġ���� �����ؼ� �ּ� 12�� �ʿ�

                // �������� ������ ���� �Ǵ� ������Ʈ
                StageData stageData;
                if (stageDataCache.TryGetValue(stageID, out stageData))
                {
                    // ���� ������ ������Ʈ
                    stageData.chapterID = int.Parse(values[1]);
                    stageData.stageName = values[2];
                    stageData.nextStageID = values[3];
                    stageData.isBossStage = bool.Parse(values[4]);
                    stageData.isMidBossStage = bool.Parse(values[5]);
                }
                else
                {
                    // �� ������ ����
                    stageData = ScriptableObject.CreateInstance<StageData>();
                    stageData.stageID = stageID;
                    stageData.chapterID = int.Parse(values[1]);
                    stageData.stageName = values[2];
                    stageData.nextStageID = values[3];
                    stageData.isBossStage = bool.Parse(values[4]);
                    stageData.isMidBossStage = bool.Parse(values[5]);
                    stageDataCache[stageID] = stageData;
                }

                // �÷��̾� ���� ��ġ
                float spawnX = float.Parse(values[6]);
                float spawnY = float.Parse(values[7]);
                float spawnZ = float.Parse(values[8]);
                stageData.playerSpawnPosition = new Vector3(spawnX, spawnY, spawnZ);

                // ��Ż ���� ��ġ
                float portalX = float.Parse(values[9]);
                float portalY = float.Parse(values[10]);
                float portalZ = float.Parse(values[11]);
                stageData.portalSpawnPosition = new Vector3(portalX, portalY, portalZ);

                Debug.Log($"�������� �⺻ ���� �ε� �Ϸ�: {stageID}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"�������� ���� �ε� ����: {stageFile}, {e.Message}");
            }
        }

        // 2. ���� ���� ���� ���� �ε�
        string[] spawnFiles = Directory.GetFiles(stageFolder, "*_spawns.csv");
        foreach (string spawnFile in spawnFiles)
        {
            try
            {
                // ���� �̸����� �������� ID ����
                string fileName = Path.GetFileNameWithoutExtension(spawnFile);
                string stageID = fileName.Substring(0, fileName.Length - 7); // "_spawns" �κ� ����

                // �ش� �������� ������ Ȯ��
                if (!stageDataCache.TryGetValue(stageID, out StageData stageData))
                {
                    Debug.LogWarning($"���� ���Ͽ� �ش��ϴ� �������� �����Ͱ� �����ϴ�: {stageID}");
                    continue;
                }

                // ���� ���� ���� �ʱ�ȭ (�����)
                stageData.fixedSpawns.Clear();
                stageData.randomMonsters.Clear();
                stageData.spawnPoints.Clear();

                // ���� ���� �б�
                string csv = await File.ReadAllTextAsync(spawnFile);
                string[] lines = csv.Split('\n');
                
                // ��� �ǳʶٱ�
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] values = line.Split(',');
                    if (values.Length < 7) continue;

                    // CSV���� �������� ID�� Ȯ�� (��ġ�ؾ� ��)
                    string csvStageID = values[0];
                    if (csvStageID != stageID) continue;

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

                    // ���� ���� �߰�
                    if (spawnInfo.monsterID >= 1000 || spawnInfo.isFixedPosition)
                    {
                        stageData.fixedSpawns.Add(spawnInfo);
                    }
                    else
                    {
                        stageData.randomMonsters.Add(spawnInfo);
                        
                        // ���� ���� ��ġ�ε� �߰�
                        stageData.spawnPoints.Add(spawnInfo.position);
                    }
                }

                // ���� ���� ����Ʈ�� �ʹ� ������ �߰�
                if (stageData.spawnPoints.Count < 5)
                {
                    int additionalPoints = 5 - stageData.spawnPoints.Count;
                    for (int i = 0; i < additionalPoints; i++)
                    {
                        float x = Random.Range(-10f, 10f);
                        float z = Random.Range(-10f, 10f);
                        stageData.spawnPoints.Add(new Vector3(x, 0f, z));
                    }
                }

                Debug.Log($"�������� ���� ���� �ε� �Ϸ�: {stageID}, ����: {stageData.fixedSpawns.Count}��, ����: {stageData.randomMonsters.Count}��");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"���� ���� �ε� ����: {spawnFile}, {e.Message}");
            }
        }
    }

    // �������� ������ ��������
    public StageData GetStageData(string stageID)
    {
        if (stageDataCache.TryGetValue(stageID, out StageData stageData))
            return stageData;

        Debug.LogWarning($"�������� �����͸� ã�� �� ����: {stageID}");
        return null;
    }

    // ���߿�: ��� �������� ������ ���
    public void DebugPrintStageData()
    {
        foreach (var entry in stageDataCache)
        {
            StageData stage = entry.Value;
            Debug.Log($"��������: {stage.stageID} - {stage.stageName}");
            Debug.Log($"  é��: {stage.chapterID}, ���� ��������: {stage.nextStageID}");
            Debug.Log($"  ����: {stage.isBossStage}, �߰�����: {stage.isMidBossStage}");
            Debug.Log($"  �÷��̾� ���� ��ġ: {stage.playerSpawnPosition}");
            Debug.Log($"  ��Ż ���� ��ġ: {stage.portalSpawnPosition}");
            Debug.Log($"  ���� ����: {stage.fixedSpawns.Count}��");
            Debug.Log($"  ���� ���� Ǯ: {stage.randomMonsters.Count}��");
            Debug.Log($"  ���� ����Ʈ: {stage.spawnPoints.Count}��");
        }
    }
}