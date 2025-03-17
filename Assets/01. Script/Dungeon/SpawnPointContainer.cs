// SpawnPointContainer.cs
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SpawnPointContainer : MonoBehaviour
{
    [Header("�������� ����")]
    public string stageID = "1_1";
    public string stageName = "Stage 1-1";

    [Header("�÷��̾� ����")]
    public Transform playerSpawnPoint;

    [Header("��Ż ����")]
    public Transform portalSpawnPoint;

    // �����Ϳ����� ����� �޼���
    // SpawnPointContainer.cs�� Ȯ�� �޼���
    public void ExportToCSV()
    {
#if UNITY_EDITOR
        string path = EditorUtility.SaveFilePanel(
            "���� ������ ����",
            Application.dataPath,
            $"{stageID}_spawns.csv",
            "csv");

        if (string.IsNullOrEmpty(path)) return;

        using (StreamWriter writer = new StreamWriter(path))
        {
            // ��� �ۼ�
            writer.WriteLine("StageID,MonsterID,IsFixedPosition,PositionX,PositionY,PositionZ,SpawnWeight");

            // ���� ���� ����
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

            // ���� ���� ����
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

            // �÷��̾� ���� ��ġ ���� (Stage.csv ���� ó��)
            string stagePath = Path.Combine(Path.GetDirectoryName(path), $"{stageID}_stage.csv");
            using (StreamWriter stageWriter = new StreamWriter(stagePath))
            {
                stageWriter.WriteLine("StageID,ChapterID,StageName,NextStageID,IsBossStage,IsMidBossStage,SpawnPosX,SpawnPosY,SpawnPosZ,PortalPosX,PortalPosY,PortalPosZ");

                // é�� ID ���� (��: "1_2"���� "1" ����)
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

                bool isBossStage = stageID.EndsWith("_10"); // ��: 1_10�� ���� ��������
                bool isMidBossStage = stageID.EndsWith("_5"); // ��: 1_5�� �߰����� ��������

                stageWriter.WriteLine($"{stageID},{chapterID},{stageName},{nextStageID},{isBossStage},{isMidBossStage},{playerPos.x},{playerPos.y},{playerPos.z},{portalPos.x},{portalPos.y},{portalPos.z}");
            }
        }

        EditorUtility.DisplayDialog("�������� �Ϸ�", $"���� �����Ͱ� {path}�� ����Ǿ����ϴ�", "Ȯ��");
#endif
    }

    public void ImportFromCSV()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel(
            "���� ������ �ҷ�����",
            Application.dataPath,
            "csv");

        if (string.IsNullOrEmpty(path)) return;

        // ���� ���� ����Ʈ ����
        ClearSpawnPoints();

        // CSV ���� �ε�
        string[] lines = File.ReadAllLines(path);
        if (lines.Length <= 1) return; // ����� �ִ� ���

        for (int i = 1; i < lines.Length; i++) // ù ��(���) �ǳʶٱ�
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 7) continue;

            // ������ �Ľ�
            string csvStageID = values[0];
            int monsterID = int.Parse(values[1]);
            bool isFixed = bool.Parse(values[2]);
            float x = float.Parse(values[3]);
            float y = float.Parse(values[4]);
            float z = float.Parse(values[5]);
            float weight = float.Parse(values[6]);

            // �ش� �������� �����͸� ó��
            if (csvStageID != stageID) continue;

            // ���� ����Ʈ ����
            CreateSpawnPoint(new Vector3(x, y, z), monsterID, isFixed, weight);
        }

        // �÷��̾� ���� ��ġ�� �ҷ�����
        string stagePath = Path.Combine(Path.GetDirectoryName(path), $"{stageID}_stage.csv");
        if (File.Exists(stagePath))
        {
            string[] stageLines = File.ReadAllLines(stagePath);
            if (stageLines.Length > 1)
            {
                string[] values = stageLines[1].Split(',');
                if (values.Length >= 12)
                {
                    // �÷��̾� ���� ��ġ
                    float playerX = float.Parse(values[6]);
                    float playerY = float.Parse(values[7]);
                    float playerZ = float.Parse(values[8]);

                    // ��Ż ���� ��ġ
                    float portalX = float.Parse(values[9]);
                    float portalY = float.Parse(values[10]);
                    float portalZ = float.Parse(values[11]);

                    // �÷��̾� ���� ����
                    if (playerSpawnPoint != null)
                    {
                        playerSpawnPoint.position = new Vector3(playerX, playerY, playerZ);
                    }

                    // ��Ż ���� ����
                    if (portalSpawnPoint != null)
                    {
                        portalSpawnPoint.position = new Vector3(portalX, portalY, portalZ);
                    }
                }
            }
        }

        EditorUtility.DisplayDialog("�������� �Ϸ�", "���� �����͸� ���������� �ҷ��Խ��ϴ�", "Ȯ��");
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

        // �θ� ����
        Transform parent = isFixed ?
            transform.Find("FixedSpawns") : transform.Find("RandomSpawns");
        spawnObj.transform.parent = parent;

        // ��Ŀ ������Ʈ �߰�
        SpawnPointMarker marker = spawnObj.AddComponent<SpawnPointMarker>();
        marker.spawnType = spawnType;
        marker.monsterID = monsterID;
        marker.spawnWeight = weight;
        marker.isBoss = monsterID >= 1000; // ID�� 1000 �̻��̸� ������ ����
        marker.gizmoColor = isFixed ? Color.red : Color.blue;
#endif
    }
}