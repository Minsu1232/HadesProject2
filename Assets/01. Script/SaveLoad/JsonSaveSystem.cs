using System.IO;
using UnityEngine;

// JSON ��� ���� �ý��� ����
public class JsonSaveSystem : ISaveSystem
{
    private int currentSlot = 0; // ���� ���õ� ���� (0, 1, 2)

    // ���� ���� ���� �޼���
    public void SetCurrentSlot(int slot)
    {
        if (slot >= 0 && slot <= 2)
        {
            currentSlot = slot;
        }
        else
        {
            Debug.LogError($"��ȿ���� ���� ���̺� ����: {slot}. 0-2 ������ ���̾�� �մϴ�.");
        }
    }

    // ���� ���� ��ȯ �޼���
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

            // FileStream�� StreamWriter�� ����Ͽ� ������ �÷��� ����
            using (FileStream fs = new FileStream(GetSavePath(fileName), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(json);
                    writer.Flush(); // ��������� ���� �÷���
                } // using ����� ����� �ڵ����� ����
            }

            // �߰�: ���� �ý��� ����ȭ ���� ����
            System.IO.Directory.GetFiles(Path.GetDirectoryName(GetSavePath(fileName)));

            Debug.Log($"������ �����: ���� {currentSlot}, {fileName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"������ ���� ����: ���� {currentSlot}, {fileName}, {e.Message}");
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
            Debug.LogError($"������ �ε� ����: ���� {currentSlot}, {fileName}, {e.Message}");
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
                Debug.Log($"������ ������: ���� {currentSlot}, {fileName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"������ ���� ����: ���� {currentSlot}, {fileName}, {e.Message}");
        }
    }

    // ������ ��� ������ ����
    public void DeleteSlot()
    {
        try
        {
            string slotDirectory = Path.Combine(Application.persistentDataPath, "SaveFiles", $"Slot{currentSlot}");
            if (Directory.Exists(slotDirectory))
            {
                Directory.Delete(slotDirectory, true);
                Debug.Log($"���� ������: {currentSlot}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"���� ���� ����: {currentSlot}, {e.Message}");
        }
    }

    // ���Կ� ���̺� �����Ͱ� �ִ��� Ȯ��
    public bool SlotExists()
    {
        string slotDirectory = Path.Combine(Application.persistentDataPath, "SaveFiles", $"Slot{currentSlot}");
        return Directory.Exists(slotDirectory) &&
               Directory.GetFiles(slotDirectory, "*.json").Length > 0;
    }
}