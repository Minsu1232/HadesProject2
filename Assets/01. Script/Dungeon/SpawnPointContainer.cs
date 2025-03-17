// SpawnPointContainer.cs
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SpawnPointContainer : MonoBehaviour
{
    [Header("스테이지 정보")]
    public string stageID = "1_1";
    public string stageName = "Stage 1-1";

    [Header("플레이어 스폰")]
    public Transform playerSpawnPoint;

    [Header("포탈 스폰")]
    public Transform portalSpawnPoint;

    // 에디터에서만 사용할 메서드
    // SpawnPointContainer.cs의 확장 메서드
    public void ExportToCSV()
    {
#if UNITY_EDITOR
        string path = EditorUtility.SaveFilePanel(
            "스폰 데이터 저장",
            Application.dataPath,
            $"{stageID}_spawns.csv",
            "csv");

        if (string.IsNullOrEmpty(path)) return;

        using (StreamWriter writer = new StreamWriter(path))
        {
            // 헤더 작성
            writer.WriteLine("StageID,MonsterID,IsFixedPosition,PositionX,PositionY,PositionZ,SpawnWeight");

            // 고정 스폰 저장
            Transform fixedContainer = transform.Find("FixedSpawns");
            if (fixedContainer != null)
            {
                foreach (Transform child in fixedContainer)
                {
                    SpawnPointMarker marker = child.GetComponent<SpawnPointMarker>();
                    if (marker != null)
                    {
                        Vector3 pos = child.position;
                        writer.WriteLine($"{stageID},{marker.monsterID},true,{pos.x},{pos.y},{pos.z},{1.0f}");
                    }
                }
            }

            // 랜덤 스폰 저장
            Transform randomContainer = transform.Find("RandomSpawns");
            if (randomContainer != null)
            {
                foreach (Transform child in randomContainer)
                {
                    SpawnPointMarker marker = child.GetComponent<SpawnPointMarker>();
                    if (marker != null)
                    {
                        Vector3 pos = child.position;
                        writer.WriteLine($"{stageID},{marker.monsterID},false,{pos.x},{pos.y},{pos.z},{marker.spawnWeight}");
                    }
                }
            }

            // 플레이어 스폰 위치 저장 (Stage.csv 별도 처리)
            string stagePath = Path.Combine(Path.GetDirectoryName(path), $"{stageID}_stage.csv");
            using (StreamWriter stageWriter = new StreamWriter(stagePath))
            {
                stageWriter.WriteLine("StageID,ChapterID,StageName,NextStageID,IsBossStage,IsMidBossStage,SpawnPosX,SpawnPosY,SpawnPosZ,PortalPosX,PortalPosY,PortalPosZ");

                // 챕터 ID 추출 (예: "1_2"에서 "1" 추출)
                string chapterID = "1";
                if (stageID.Contains("_"))
                {
                    chapterID = stageID.Split('_')[0];
                }

                Vector3 playerPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
                Vector3 portalPos = portalSpawnPoint != null ? portalSpawnPoint.position : Vector3.zero;

                string nextStageID = "";
                int stageNum = int.Parse(stageID.Split('_')[1]);
                nextStageID = $"{chapterID}_{stageNum + 1}";

                bool isBossStage = stageID.EndsWith("_10"); // 예: 1_10은 보스 스테이지
                bool isMidBossStage = stageID.EndsWith("_5"); // 예: 1_5는 중간보스 스테이지

                stageWriter.WriteLine($"{stageID},{chapterID},{stageName},{nextStageID},{isBossStage},{isMidBossStage},{playerPos.x},{playerPos.y},{playerPos.z},{portalPos.x},{portalPos.y},{portalPos.z}");
            }
        }

        EditorUtility.DisplayDialog("내보내기 완료", $"스폰 데이터가 {path}에 저장되었습니다", "확인");
#endif
    }

    public void ImportFromCSV()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel(
            "스폰 데이터 불러오기",
            Application.dataPath,
            "csv");

        if (string.IsNullOrEmpty(path)) return;

        // 기존 스폰 포인트 제거
        ClearSpawnPoints();

        // CSV 파일 로드
        string[] lines = File.ReadAllLines(path);
        if (lines.Length <= 1) return; // 헤더만 있는 경우

        for (int i = 1; i < lines.Length; i++) // 첫 줄(헤더) 건너뛰기
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 7) continue;

            // 데이터 파싱
            string csvStageID = values[0];
            int monsterID = int.Parse(values[1]);
            bool isFixed = bool.Parse(values[2]);
            float x = float.Parse(values[3]);
            float y = float.Parse(values[4]);
            float z = float.Parse(values[5]);
            float weight = float.Parse(values[6]);

            // 해당 스테이지 데이터만 처리
            if (csvStageID != stageID) continue;

            // 스폰 포인트 생성
            CreateSpawnPoint(new Vector3(x, y, z), monsterID, isFixed, weight);
        }

        // 플레이어 스폰 위치도 불러오기
        string stagePath = Path.Combine(Path.GetDirectoryName(path), $"{stageID}_stage.csv");
        if (File.Exists(stagePath))
        {
            string[] stageLines = File.ReadAllLines(stagePath);
            if (stageLines.Length > 1)
            {
                string[] values = stageLines[1].Split(',');
                if (values.Length >= 12)
                {
                    // 플레이어 스폰 위치
                    float playerX = float.Parse(values[6]);
                    float playerY = float.Parse(values[7]);
                    float playerZ = float.Parse(values[8]);

                    // 포탈 스폰 위치
                    float portalX = float.Parse(values[9]);
                    float portalY = float.Parse(values[10]);
                    float portalZ = float.Parse(values[11]);

                    // 플레이어 스폰 설정
                    if (playerSpawnPoint != null)
                    {
                        playerSpawnPoint.position = new Vector3(playerX, playerY, playerZ);
                    }

                    // 포탈 스폰 설정
                    if (portalSpawnPoint != null)
                    {
                        portalSpawnPoint.position = new Vector3(portalX, portalY, portalZ);
                    }
                }
            }
        }

        EditorUtility.DisplayDialog("가져오기 완료", "스폰 데이터를 성공적으로 불러왔습니다", "확인");
#endif
    }

    private void ClearSpawnPoints()
    {
        Transform fixedContainer = transform.Find("FixedSpawns");
        Transform randomContainer = transform.Find("RandomSpawns");

        if (fixedContainer != null)
        {
            foreach (Transform child in fixedContainer)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        if (randomContainer != null)
        {
            foreach (Transform child in randomContainer)
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    private void CreateSpawnPoint(Vector3 position, int monsterID, bool isFixed, float weight)
    {
#if UNITY_EDITOR
        SpawnPointMarker.SpawnType spawnType = isFixed ?
            SpawnPointMarker.SpawnType.Fixed : SpawnPointMarker.SpawnType.Random;

        string objName = isFixed ? $"Fixed_{monsterID}" : $"Random_{monsterID}";
        GameObject spawnObj = new GameObject(objName);
        spawnObj.transform.position = position;

        // 부모 설정
        Transform parent = isFixed ?
            transform.Find("FixedSpawns") : transform.Find("RandomSpawns");
        spawnObj.transform.parent = parent;

        // 마커 컴포넌트 추가
        SpawnPointMarker marker = spawnObj.AddComponent<SpawnPointMarker>();
        marker.spawnType = spawnType;
        marker.monsterID = monsterID;
        marker.spawnWeight = weight;
        marker.isBoss = monsterID >= 1000; // ID가 1000 이상이면 보스로 간주
        marker.gizmoColor = isFixed ? Color.red : Color.blue;
#endif
    }
}