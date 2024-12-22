using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class StageData
{
    public int chapterId;
    public int stageId;
    public AssetReferenceGameObject stagePrefab;
    public Vector3 playerSpawnPoint;
    public List<SpawnPoint> monsterSpawnPoints;
    public string clearCondition;  // Kill, Collect, Boss ��
    public int clearRequirement;   // óġ ��, ���� �� ��
}

[System.Serializable]
public class SpawnPoint
{
    public Vector3 position;
    public int monsterId;
    public float spawnDelay;
}

public class StageManager : Singleton<StageManager>
{
    private Dictionary<string, StageData> stageDatabase = new Dictionary<string, StageData>();
    private string persistentFilePath;
    private string streamingFilePath;
    private GameObject currentStage;
    private StageData currentStageData;

    private void Awake()
    {
        InitializePaths();
        CheckAndCreateLocalData();
    }

    private void InitializePaths()
    {
        persistentFilePath = Path.Combine(Application.persistentDataPath, "Stages.csv");
        streamingFilePath = Path.Combine(Application.streamingAssetsPath, "Stages.csv");
    }

    private void CheckAndCreateLocalData()
    {
        if (!File.Exists(persistentFilePath))
        {
            Debug.LogWarning($"CSV ������ �����ϴ�. StreamingAssets���� �����մϴ�: {persistentFilePath}");
            CopyCSVFromStreamingAssets();
        }
    }

    private void CopyCSVFromStreamingAssets()
    {
        if (File.Exists(streamingFilePath))
        {
            File.Copy(streamingFilePath, persistentFilePath);
            Debug.Log($"StreamingAssets���� CSV ���� ���� �Ϸ�: {persistentFilePath}");
        }
        else
        {
            Debug.LogError("StreamingAssets���� Stages.csv ������ ã�� �� �����ϴ�.");
        }
    }

    public void InitializeStages()
    {
        if (!File.Exists(persistentFilePath))
        {
            Debug.LogError($"�������� ������ CSV ������ ã�� �� �����ϴ�: {persistentFilePath}");
            return;
        }

        string[] lines = File.ReadAllLines(persistentFilePath);
        bool isFirstLine = true;

        foreach (string line in lines)
        {
            if (isFirstLine)
            {
                isFirstLine = false;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Trim().Split(',');

            StageData stageData = new StageData
            {
                chapterId = int.Parse(values[0]),
                stageId = int.Parse(values[1]),
                stagePrefab = new AssetReferenceGameObject(values[2]), // Addressables GUID
                playerSpawnPoint = ParseVector3(values[3]),
                clearCondition = values[4],
                clearRequirement = int.Parse(values[5])
            };

            // ���� ���� ����Ʈ �Ľ� (�޸��� ���е� ���� ������)
            string[] spawnPointsData = values[6].Split(';');
            stageData.monsterSpawnPoints = new List<SpawnPoint>();

            foreach (string spawnData in spawnPointsData)
            {
                string[] spawnValues = spawnData.Split('|');
                SpawnPoint spawnPoint = new SpawnPoint
                {
                    position = ParseVector3(spawnValues[0]),
                    monsterId = int.Parse(spawnValues[1]),
                    spawnDelay = float.Parse(spawnValues[2])
                };
                stageData.monsterSpawnPoints.Add(spawnPoint);
            }

            string stageKey = $"{stageData.chapterId}-{stageData.stageId}";
            stageDatabase[stageKey] = stageData;
            Debug.Log($"�������� �ε�: {stageKey}");
        }

        Debug.Log($"�������� ������ �ε� �Ϸ�: {stageDatabase.Count}���� ��������");
    }

    public async void LoadStage(int chapter, int stage)
    {
        string stageKey = $"{chapter}-{stage}";
        if (!stageDatabase.TryGetValue(stageKey, out StageData stageData))
        {
            Debug.LogError($"���������� ã�� �� �����ϴ�: {stageKey}");
            return;
        }

        // ���� �������� ����
        UnloadCurrentStage();

        // �� �������� �ε�
        var loadOperation = stageData.stagePrefab.LoadAssetAsync();
        GameObject stagePrefab = await loadOperation.Task;

        if (stagePrefab != null)
        {
            currentStage = Instantiate(stagePrefab);
            currentStageData = stageData;

            // �÷��̾� ��ġ ����
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = stageData.playerSpawnPoint;
            }

            // ���� ���� ����
            StartMonsterSpawner();
        }
    }

    private void UnloadCurrentStage()
    {
        if (currentStage != null)
        {
            if (currentStageData?.stagePrefab != null)
            {
                currentStageData.stagePrefab.ReleaseAsset();
            }
            Destroy(currentStage);
            currentStage = null;
            currentStageData = null;
        }
    }

    private void StartMonsterSpawner()
    {
        foreach (var spawnPoint in currentStageData.monsterSpawnPoints)
        {
            StartCoroutine(SpawnMonsterWithDelay(spawnPoint));
        }
    }

    private System.Collections.IEnumerator SpawnMonsterWithDelay(SpawnPoint spawnPoint)
    {
        yield return new WaitForSeconds(spawnPoint.spawnDelay);

        // MonsterManager�� ���� ���� ����
        // MonsterManager.Instance.SpawnMonster(spawnPoint.monsterId, spawnPoint.position);
    }

    private Vector3 ParseVector3(string value)
    {
        string[] parts = value.Split('/');
        return new Vector3(
            float.Parse(parts[0]),
            float.Parse(parts[1]),
            float.Parse(parts[2])
        );
    }

    // CSV ���� ���
    public void SaveStageDataToCSV()
    {
        using (StreamWriter writer = new StreamWriter(persistentFilePath))
        {
            // ���
            writer.WriteLine("ChapterID,StageID,PrefabGUID,PlayerSpawnPoint,ClearCondition,ClearRequirement,MonsterSpawnPoints");

            foreach (var pair in stageDatabase)
            {
                StageData stage = pair.Value;

                // ���� ���� ����Ʈ ������ ����
                List<string> spawnPointStrings = new List<string>();
                foreach (var spawn in stage.monsterSpawnPoints)
                {
                    string spawnString = $"{FormatVector3(spawn.position)}|{spawn.monsterId}|{spawn.spawnDelay}";
                    spawnPointStrings.Add(spawnString);
                }
                string spawnPointsData = string.Join(";", spawnPointStrings);

                // �������� ������ ���� �ۼ�
                string line = $"{stage.chapterId}," +
                            $"{stage.stageId}," +
                            $"{stage.stagePrefab.AssetGUID}," +
                            $"{FormatVector3(stage.playerSpawnPoint)}," +
                            $"{stage.clearCondition}," +
                            $"{stage.clearRequirement}," +
                            $"{spawnPointsData}";

                writer.WriteLine(line);
            }
        }
        Debug.Log($"�������� ������ ���� �Ϸ�: {persistentFilePath}");
    }

    private string FormatVector3(Vector3 vector)
    {
        return $"{vector.x}/{vector.y}/{vector.z}";
    }

    public void OnStageClear()
    {
        if (currentStageData != null)
        {
            int nextStage = currentStageData.stageId + 1;
            LoadStage(currentStageData.chapterId, nextStage);
        }
    }

    private void OnDestroy()
    {
        UnloadCurrentStage();
    }
}