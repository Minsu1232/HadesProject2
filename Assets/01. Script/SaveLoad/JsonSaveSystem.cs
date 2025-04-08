using System.IO;
using UnityEngine;

// JSON 기반 저장 시스템 구현
public class JsonSaveSystem : ISaveSystem
{
    private int currentSlot = 0; // 현재 선택된 슬롯 (0, 1, 2)

    // 현재 슬롯 설정 메서드
    public void SetCurrentSlot(int slot)
    {
        if (slot >= 0 && slot <= 2)
        {
            currentSlot = slot;
        }
        else
        {
            Debug.LogError($"유효하지 않은 세이브 슬롯: {slot}. 0-2 사이의 값이어야 합니다.");
        }
    }

    // 현재 슬롯 반환 메서드
    public int GetCurrentSlot()
    {
        return currentSlot;
    }

    private string GetSavePath(string fileName) =>
        Path.Combine(Application.persistentDataPath, "SaveFiles", $"Slot{currentSlot}", $"{fileName}.json");

    public void SaveData<T>(T data, string fileName) where T : class
    {
        try
        {
            string directoryPath = Path.GetDirectoryName(GetSavePath(fileName));
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string json = JsonUtility.ToJson(data, true);

            // FileStream과 StreamWriter를 사용하여 강제로 플러시 수행
            using (FileStream fs = new FileStream(GetSavePath(fileName), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(json);
                    writer.Flush(); // 명시적으로 버퍼 플러시
                } // using 블록을 벗어나면 자동으로 닫힘
            }

            // 추가: 파일 시스템 동기화 강제 수행
            System.IO.Directory.GetFiles(Path.GetDirectoryName(GetSavePath(fileName)));

            Debug.Log($"데이터 저장됨: 슬롯 {currentSlot}, {fileName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"데이터 저장 실패: 슬롯 {currentSlot}, {fileName}, {e.Message}");
        }
    }

    public T LoadData<T>(string fileName) where T : class, new()
    {
        try
        {
            string path = GetSavePath(fileName);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<T>(json);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"데이터 로드 실패: 슬롯 {currentSlot}, {fileName}, {e.Message}");
        }

        return new T();
    }

    public bool HasData(string fileName)
    {
        return File.Exists(GetSavePath(fileName));
    }

    public void DeleteData(string fileName)
    {
        try
        {
            string path = GetSavePath(fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"데이터 삭제됨: 슬롯 {currentSlot}, {fileName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"데이터 삭제 실패: 슬롯 {currentSlot}, {fileName}, {e.Message}");
        }
    }

    // 슬롯의 모든 데이터 삭제
    public void DeleteSlot()
    {
        try
        {
            string slotDirectory = Path.Combine(Application.persistentDataPath, "SaveFiles", $"Slot{currentSlot}");
            if (Directory.Exists(slotDirectory))
            {
                Directory.Delete(slotDirectory, true);
                Debug.Log($"슬롯 삭제됨: {currentSlot}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"슬롯 삭제 실패: {currentSlot}, {e.Message}");
        }
    }

    // 슬롯에 세이브 데이터가 있는지 확인
    public bool SlotExists()
    {
        string slotDirectory = Path.Combine(Application.persistentDataPath, "SaveFiles", $"Slot{currentSlot}");
        return Directory.Exists(slotDirectory) &&
               Directory.GetFiles(slotDirectory, "*.json").Length > 0;
    }
}