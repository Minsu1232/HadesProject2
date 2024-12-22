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
    public string clearCondition;  // Kill, Collect, Boss 등
    public int clearRequirement;   // 처치 수, 수집 수 등
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
            Debug.LogWarning($"CSV 파일이 없습니다. StreamingAssets에서 복사합니다: {persistentFilePath}");
            CopyCSVFromStreamingAssets();
        }
    }

    private void CopyCSVFromStreamingAssets()
    {
        if (File.Exists(streamingFilePath))
        {
            File.Copy(streamingFilePath, persistentFilePath);
            Debug.Log($"StreamingAssets에서 CSV 파일 복사 완료: {persistentFilePath}");
        }
        else
        {
            Debug.LogError("StreamingAssets에서 Stages.csv 파일을 찾을 수 없습니다.");
        }
    }

    public void InitializeStages()
    {
        if (!File.Exists(persistentFilePath))
        {
            Debug.LogError($"스테이지 데이터 CSV 파일을 찾을 수 없습니다: {persistentFilePath}");
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

            // 몬스터 스폰 포인트 파싱 (콤마로 구분된 여러 데이터)
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
            Debug.Log($"스테이지 로드: {stageKey}");
        }

        Debug.Log($"스테이지 데이터 로드 완료: {stageDatabase.Count}개의 스테이지");
    }

    public async void LoadStage(int chapter, int stage)
    {
        string stageKey = $"{chapter}-{stage}";
        if (!stageDatabase.TryGetValue(stageKey, out StageData stageData))
        {
            Debug.LogError($"스테이지를 찾을 수 없습니다: {stageKey}");
            return;
        }

        // 이전 스테이지 제거
        UnloadCurrentStage();

        // 새 스테이지 로드
        var loadOperation = stageData.stagePrefab.LoadAssetAsync();
        GameObject stagePrefab = await loadOperation.Task;

        if (stagePrefab != null)
        {
            currentStage = Instantiate(stagePrefab);
            currentStageData = stageData;

            // 플레이어 위치 설정
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = stageData.playerSpawnPoint;
            }

            // 몬스터 스폰 설정
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

        // MonsterManager를 통해 몬스터 생성
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

    // CSV 저장 기능
    public void SaveStageDataToCSV()
    {
        using (StreamWriter writer = new StreamWriter(persistentFilePath))
        {
            // 헤더
            writer.WriteLine("ChapterID,StageID,PrefabGUID,PlayerSpawnPoint,ClearCondition,ClearRequirement,MonsterSpawnPoints");

            foreach (var pair in stageDatabase)
            {
                StageData stage = pair.Value;

                // 몬스터 스폰 포인트 데이터 구성
                List<string> spawnPointStrings = new List<string>();
                foreach (var spawn in stage.monsterSpawnPoints)
                {
                    string spawnString = $"{FormatVector3(spawn.position)}|{spawn.monsterId}|{spawn.spawnDelay}";
                    spawnPointStrings.Add(spawnString);
                }
                string spawnPointsData = string.Join(";", spawnPointStrings);

                // 스테이지 데이터 라인 작성
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
        Debug.Log($"스테이지 데이터 저장 완료: {persistentFilePath}");
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