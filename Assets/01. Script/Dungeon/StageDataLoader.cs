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
            // CSV 파일 로드 및 파싱
            Dictionary<string, StageData> stages = await LoadStagesFromCSV();
            Dictionary<string, List<MonsterSpawnInfo>> monsterSpawns = await LoadMonsterSpawnsFromCSV();

            // 데이터 결합
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

                // 기본 스폰 포인트 추가 (실제 환경에서는 레벨 디자인에 맞게 조정 필요)
                for (int i = 0; i < 10; i++)
                {
                    float x = Random.Range(-10f, 10f);
                    float z = Random.Range(-10f, 10f);
                    stageData.spawnPoints.Add(new Vector3(x, 0f, z));
                }

                // 캐시에 저장
                stageDataCache[stageID] = stageData;
            }

            Debug.Log($"스테이지 데이터 로딩 완료: {stageDataCache.Count}개 스테이지");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"스테이지 데이터 로딩 실패: {e.Message}");
        }
    }

    public StageData GetStageData(string stageID)
    {
        if (stageDataCache.TryGetValue(stageID, out StageData stageData))
            return stageData;

        Debug.LogWarning($"스테이지 데이터를 찾을 수 없음: {stageID}");
        return null;
    }

    // CSV에서 스테이지 기본 정보 로드
    private async Task<Dictionary<string, StageData>> LoadStagesFromCSV()
    {
        Dictionary<string, StageData> result = new Dictionary<string, StageData>();

        // CSV 파일 경로
        string path = Path.Combine(Application.streamingAssetsPath, STAGES_CSV);

        // CSV 파일 로컬 복사 (필요한 경우)
        if (!File.Exists(path))
        {
            Debug.LogError($"스테이지 CSV 파일을 찾을 수 없음: {path}");
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

            result[stageData.stageID] = stageData;
        }

        return result;
    }

    // CSV에서 몬스터 스폰 정보 로드
    private async Task<Dictionary<string, List<MonsterSpawnInfo>>> LoadMonsterSpawnsFromCSV()
    {
        Dictionary<string, List<MonsterSpawnInfo>> result = new Dictionary<string, List<MonsterSpawnInfo>>();

        // CSV 파일 경로
        string path = Path.Combine(Application.streamingAssetsPath, STAGE_MONSTERS_CSV);

        // CSV 파일 존재 확인
        if (!File.Exists(path))
        {
            Debug.LogError($"몬스터 스폰 CSV 파일을 찾을 수 없음: {path}");
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

    // 개발용: 스테이지 데이터 출력
    public void DebugPrintStageData()
    {
        foreach (var entry in stageDataCache)
        {
            StageData stage = entry.Value;
            Debug.Log($"스테이지: {stage.stageID} - {stage.stageName}");
            Debug.Log($"  고정 몬스터: {stage.fixedSpawns.Count}개");
            Debug.Log($"  랜덤 몬스터 풀: {stage.randomMonsters.Count}개");
            Debug.Log($"  스폰 포인트: {stage.spawnPoints.Count}개");
        }
    }
}