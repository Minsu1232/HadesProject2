using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class QuestInfo
{
    public int questID;         // ����Ʈ ���� ID
    public int chapter;         // é�� ��ȣ
    public string description;  // ����Ʈ ����
    public bool isComplete;     // �Ϸ� ����
    public string reward;       // ���� ������
}

// ����Ʈ ������ Ŭ���� 
[System.Serializable]
public class QuestData
{
    public List<QuestInfo> quests = new List<QuestInfo>();
}

public class QuestManager : Singleton<QuestManager>
{
    public void InitializeQuests()
    {
        string path = Path.Combine(Application.persistentDataPath, "SaveFiles", "quests.json");
        if (!File.Exists(path))
        {
            QuestData newData = new QuestData();

            // �ʱ� ����Ʈ ������ �߰�
            newData.quests.Add(new QuestInfo
            {
                questID = 1,
                chapter = 1,
                description = "Defeat the Chaos Legion",
                isComplete = false,
                reward = "dagger"
            });
            newData.quests.Add(new QuestInfo
            {
                questID = 2,
                chapter = 2,
                description = "Ŭ����� �η縶�� ��ȭ ����",
                isComplete = false,
                reward = "dagger"
            });


            // �� ���� �ʱ� ����Ʈ �߰� ����
            newData.quests.Add(new QuestInfo
            {
                questID = 3,
                chapter = 3,
                description = "Find the lost artifact",
                isComplete = false,
                reward = "staff"
            });
            newData.quests.Add(new QuestInfo
            {
                questID = 4,
                chapter = 4,
                description = "Ŭ����� ��ű��� �ູ ȹ��",
                isComplete = false,
                reward = "dagger"
            });


            string json = JsonUtility.ToJson(newData, true);
            File.WriteAllText(path, json);
        }
    }

    // ������: ����Ʈ �ε� �޼���
    public QuestData LoadQuests()
    {
        string path = Path.Combine(Application.persistentDataPath, "SaveFiles", "quests.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<QuestData>(json);
        }
        return null;
    }

    // ������: ����Ʈ ���� �޼���
    public void SaveQuests(QuestData questData)
    {
        string path = Path.Combine(Application.persistentDataPath, "SaveFiles", "quests.json");
        string json = JsonUtility.ToJson(questData, true);
        File.WriteAllText(path, json);
    }
}