using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class StageDataLoader : MonoBehaviour
{
    [Header("파일 경로 설정")]
    private const string STAGES_CSV = "Stage.csv";
    private const string STAGE_MONSTERS_CSV = "StageMonsters.csv";
    private const string STAGE_FOLDER = "Stages";  // 스테이지별 CSV 파일 폴더

    private Dictionary<string, StageData> stageDataCache = new Dictionary<string, StageData>();

    public async Task Initialize()
    {
        try
        {
            // 스테이지 데이터 로드 방식 2가지
            // 1. 기존 통합 CSV 파일 로드
            //Dictionary<string, StageData> stagesFromMainCSV = await LoadStagesFromMainCSV();
            //Dictionary<string, List<MonsterSpawnInfo>> monsterSpawnsFromMainCSV = await LoadMonsterSpawnsFromMainCSV();

            //// 기존 방식 데이터 병합
            //MergeBaseStageData(stagesFromMainCSV, monsterSpawnsFromMainCSV);

            // 2. 스테이지별 개별 CSV 파일 로드 (있으면 덮어씀)
            await LoadStageSpecificCSVFiles();

            Debug.Log($"스테이지 데이터 로드 완료: {stageDataCache.Count}개 스테이지");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"스테이지 데이터 로드 실패: {e.Message}");
        }
    }

    // 기존 통합 Stage.csv에서 스테이지 기본 정보 로드
    private async Task<Dictionary<string, StageData>> LoadStagesFromMainCSV()
    {
        Dictionary<string, StageData> result = new Dictionary<string, StageData>();

        // CSV 파일 경로
        string path = Path.Combine(Application.streamingAssetsPath, STAGES_CSV);

        // CSV 파일 존재 확인
        if (!File.Exists(path))
        {
            Debug.LogWarning($"기본 스테이지 CSV 파일을 찾을 수 없음: {path}");
            return result;
        }

        // CSV 파일 읽기
        string csv = await File.ReadAllTextAsync(path);
        string[] lines = csv.Split('\n');

        // 헤더 건너뛰기
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 6) continue;

            // 스테이지 데이터 생성
            StageData stageData = ScriptableObject.CreateInstance<StageData>();
            stageData.stageID = values[0];
            stageData.chapterID = int.Parse(values[1]);
            stageData.stageName = values[2];
            stageData.nextStageID = values[3];
            stageData.isBossStage = bool.Parse(values[4]);
            stageData.isMidBossStage = bool.Parse(values[5]);

            // 플레이어 스폰 위치
            if (values.Length >= 9)
            {
                float spawnX = float.Parse(values[6]);
                float spawnY = float.Parse(values[7]);
                float spawnZ = float.Parse(values[8]);
                stageData.playerSpawnPosition = new Vector3(spawnX, spawnY, spawnZ);
            }
            else
            {
                stageData.playerSpawnPosition = Vector3.zero; // 기본값
            }

            // 포탈 스폰 위치 (있을 경우)
            if (values.Length >= 12)
            {
                float portalX = float.Parse(values[9]);
                float portalY = float.Parse(values[10]);
                float portalZ = float.Parse(values[11]);
                stageData.portalSpawnPosition = new Vector3(portalX, portalY, portalZ);
            }
            else
            {
                stageData.portalSpawnPosition = Vector3.zero; // 기본값
            }

            result[stageData.stageID] = stageData;
        }

        return result;
    }

    // 기존 통합 StageMonsters.csv에서 몬스터 스폰 정보 로드
    private async Task<Dictionary<string, List<MonsterSpawnInfo>>> LoadMonsterSpawnsFromMainCSV()
    {
        Dictionary<string, List<MonsterSpawnInfo>> result = new Dictionary<string, List<MonsterSpawnInfo>>();

        // CSV 파일 경로
        string path = Path.Combine(Application.streamingAssetsPath, STAGE_MONSTERS_CSV);

        // CSV 파일 존재 확인
        if (!File.Exists(path))
        {
            Debug.LogWarning($"기본 몬스터 스폰 CSV 파일을 찾을 수 없음: {path}");
            return result;
        }

        // CSV 파일 읽기
        string csv = await File.ReadAllTextAsync(path);
        string[] lines = csv.Split('\n');

        // 헤더 건너뛰기
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 7) continue;

            string stageID = values[0];

            // 몬스터 스폰 정보 생성
            MonsterSpawnInfo spawnInfo = new MonsterSpawnInfo();
            spawnInfo.monsterID = int.Parse(values[1]);
            spawnInfo.isFixedPosition = bool.Parse(values[2]);

            // 위치 정보
            float x = float.Parse(values[3]);
            float y = float.Parse(values[4]);
            float z = float.Parse(values[5]);
            spawnInfo.position = new Vector3(x, y, z);

            // 가중치
            spawnInfo.spawnWeight = float.Parse(values[6]);

            // 결과에 추가
            if (!result.ContainsKey(stageID))
            {
                result[stageID] = new List<MonsterSpawnInfo>();
            }
            result[stageID].Add(spawnInfo);
        }

        return result;
    }

    // 기존 방식 데이터 병합
    private void MergeBaseStageData(Dictionary<string, StageData> stages, Dictionary<string, List<MonsterSpawnInfo>> monsterSpawns)
    {
        foreach (var stageEntry in stages)
        {
            string stageID = stageEntry.Key;
            StageData stageData = stageEntry.Value;

            // 몬스터 스폰 정보 추가
            if (monsterSpawns.TryGetValue(stageID, out List<MonsterSpawnInfo> monsters))
            {
                foreach (var monster in monsters)
                {
                    // ID가 1000 이상이면 보스로 간주하고 항상 고정 위치
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

            // 기본 랜덤 스폰 포인트 추가 (실제 환경에서는 레벨 디자인에 맞게 조정 필요)
            if (stageData.spawnPoints.Count == 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    float x = Random.Range(-10f, 10f);
                    float z = Random.Range(-10f, 10f);
                    stageData.spawnPoints.Add(new Vector3(x, 0f, z));
                }
            }

            // 캐시에 저장
            stageDataCache[stageID] = stageData;
        }
    }

    // 스테이지별 개별 CSV 파일 로드
    private async Task LoadStageSpecificCSVFiles()
    {
        string stageFolder = Path.Combine(Application.streamingAssetsPath, STAGE_FOLDER);
        
        // 폴더 존재 확인
        if (!Directory.Exists(stageFolder))
        {
            Debug.LogWarning($"스테이지 폴더가 없습니다: {stageFolder}");
            return;
        }

        // 1. 스테이지 기본 정보 파일 로드
        string[] stageFiles = Directory.GetFiles(stageFolder, "*_stage.csv");
        foreach (string stageFile in stageFiles)
        {
            try
            {
                // 파일 이름에서 스테이지 ID 추출
                string fileName = Path.GetFileNameWithoutExtension(stageFile);
                string stageID = fileName.Substring(0, fileName.Length - 6); // "_stage" 부분 제거

                // 스테이지 파일 읽기
                string csv = await File.ReadAllTextAsync(stageFile);
                string[] lines = csv.Split('\n');
                
                // 헤더 건너뛰기 (최소 2줄 필요)
                if (lines.Length < 2) continue;
                
                // 첫 번째 데이터 행 처리
                string line = lines[1].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] values = line.Split(',');
                if (values.Length < 12) continue; // 포탈 위치까지 포함해서 최소 12개 필요

                // 스테이지 데이터 생성 또는 업데이트
                StageData stageData;
                if (stageDataCache.TryGetValue(stageID, out stageData))
                {
                    // 기존 데이터 업데이트
                    stageData.chapterID = int.Parse(values[1]);
                    stageData.stageName = values[2];
                    stageData.nextStageID = values[3];
                    stageData.isBossStage = bool.Parse(values[4]);
                    stageData.isMidBossStage = bool.Parse(values[5]);
                }
                else
                {
                    // 새 데이터 생성
                    stageData = ScriptableObject.CreateInstance<StageData>();
                    stageData.stageID = stageID;
                    stageData.chapterID = int.Parse(values[1]);
                    stageData.stageName = values[2];
                    stageData.nextStageID = values[3];
                    stageData.isBossStage = bool.Parse(values[4]);
                    stageData.isMidBossStage = bool.Parse(values[5]);
                    stageDataCache[stageID] = stageData;
                }

                // 플레이어 스폰 위치
                float spawnX = float.Parse(values[6]);
                float spawnY = float.Parse(values[7]);
                float spawnZ = float.Parse(values[8]);
                stageData.playerSpawnPosition = new Vector3(spawnX, spawnY, spawnZ);

                // 포탈 스폰 위치
                float portalX = float.Parse(values[9]);
                float portalY = float.Parse(values[10]);
                float portalZ = float.Parse(values[11]);
                stageData.portalSpawnPosition = new Vector3(portalX, portalY, portalZ);

                Debug.Log($"스테이지 기본 정보 로드 완료: {stageID}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"스테이지 파일 로드 실패: {stageFile}, {e.Message}");
            }
        }

        // 2. 몬스터 스폰 정보 파일 로드
        string[] spawnFiles = Directory.GetFiles(stageFolder, "*_spawns.csv");
        foreach (string spawnFile in spawnFiles)
        {
            try
            {
                // 파일 이름에서 스테이지 ID 추출
                string fileName = Path.GetFileNameWithoutExtension(spawnFile);
                string stageID = fileName.Substring(0, fileName.Length - 7); // "_spawns" 부분 제거

                // 해당 스테이지 데이터 확인
                if (!stageDataCache.TryGetValue(stageID, out StageData stageData))
                {
                    Debug.LogWarning($"스폰 파일에 해당하는 스테이지 데이터가 없습니다: {stageID}");
                    continue;
                }

                // 기존 스폰 정보 초기화 (덮어쓰기)
                stageData.fixedSpawns.Clear();
                stageData.randomMonsters.Clear();
                stageData.spawnPoints.Clear();

                // 스폰 파일 읽기
                string csv = await File.ReadAllTextAsync(spawnFile);
                string[] lines = csv.Split('\n');
                
                // 헤더 건너뛰기
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] values = line.Split(',');
                    if (values.Length < 7) continue;

                    // CSV에서 스테이지 ID도 확인 (일치해야 함)
                    string csvStageID = values[0];
                    if (csvStageID != stageID) continue;

                    // 몬스터 스폰 정보 생성
                    MonsterSpawnInfo spawnInfo = new MonsterSpawnInfo();
                    spawnInfo.monsterID = int.Parse(values[1]);
                    spawnInfo.isFixedPosition = bool.Parse(values[2]);

                    // 위치 정보
                    float x = float.Parse(values[3]);
                    float y = float.Parse(values[4]);
                    float z = float.Parse(values[5]);
                    spawnInfo.position = new Vector3(x, y, z);

                    // 가중치
                    spawnInfo.spawnWeight = float.Parse(values[6]);

                    // 스폰 정보 추가
                    if (spawnInfo.monsterID >= 1000 || spawnInfo.isFixedPosition)
                    {
                        stageData.fixedSpawns.Add(spawnInfo);
                    }
                    else
                    {
                        stageData.randomMonsters.Add(spawnInfo);
                        
                        // 랜덤 스폰 위치로도 추가
                        stageData.spawnPoints.Add(spawnInfo.position);
                    }
                }

                // 랜덤 스폰 포인트가 너무 적으면 추가
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

                Debug.Log($"스테이지 몬스터 정보 로드 완료: {stageID}, 고정: {stageData.fixedSpawns.Count}개, 랜덤: {stageData.randomMonsters.Count}개");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"스폰 파일 로드 실패: {spawnFile}, {e.Message}");
            }
        }
    }

    // 스테이지 데이터 가져오기
    public StageData GetStageData(string stageID)
    {
        if (stageDataCache.TryGetValue(stageID, out StageData stageData))
            return stageData;

        Debug.LogWarning($"스테이지 데이터를 찾을 수 없음: {stageID}");
        return null;
    }

    // 개발용: 모든 스테이지 데이터 출력
    public void DebugPrintStageData()
    {
        foreach (var entry in stageDataCache)
        {
            StageData stage = entry.Value;
            Debug.Log($"스테이지: {stage.stageID} - {stage.stageName}");
            Debug.Log($"  챕터: {stage.chapterID}, 다음 스테이지: {stage.nextStageID}");
            Debug.Log($"  보스: {stage.isBossStage}, 중간보스: {stage.isMidBossStage}");
            Debug.Log($"  플레이어 스폰 위치: {stage.playerSpawnPosition}");
            Debug.Log($"  포탈 스폰 위치: {stage.portalSpawnPosition}");
            Debug.Log($"  고정 몬스터: {stage.fixedSpawns.Count}개");
            Debug.Log($"  랜덤 몬스터 풀: {stage.randomMonsters.Count}개");
            Debug.Log($"  스폰 포인트: {stage.spawnPoints.Count}개");
        }
    }
}