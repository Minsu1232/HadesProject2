using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class QuestInfo
{
    public int questID;         // 퀘스트 고유 ID
    public int chapter;         // 챕터 번호
    public string description;  // 퀘스트 설명
    public bool isComplete;     // 완료 여부
    public string reward;       // 보상 아이템
}

// 퀘스트 데이터 클래스 
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

            // 초기 퀘스트 데이터 추가
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
                description = "클리어시 두루마리 강화 가능",
                isComplete = false,
                reward = "dagger"
            });


            // 더 많은 초기 퀘스트 추가 가능
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
                description = "클리어시 사신기의 축복 획득",
                isComplete = false,
                reward = "dagger"
            });


            string json = JsonUtility.ToJson(newData, true);
            File.WriteAllText(path, json);
        }
    }

    // 선택적: 퀘스트 로드 메서드
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

    // 선택적: 퀘스트 저장 메서드
    public void SaveQuests(QuestData questData)
    {
        string path = Path.Combine(Application.persistentDataPath, "SaveFiles", "quests.json");
        string json = JsonUtility.ToJson(questData, true);
        File.WriteAllText(path, json);
    }
}