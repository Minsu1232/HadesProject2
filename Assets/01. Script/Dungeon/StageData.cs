using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StageData", menuName = "Soul Gatherer/Stage Data")]
public class StageData : ScriptableObject
{
    public string stageID;         // "1-1", "1-2" ��
    public int chapterID;          // 1 = é�� 1
    public string stageName;       // �������� �̸�
    public string nextStageID;     // ���� �������� ID
    public bool isBossStage;       // ���� ���� ��������
    public bool isMidBossStage;    // �߰� ���� ��������
    public Vector3 playerSpawnPosition; // ������ġ
    public Vector3 portalSpawnPosition; // ��Ż ���� ��ġ
    [Header("���� ����")]
    public List<MonsterSpawnInfo> fixedSpawns = new List<MonsterSpawnInfo>();  // ������ Ư�� ��ġ�� ������ ���� 
    public List<MonsterSpawnInfo> randomMonsters = new List<MonsterSpawnInfo>(); // ���� ��ġ�� ������ ���� Ǯ
    public int totalRandomSpawns = 5;  // ���� ���� ���� ��

    [Header("���� ��ġ")]
    public List<Vector3> spawnPoints = new List<Vector3>();
}

[System.Serializable]
public class MonsterSpawnInfo
{
    public int monsterID;          // ���� ID (1~999) �Ǵ� ���� ID (1001+)
    public Vector3 position;       // ���� ��ġ ������ (�ʿ��� ��츸)
    public float spawnWeight = 1f; // ���� ���� ����ġ
    public bool isFixedPosition;   // true�� ��� position ���, false�� ��� ���� ��ġ
}