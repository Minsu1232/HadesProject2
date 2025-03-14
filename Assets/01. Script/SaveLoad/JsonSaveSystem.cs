using System.IO;
using UnityEngine;

// JSON 기반 저장 시스템 구현
public class JsonSaveSystem : ISaveSystem
{
    private string GetSavePath(string fileName) =>
        Path.Combine(Application.persistentDataPath, "SaveFiles", $"{fileName}.json");

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
            File.WriteAllText(GetSavePath(fileName), json);
            Debug.Log($"데이터 저장됨: {fileName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"데이터 저장 오류: {fileName}, {e.Message}");
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
            Debug.LogError($"데이터 로드 오류: {fileName}, {e.Message}");
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
                Debug.Log($"데이터 삭제됨: {fileName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"데이터 삭제 오류: {fileName}, {e.Message}");
        }
    }
}