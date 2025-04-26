using System;
using System.IO;
using UnityEngine;
using Steamworks;

// ���� Ŭ���带 ����ϴ� ���� �ý��� ����
public class SteamCloudSaveSystem : ISaveSystem 
{
    private const string CloudFilenamePrefix = "cloud_";
    private int currentSlot = 0;

    // ���� ���� �ý��� (�����)
    private JsonSaveSystem localSaveSystem;

    public SteamCloudSaveSystem()
    {
        localSaveSystem = new JsonSaveSystem();
    }

    public void SetCurrentSlot(int slot)
    {
        currentSlot = slot;
        localSaveSystem.SetCurrentSlot(slot);
    }

    public int GetCurrentSlot()
    {
        return currentSlot;
    }

    // Ŭ���� ���ϸ� ����: cloud_slot{���Թ�ȣ}_{������ID}.json
    private string GetCloudFileName(string dataId)
    {
        return $"{CloudFilenamePrefix}slot{currentSlot}_{dataId}.json";
    }

    // ������ ���� (���� + Ŭ����)
    public void SaveData<T>(T data, string dataId) where T : class
    {
        try
        {
            // ���ÿ� �켱 ���� (���)
            localSaveSystem.SaveData(data, dataId);

            // ���� �Ŵ��� ���� �� �ʱ�ȭ ���� Ȯ��
            if (SteamworksManager.Instance == null || !SteamworksManager.Instance.Initialized)
            {
                Debug.LogWarning("���� �Ŵ����� �ʱ�ȭ���� �ʾҽ��ϴ�. ���ÿ��� �����մϴ�.");
                return;
            }

            // ������ ����ȭ
            string json = JsonUtility.ToJson(data, true);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

            // Ŭ���忡 ����
            string cloudFileName = GetCloudFileName(dataId);
            bool success = SteamworksManager.Instance.SaveToCloud(cloudFileName, bytes);

            if (success)
            {
                Debug.Log($"�����Ͱ� Ŭ���忡 �����: {cloudFileName}, ũ��: {bytes.Length} ����Ʈ");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ŭ���� ���� �� ���� �߻�: {e.Message}");
        }
    }

    // ������ �ε� (�켱 Ŭ����, ���� �� ����)
    public T LoadData<T>(string dataId) where T : class, new()
    {
        try
        {
            // ���� �Ŵ��� ���� �� �ʱ�ȭ ���� Ȯ��
            if (SteamworksManager.Instance == null || !SteamworksManager.Instance.Initialized)
            {
                Debug.LogWarning("���� �Ŵ����� �ʱ�ȭ���� �ʾҽ��ϴ�. ���ÿ����� �ε��մϴ�.");
                return localSaveSystem.LoadData<T>(dataId);
            }

            // Ŭ���忡�� �ε� �õ�
            string cloudFileName = GetCloudFileName(dataId);
            byte[] data = SteamworksManager.Instance.LoadFromCloud(cloudFileName);

            if (data != null && data.Length > 0)
            {
                string json = System.Text.Encoding.UTF8.GetString(data);
                T result = JsonUtility.FromJson<T>(json);

                if (result != null)
                {
                    Debug.Log($"�����Ͱ� Ŭ���忡�� �ε��: {cloudFileName}");

                    // Ŭ���� �����͸� ���ÿ��� ���� (����ȭ ����)
                    SyncCloudToLocal(result, dataId);

                    return result;
                }
            }

            Debug.Log($"Ŭ���忡�� �����͸� ã�� �� ����: {cloudFileName}, ���ÿ��� �õ��մϴ�.");

            // Ŭ���忡�� ������ ��� ���ÿ��� �ε�
            return localSaveSystem.LoadData<T>(dataId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Ŭ���� �ε� �� ���� �߻�: {e.Message}, ���ÿ��� �õ��մϴ�.");
            return localSaveSystem.LoadData<T>(dataId);
        }
    }

    // Ŭ���� �����͸� ���÷� ����ȭ
    private void SyncCloudToLocal<T>(T data, string dataId) where T : class
    {
        try
        {
            localSaveSystem.SaveData(data, dataId);
            Debug.Log($"Ŭ���� �����Ͱ� ���÷� ����ȭ��: {dataId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Ŭ����->���� ����ȭ �� ���� �߻�: {e.Message}");
        }
    }

    // ������ ���� ���� Ȯ�� (Ŭ���� �Ǵ� ����)
    public bool HasData(string dataId)
    {
        // ���� �Ŵ��� �ʱ�ȭ Ȯ��
        if (SteamworksManager.Instance != null && SteamworksManager.Instance.Initialized)
        {
            // Ŭ���� ���� ���� Ȯ��
            string cloudFileName = GetCloudFileName(dataId);
            bool cloudExists = SteamRemoteStorage.FileExists(cloudFileName);

            if (cloudExists)
            {
                return true;
            }
        }

        // Ŭ���忡 ������ ���� Ȯ��
        return localSaveSystem.HasData(dataId);
    }

    // ������ ���� (Ŭ���� + ����)
    public void DeleteData(string dataId)
    {
        // ���� ������ ����
        localSaveSystem.DeleteData(dataId);

        // ���� �Ŵ��� �ʱ�ȭ Ȯ��
        if (SteamworksManager.Instance != null && SteamworksManager.Instance.Initialized)
        {
            // Ŭ���� ���� ����
            string cloudFileName = GetCloudFileName(dataId);
            bool success = SteamworksManager.Instance.DeleteCloudFile(cloudFileName);

            if (success)
            {
                Debug.Log($"Ŭ���� ������ ������: {cloudFileName}");
            }
        }
    }

    // ���� ���� ����
    public void DeleteSlot()
    {
        Debug.Log($"���� {currentSlot} ���� ���� (���� �� Ŭ����)");

        // ���� ���� ����
        localSaveSystem.DeleteSlot();

        // ���� �Ŵ��� �ʱ�ȭ Ȯ��
        if (SteamworksManager.Instance != null && SteamworksManager.Instance.Initialized)
        {
            try
            {
                // Ŭ���� ���� ��� ��������
                string[] cloudFiles = SteamworksManager.Instance.GetCloudFileList();
                Debug.Log($"Ŭ���� ���� ��� ������: {cloudFiles.Length}�� ����");

                // ���� ������ ��� ���� ã�� ����
                string slotPrefix = $"{CloudFilenamePrefix}slot{currentSlot}_";
                int deletedCount = 0;

                foreach (string fileName in cloudFiles)
                {
                    if (fileName.StartsWith(slotPrefix))
                    {
                        Debug.Log($"Ŭ���� ���� ���� �õ�: {fileName}");
                        bool success = SteamworksManager.Instance.DeleteCloudFile(fileName);

                        if (success)
                        {
                            deletedCount++;
                            Debug.Log($"Ŭ���� ���� ���� ���� ����: {fileName}");
                        }
                        else
                        {
                            Debug.LogError($"Ŭ���� ���� ���� ���� ����: {fileName}");
                        }
                    }
                }

                // ��Ÿ�����Ϳ����� ���� ���� �ʱ�ȭ
                // ��Ÿ������ ������ ������ �������� ���� �� �����Ƿ� ������ �ʱ�ȭ
                UpdateMetadataForDeletedSlot();

                Debug.Log($"Ŭ���忡�� �� {deletedCount}�� ���� ������");
            }
            catch (Exception e)
            {
                Debug.LogError($"Ŭ���� ���� ���� �� ���� �߻�: {e.Message}");
            }
        }
    }
    // ���� �߰��� �޼���
    private void UpdateMetadataForDeletedSlot()
    {
        try
        {
            // ��ü ��Ÿ������ ���� �̸�
            string metadataFileName = $"{CloudFilenamePrefix}slotMetadata.json";

            // Ŭ���忡�� ��Ÿ������ ���� �ε�
            byte[] data = SteamworksManager.Instance.LoadFromCloud(metadataFileName);
            if (data != null && data.Length > 0)
            {
                string json = System.Text.Encoding.UTF8.GetString(data);

                // ��Ÿ�����Ϳ��� ���� ���� ���� �ʱ�ȭ
                if (json.Contains("slots") && json.Contains("playerName"))
                {
                    // ��Ÿ������ ���� ������ ������ȭ
                    // ���� ������ �°� ���� �ʿ�
                    SaveManager.SlotMetadataWrapper wrapper =
                        JsonUtility.FromJson<SaveManager.SlotMetadataWrapper>(json);

                    if (wrapper != null && wrapper.slots != null &&
                        currentSlot < wrapper.slots.Count && currentSlot >= 0)
                    {
                        // ���� ��Ÿ������ �ʱ�ȭ
                        wrapper.slots[currentSlot] = new SaveManager.SlotMetadata();

                        // �ٽ� Ŭ���忡 ����
                        string updatedJson = JsonUtility.ToJson(wrapper);
                        byte[] updatedData = System.Text.Encoding.UTF8.GetBytes(updatedJson);

                        bool success = SteamworksManager.Instance.SaveToCloud(metadataFileName, updatedData);
                        Debug.Log($"Ŭ���� ��Ÿ������ ������Ʈ {(success ? "����" : "����")}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"��Ÿ������ ������Ʈ �� ����: {e.Message}");
        }
    }
    // ���� ���� ���� Ȯ��
    public bool SlotExists()
    {
        // ���� ���� ���� Ȯ��
        bool localExists = localSaveSystem.SlotExists();

        // ���� �Ŵ��� �ʱ�ȭ Ȯ��
        if (SteamworksManager.Instance != null && SteamworksManager.Instance.Initialized)
        {
            // Ŭ���� ���� ��� ��������
            string[] cloudFiles = SteamworksManager.Instance.GetCloudFileList();

            // ���� ������ ������ �ִ��� Ȯ��
            string slotPrefix = $"{CloudFilenamePrefix}slot{currentSlot}_";

            foreach (string fileName in cloudFiles)
            {
                if (fileName.StartsWith(slotPrefix))
                {
                    return true;
                }
            }
        }

        return localExists;
    }

    // ��� Ŭ���� ���� ������ ���÷� ����ȭ
    public void SyncAllCloudToLocal()
    {
        if (SteamworksManager.Instance == null || !SteamworksManager.Instance.Initialized)
        {
            Debug.LogWarning("���� �Ŵ����� �ʱ�ȭ���� �ʾҽ��ϴ�. ����ȭ�� �� �����ϴ�.");
            return;
        }

        try
        {
            // Ŭ���� ���� ��� ��������
            string[] cloudFiles = SteamworksManager.Instance.GetCloudFileList();

            foreach (string cloudFileName in cloudFiles)
            {
                if (cloudFileName.StartsWith(CloudFilenamePrefix))
                {
                    // Ŭ���� ���ϸ��� ���� ����
                    string fileNameWithoutPrefix = cloudFileName.Substring(CloudFilenamePrefix.Length);
                    int slotEndIndex = fileNameWithoutPrefix.IndexOf('_');

                    if (slotEndIndex > 0)
                    {
                        string slotStr = fileNameWithoutPrefix.Substring(0, slotEndIndex);
                        int slot = -1;

                        if (slotStr.StartsWith("slot") && int.TryParse(slotStr.Substring(4), out slot))
                        {
                            // ������ ID ����
                            string dataId = fileNameWithoutPrefix.Substring(slotEndIndex + 1);
                            if (dataId.EndsWith(".json"))
                            {
                                dataId = dataId.Substring(0, dataId.Length - 5);
                            }

                            // ���� ���� ����
                            int originalSlot = currentSlot;

                            // �ش� �������� ����
                            SetCurrentSlot(slot);

                            // ���� ��� ��� (JsonSaveSystem�� ������ ��� ���)
                            string localPath = Path.Combine(
                                Application.persistentDataPath,
                                "SaveFiles",
                                $"Slot{slot}",
                                $"{dataId}.json"
                            );

                            // Ŭ���忡�� ���÷� ����ȭ
                            SteamworksManager.Instance.SyncCloudToLocal(cloudFileName, localPath);

                            // ���� �������� ����
                            SetCurrentSlot(originalSlot);
                        }
                    }
                }
            }

            Debug.Log("��� Ŭ���� ������ ���÷� ����ȭ �Ϸ�");
        }
        catch (Exception e)
        {
            Debug.LogError($"Ŭ���� ����ȭ �� ���� �߻�: {e.Message}");
        }
    }

    // ��� ���� ���� ������ Ŭ����� ����ȭ
    public void SyncAllLocalToCloud()
    {
        if (SteamworksManager.Instance == null || !SteamworksManager.Instance.Initialized)
        {
            Debug.LogWarning("���� �Ŵ����� �ʱ�ȭ���� �ʾҽ��ϴ�. ����ȭ�� �� �����ϴ�.");
            return;
        }

        try
        {
            // ���� ���� ����
            int originalSlot = currentSlot;

            // ��� ���� Ž��
            for (int slot = 0; slot < 3; slot++) // �ִ� ���� ���� SaveManager�� �����ϰ� ����
            {
                SetCurrentSlot(slot);

                if (localSaveSystem.SlotExists())
                {
                    string slotPath = Path.Combine(
                        Application.persistentDataPath,
                        "SaveFiles",
                        $"Slot{slot}"
                    );

                    if (Directory.Exists(slotPath))
                    {
                        // ���� ������ ��� JSON ���� ��������
                        string[] jsonFiles = Directory.GetFiles(slotPath, "*.json");

                        foreach (string localPath in jsonFiles)
                        {
                            // ���ϸ��� ������ ID ����
                            string fileName = Path.GetFileNameWithoutExtension(localPath);
                            string cloudFileName = GetCloudFileName(fileName);

                            // ���ÿ��� Ŭ����� ����ȭ
                            SteamworksManager.Instance.SyncLocalToCloud(localPath, cloudFileName);
                        }
                    }
                }
            }

            // ���� �������� ����
            SetCurrentSlot(originalSlot);

            Debug.Log("��� ���� ������ Ŭ����� ����ȭ �Ϸ�");
        }
        catch (Exception e)
        {
            Debug.LogError($"����->Ŭ���� ����ȭ �� ���� �߻�: {e.Message}");
        }
    }

    // ���� ���� �� ���ð� Ŭ���� ������ �� �� �ֽ� ������ ���
    public void CompareAndSelectBestData()
    {
        if (SteamworksManager.Instance == null || !SteamworksManager.Instance.Initialized)
        {
            Debug.LogWarning("���� �Ŵ����� �ʱ�ȭ���� �ʾҽ��ϴ�. ������ �񱳸� ������ �� �����ϴ�.");
            return;
        }

        Debug.Log("���ð� Ŭ���� ������ �� ����...");

        try
        {
            // ���� ���� ��Ÿ������ ����ȭ
            SyncSlotMetadata();

            // �� ���Ը��� ��
            for (int slot = 0; slot < 3; slot++)
            {
                SetCurrentSlot(slot);

                // ���� ��Ÿ������ ���� Ȯ��
                string metadataId = "slotMetadata";
                string cloudMetadataFileName = GetCloudFileName(metadataId);

                // Ŭ���忡 ��Ÿ������ ������ �ִ��� Ȯ��
                bool cloudHasMetadata = SteamRemoteStorage.FileExists(cloudMetadataFileName);

                // ���ÿ� ��Ÿ������ ������ �ִ��� Ȯ��
                bool localHasMetadata = localSaveSystem.HasData(metadataId);

                // ��Ÿ������ �� �� ����ȭ
                if (cloudHasMetadata && localHasMetadata)
                {
                    // ���� �� ������ ��¥ �� �� �ֽ� �� ���
                    DateTime cloudLastModified = GetCloudFileLastModified(cloudMetadataFileName);
                    DateTime localLastModified = GetLocalFileLastModified(metadataId);

                    if (cloudLastModified > localLastModified)
                    {
                        // Ŭ���尡 �� �ֽ��̸� Ŭ���� ������ ���
                        Debug.Log($"���� {slot}: Ŭ���� �����Ͱ� �� �ֽ��Դϴ�. Ŭ���� �����͸� ���÷� ����ȭ�մϴ�.");
                        SyncCloudToLocalForSlot(slot);
                    }
                    else
                    {
                        // ������ �� �ֽ��̸� ���� ������ ���
                        Debug.Log($"���� {slot}: ���� �����Ͱ� �� �ֽ��Դϴ�. ���� �����͸� Ŭ����� ����ȭ�մϴ�.");
                        SyncLocalToCloudForSlot(slot);
                    }
                }
                else if (cloudHasMetadata)
                {
                    // Ŭ���忡�� ������ Ŭ���� ������ ���
                    Debug.Log($"���� {slot}: ���ÿ� �����Ͱ� �����ϴ�. Ŭ���� �����͸� ���÷� ����ȭ�մϴ�.");
                    SyncCloudToLocalForSlot(slot);
                }
                else if (localHasMetadata)
                {
                    // ���ÿ��� ������ ���� ������ ���
                    Debug.Log($"���� {slot}: Ŭ���忡 �����Ͱ� �����ϴ�. ���� �����͸� Ŭ����� ����ȭ�մϴ�.");
                    SyncLocalToCloudForSlot(slot);
                }
                // �� �� ������ �ƹ��͵� �� ��
            }

            Debug.Log("������ �� �� ����ȭ �Ϸ�");
        }
        catch (Exception e)
        {
            Debug.LogError($"������ �� �� ���� �߻�: {e.Message}");
        }
    }
    // ���� ��Ÿ������ ����ȭ �޼��� �߰�
    private void SyncSlotMetadata()
    {
        // ���� ��Ÿ������ ���� �̸� (SaveManager�� ���ǵ� �� ���)
        string metadataId = "slotMetadata";
        string cloudMetadataFileName = GetCloudFileName(metadataId);

        // Ŭ���忡 ��Ÿ������ ������ �ִ��� Ȯ��
        bool cloudHasMetadata = SteamRemoteStorage.FileExists(cloudMetadataFileName);

        // ���� ���
        string localPath = Path.Combine(
            Application.persistentDataPath,
            "SaveFiles",
            $"{metadataId}.json"
        );

        // ���ÿ� ��Ÿ������ ������ �ִ��� Ȯ��
        bool localHasMetadata = File.Exists(localPath);

        // ��Ÿ������ �� �� ����ȭ
        if (cloudHasMetadata && localHasMetadata)
        {
            // ���� �� ������ ��¥ ��
            DateTime cloudLastModified = GetCloudFileLastModified(cloudMetadataFileName);
            DateTime localLastModified = File.GetLastWriteTime(localPath);

            if (cloudLastModified > localLastModified)
            {
                // Ŭ���尡 �� �ֽ��̸� Ŭ���� ������ ���
                Debug.Log("���� ��Ÿ������: Ŭ���� �����Ͱ� �� �ֽ��Դϴ�. Ŭ���� �����͸� ���÷� ����ȭ�մϴ�.");
                SteamworksManager.Instance.SyncCloudToLocal(cloudMetadataFileName, localPath);
            }
            else
            {
                // ������ �� �ֽ��̸� ���� ������ ���
                Debug.Log("���� ��Ÿ������: ���� �����Ͱ� �� �ֽ��Դϴ�. ���� �����͸� Ŭ����� ����ȭ�մϴ�.");
                SteamworksManager.Instance.SyncLocalToCloud(localPath, cloudMetadataFileName);
            }
        }
        else if (cloudHasMetadata)
        {
            // Ŭ���忡�� ������ Ŭ���� ������ ���
            Debug.Log("���� ��Ÿ������: ���ÿ� �����Ͱ� �����ϴ�. Ŭ���� �����͸� ���÷� ����ȭ�մϴ�.");
            SteamworksManager.Instance.SyncCloudToLocal(cloudMetadataFileName, localPath);
        }
        else if (localHasMetadata)
        {
            // ���ÿ��� ������ ���� ������ ���
            Debug.Log("���� ��Ÿ������: Ŭ���忡 �����Ͱ� �����ϴ�. ���� �����͸� Ŭ����� ����ȭ�մϴ�.");
            SteamworksManager.Instance.SyncLocalToCloud(localPath, cloudMetadataFileName);
        }
        // �� �� ������ �ƹ��͵� �� ��
    }

    // Ư�� ������ Ŭ���� �����͸� ���÷� ����ȭ
    private void SyncCloudToLocalForSlot(int slot)
    {
        int originalSlot = currentSlot;
        SetCurrentSlot(slot);

        string[] cloudFiles = SteamworksManager.Instance.GetCloudFileList();
        string slotPrefix = $"{CloudFilenamePrefix}slot{slot}_";

        foreach (string cloudFileName in cloudFiles)
        {
            if (cloudFileName.StartsWith(slotPrefix))
            {
                // ������ ID ����
                string fileNameWithoutPrefix = cloudFileName.Substring(slotPrefix.Length);
                string dataId = fileNameWithoutPrefix;

                if (dataId.EndsWith(".json"))
                {
                    dataId = dataId.Substring(0, dataId.Length - 5);
                }

                // ���� ��� ���
                string localPath = Path.Combine(
                    Application.persistentDataPath,
                    "SaveFiles",
                    $"Slot{slot}",
                    $"{dataId}.json"
                );

                // Ŭ���忡�� ���÷� ����ȭ
                SteamworksManager.Instance.SyncCloudToLocal(cloudFileName, localPath);
            }
        }

        SetCurrentSlot(originalSlot);
    }

    // Ư�� ������ ���� �����͸� Ŭ����� ����ȭ
    private void SyncLocalToCloudForSlot(int slot)
    {
        int originalSlot = currentSlot;
        SetCurrentSlot(slot);

        string slotPath = Path.Combine(
            Application.persistentDataPath,
            "SaveFiles",
            $"Slot{slot}"
        );

        if (Directory.Exists(slotPath))
        {
            // ���� ������ ��� JSON ���� ��������
            string[] jsonFiles = Directory.GetFiles(slotPath, "*.json");

            foreach (string localPath in jsonFiles)
            {
                // ���ϸ��� ������ ID ����
                string fileName = Path.GetFileNameWithoutExtension(localPath);
                string cloudFileName = GetCloudFileName(fileName);

                // ���ÿ��� Ŭ����� ����ȭ
                SteamworksManager.Instance.SyncLocalToCloud(localPath, cloudFileName);
            }
        }

        SetCurrentSlot(originalSlot);
    }

    // Ŭ���� ������ ������ ���� �ð� ��������
    private DateTime GetCloudFileLastModified(string fileName)
    {
        // ���� API������ ���� ���� �ð��� ���� �������� ����� �������̹Ƿ�
        // ���� ������ �ε��Ͽ� ��Ÿ�����Ϳ��� �ð� ����
        try
        {
            byte[] data = SteamworksManager.Instance.LoadFromCloud(fileName);
            if (data != null && data.Length > 0)
            {
                string json = System.Text.Encoding.UTF8.GetString(data);

                // SlotMetadataWrapper���� ������ ���� �ð� ����
                // ���� ������ SaveManager�� SlotMetadata ������ ���� �ٸ� �� ����
                if (json.Contains("lastSaveTimeStr"))
                {
                    // ������ ���Խ� �Ǵ� ���ڿ� �Ľ����� ��¥ ����
                    int index = json.IndexOf("lastSaveTimeStr");
                    if (index > 0)
                    {
                        int valueStart = json.IndexOf("\"", index + 17) + 1;
                        int valueEnd = json.IndexOf("\"", valueStart);

                        if (valueStart > 0 && valueEnd > valueStart)
                        {
                            string dateStr = json.Substring(valueStart, valueEnd - valueStart);
                            if (DateTime.TryParse(dateStr, out DateTime result))
                            {
                                return result;
                            }
                        }
                    }
                }
            }

            // �Ľ� ���� �� �⺻�� ��ȯ
            return DateTime.MinValue;
        }
        catch (Exception e)
        {
            Debug.LogError($"Ŭ���� ���� ��¥ �Ľ� ����: {e.Message}");
            return DateTime.MinValue;
        }
    }

    // ���� ������ ������ ���� �ð� ��������
    private DateTime GetLocalFileLastModified(string dataId)
    {
        try
        {
            string path = Path.Combine(
                Application.persistentDataPath,
                "SaveFiles",
                $"Slot{currentSlot}",
                $"{dataId}.json"
            );

            if (File.Exists(path))
            {
                // ���� �ý��ۿ��� ������ ���� �ð� ��������
                return File.GetLastWriteTime(path);
            }

            return DateTime.MinValue;
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ���� ��¥ Ȯ�� ����: {e.Message}");
            return DateTime.MinValue;
        }
    }
}