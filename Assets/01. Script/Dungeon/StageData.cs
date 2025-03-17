using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StageData", menuName = "Soul Gatherer/Stage Data")]
public class StageData : ScriptableObject
{
    public string stageID;         // "1-1", "1-2" 등
    public int chapterID;          // 1 = 챕터 1
    public string stageName;       // 스테이지 이름
    public string nextStageID;     // 다음 스테이지 ID
    public bool isBossStage;       // 최종 보스 스테이지
    public bool isMidBossStage;    // 중간 보스 스테이지
    public Vector3 playerSpawnPosition; // 스폰위치
    public Vector3 portalSpawnPosition; // 포탈 생성 위치
    [Header("몬스터 설정")]
    public List<MonsterSpawnInfo> fixedSpawns = new List<MonsterSpawnInfo>();  // 보스나 특정 위치에 고정된 몬스터 
    public List<MonsterSpawnInfo> randomMonsters = new List<MonsterSpawnInfo>(); // 랜덤 위치에 스폰될 몬스터 풀
    public int totalRandomSpawns = 5;  // 랜덤 몬스터 스폰 수

    [Header("스폰 위치")]
    public List<Vector3> spawnPoints = new List<Vector3>();
}

[System.Serializable]
public class MonsterSpawnInfo
{
    public int monsterID;          // 몬스터 ID (1~999) 또는 보스 ID (1001+)
    public Vector3 position;       // 고정 위치 스폰용 (필요한 경우만)
    public float spawnWeight = 1f; // 랜덤 스폰 가중치
    public bool isFixedPosition;   // true인 경우 position 사용, false인 경우 랜덤 위치
}