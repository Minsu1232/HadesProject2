using UnityEngine;
using Steamworks;
using System;
using System.IO;
using GSpawn_Pro;

// ���� API�� �ʱ�ȭ�ϰ� �����ϴ� �̱��� Ŭ����
public class SteamworksManager : Singleton<SteamworksManager>
{
    // ���� API�� �ʱ�ȭ�Ǿ����� ����
    private bool _initialized = false;
    public bool Initialized => _initialized;

    // �ݹ� ó���� ���� ��ü��
    private CallResult<RemoteStorageFileShareResult_t> m_FileShareResult;

    // ���� ���� Callback ó��
    protected Callback<UserStatsReceived_t> m_UserStatsReceived;
    protected Callback<UserStatsStored_t> m_UserStatsStored;
    protected Callback<UserAchievementStored_t> m_AchievementStored;

    // Ŭ���� ���� Callback ó��
    protected Callback<RemoteStorageFileWriteAsyncComplete_t> m_FileWriteAsyncComplete;
    protected Callback<RemoteStorageFileReadAsyncComplete_t> m_FileReadAsyncComplete;

    // �� ID
    private AppId_t m_AppID;

    protected override void Awake()
    {
        base.Awake();

        // �� ��ȯ �ÿ��� ����
        DontDestroyOnLoad(gameObject);

        // ���� API �ʱ�ȭ
        InitializeSteam();
    }

    private void InitializeSteam()
    {
        Debug.Log("���� API �ʱ�ȭ �õ�...");

        try
        {
            // ������ ���� ������ Ȯ��
            if (!Packsize.Test())
            {
                Debug.LogError("���� Ŭ���̾�Ʈ�� ������� �ʾҰų� Steamworks ��Ű�� ũ�Ⱑ �ùٸ��� �ʽ��ϴ�.");
                return;
            }

            // ���� API �ʱ�ȭ
            if (!SteamAPI.Init())
            {
                Debug.LogError("���� API �ʱ�ȭ ����");
                return;
            }

            // �� ID ����
            m_AppID = SteamUtils.GetAppID();
            Debug.Log($"���� AppID: {m_AppID}");

            // �ݹ� ��ü �ʱ�ȭ
            m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
            m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
            m_AchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

            m_FileWriteAsyncComplete = Callback<RemoteStorageFileWriteAsyncComplete_t>.Create(OnFileWriteAsyncComplete);
            m_FileReadAsyncComplete = Callback<RemoteStorageFileReadAsyncComplete_t>.Create(OnFileReadAsyncComplete);

            // ���� ��� �� ���� ������ ��û
     

            // Ŭ���� ��� ���� ���� Ȯ��
            if (!SteamRemoteStorage.IsCloudEnabledForAccount())
            {
                Debug.LogWarning("���� Ŭ���尡 Ȱ��ȭ�Ǿ� ���� �ʽ��ϴ�.");
            }
            else
            {
                Debug.Log("���� Ŭ���尡 Ȱ��ȭ�Ǿ� �ֽ��ϴ�.");
                int fileCount = SteamRemoteStorage.GetFileCount();
                Debug.Log($"Ŭ���忡 {fileCount}���� ������ �ֽ��ϴ�.");
            }

            _initialized = true;
            Debug.Log("���� API �ʱ�ȭ ����");
        }
        catch (Exception e)
        {
            Debug.LogError($"���� API �ʱ�ȭ �� ���� �߻�: {e.Message}");
        }
    }
    private void Start()
    {
        //bool statsResult = SteamUserStats.RequestCurrentStats(); ������ achievementManager���� ������
        //Debug.Log($"���� ��� ��û ���: {statsResult}");
    }
    private void OnDestroy()
    {
        // ���ø����̼� ���� �� ���� API ����
        if (_initialized)
        {
            SteamAPI.Shutdown();
            Debug.Log("���� API ����");
        }
    }

    private void Update()
    {
        // ���� API �ݹ� ó��
        if (_initialized)
        {
            SteamAPI.RunCallbacks();
        }
    }

    #region ���� ó��
    private void OnUserStatsReceived(UserStatsReceived_t pCallback)
    {
        if (m_AppID.m_AppId == pCallback.m_nGameID)
        {
            if (pCallback.m_eResult == EResult.k_EResultOK)
            {
                Debug.Log("���� ��� �� ���� �ε� ����");
            }
            else
            {
                Debug.LogError($"���� ��� �ε� ����: {pCallback.m_eResult}");
            }
        }
    }

    private void OnUserStatsStored(UserStatsStored_t pCallback)
    {
        if (m_AppID.m_AppId == pCallback.m_nGameID)
        {
            if (pCallback.m_eResult == EResult.k_EResultOK)
            {
                Debug.Log("���� ��� ���� ����");
            }
            else
            {
                Debug.LogError($"���� ��� ���� ����: {pCallback.m_eResult}");
            }
        }
    }

    private void OnAchievementStored(UserAchievementStored_t pCallback)
    {
        if (m_AppID.m_AppId == pCallback.m_nGameID)
        {
            Debug.Log($"���� '{pCallback.m_rgchAchievementName}' �����");
        }
    }

    // ���� �޼� ó��
    public bool UnlockAchievement(string achievementId)
    {
        if (!_initialized) return false;

        bool alreadyAchieved = false;
        SteamUserStats.GetAchievement(achievementId, out alreadyAchieved);

        if (alreadyAchieved)
        {
            Debug.Log($"�̹� �޼��� ����: {achievementId}");
            return false;
        }

        bool result = SteamUserStats.SetAchievement(achievementId);
        if (result)
        {
            // ���� ������ ��� ������ ����
            SteamUserStats.StoreStats();
            Debug.Log($"���� �޼�: {achievementId}");
        }
        else
        {
            Debug.LogError($"���� ���� ����: {achievementId}");
        }

        return result;
    }

    // ���� ���� ��Ȳ ������Ʈ (������ ����)
    public bool UpdateStat(string statName, int value)
    {
        if (!_initialized) return false;

        bool result = SteamUserStats.SetStat(statName, value);
        if (result)
        {
            SteamUserStats.StoreStats();
            Debug.Log($"��� ������Ʈ: {statName} = {value}");
        }
        else
        {
            Debug.LogError($"��� ������Ʈ ����: {statName}");
        }

        return result;
    }

    // ���� �ʱ�ȭ (������)
    public bool ResetAchievement(string achievementId)
    {
        if (!_initialized) return false;

        bool result = SteamUserStats.ClearAchievement(achievementId);
        if (result)
        {
            SteamUserStats.StoreStats();
            Debug.Log($"���� �ʱ�ȭ: {achievementId}");
        }
        else
        {
            Debug.LogError($"���� �ʱ�ȭ ����: {achievementId}");
        }

        return result;
    }
    #endregion

    #region Ŭ���� ����� ó��
    // Ŭ���忡 ���� ����
    public bool SaveToCloud(string fileName, byte[] data)
    {
        if (!_initialized || !SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("���� Ŭ���尡 Ȱ��ȭ�Ǿ� ���� �ʰų� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return false;
        }

        bool result = SteamRemoteStorage.FileWrite(fileName, data, data.Length);

        if (result)
        {
            Debug.Log($"Ŭ���忡 ���� ���� ����: {fileName}, ũ��: {data.Length} ����Ʈ");
        }
        else
        {
            Debug.LogError($"Ŭ���忡 ���� ���� ����: {fileName}");
        }

        return result;
    }

    // �񵿱� ���� ���� (��뷮 ���Ͽ�)
    public SteamAPICall_t SaveToCloudAsync(string fileName, byte[] data)
    {
        if (!_initialized || !SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("���� Ŭ���尡 Ȱ��ȭ�Ǿ� ���� �ʰų� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return (SteamAPICall_t)0;
        }

        return SteamRemoteStorage.FileWriteAsync(fileName, data, (uint)data.Length);
    }

    private void OnFileWriteAsyncComplete(RemoteStorageFileWriteAsyncComplete_t pCallback)
    {
        if (pCallback.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log($"�񵿱� ���� ���� ����");
        }
        else
        {
            Debug.LogError($"�񵿱� ���� ���� ����: {pCallback.m_eResult}");
        }
    }

    // Ŭ���忡�� ���� �ε�
    public byte[] LoadFromCloud(string fileName)
    {
        if (!_initialized || !SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("���� Ŭ���尡 Ȱ��ȭ�Ǿ� ���� �ʰų� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return null;
        }

        if (!SteamRemoteStorage.FileExists(fileName))
        {
            Debug.LogWarning($"Ŭ���忡 ������ �������� ����: {fileName}");
            return null;
        }

        int fileSize = SteamRemoteStorage.GetFileSize(fileName);
        if (fileSize <= 0)
        {
            Debug.LogWarning($"Ŭ���� ���� ũ�Ⱑ 0 �Ǵ� ����: {fileName}, ũ��: {fileSize}");
            return null;
        }

        byte[] data = new byte[fileSize];
        int bytesRead = SteamRemoteStorage.FileRead(fileName, data, fileSize);

        if (bytesRead > 0)
        {
            Debug.Log($"Ŭ���忡�� ���� �ε� ����: {fileName}, ũ��: {bytesRead} ����Ʈ");
            return data;
        }
        else
        {
            Debug.LogError($"Ŭ���忡�� ���� �ε� ����: {fileName}");
            return null;
        }
    }

    // �񵿱� ���� �ε� (��뷮 ���Ͽ�)
    public SteamAPICall_t LoadFromCloudAsync(string fileName)
    {
        if (!_initialized || !SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("���� Ŭ���尡 Ȱ��ȭ�Ǿ� ���� �ʰų� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return (SteamAPICall_t)0;
        }

        return SteamRemoteStorage.FileReadAsync(fileName, 0, (uint)SteamRemoteStorage.GetFileSize(fileName));
    }

    private void OnFileReadAsyncComplete(RemoteStorageFileReadAsyncComplete_t pCallback)
    {
        if (pCallback.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log($"�񵿱� ���� �б� ����: {pCallback.m_cubRead} ����Ʈ");
            // ���⼭ ������ ó�� ����
        }
        else
        {
            Debug.LogError($"�񵿱� ���� �б� ����: {pCallback.m_eResult}");
        }
    }

    // Ŭ���� ���� ����
    public bool DeleteCloudFile(string fileName)
    {
        if (!_initialized || !SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("���� Ŭ���尡 Ȱ��ȭ�Ǿ� ���� �ʰų� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return false;
        }

        bool result = SteamRemoteStorage.FileDelete(fileName);

        if (result)
        {
            Debug.Log($"Ŭ���� ���� ���� ����: {fileName}");
        }
        else
        {
            Debug.LogError($"Ŭ���� ���� ���� ����: {fileName}");
        }

        return result;
    }

    // Ŭ���� ���� ��� ��������
    public string[] GetCloudFileList()
    {
        if (!_initialized || !SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("���� Ŭ���尡 Ȱ��ȭ�Ǿ� ���� �ʰų� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return new string[0];
        }

        int fileCount = SteamRemoteStorage.GetFileCount();
        string[] fileList = new string[fileCount];

        for (int i = 0; i < fileCount; i++)
        {
            int fileSizeBytes;
            string fileName = SteamRemoteStorage.GetFileNameAndSize(i, out fileSizeBytes);
            fileList[i] = fileName;
            Debug.Log($"Ŭ���� ���� {i + 1}/{fileCount}: {fileName}, ũ��: {fileSizeBytes} ����Ʈ");
        }

        return fileList;
    }

    // ���� ������ Ŭ����� ����ȭ
    public bool SyncLocalToCloud(string localFilePath, string cloudFileName)
    {
        if (!_initialized || !SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("���� Ŭ���尡 Ȱ��ȭ�Ǿ� ���� �ʰų� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return false;
        }

        try
        {
            if (!File.Exists(localFilePath))
            {
                Debug.LogError($"���� ������ �������� ����: {localFilePath}");
                return false;
            }

            byte[] data = File.ReadAllBytes(localFilePath);
            return SaveToCloud(cloudFileName, data);
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ����ȭ �� ���� �߻�: {e.Message}");
            return false;
        }
    }

    // Ŭ���� ������ ���÷� ����ȭ
    public bool SyncCloudToLocal(string cloudFileName, string localFilePath)
    {
        if (!_initialized || !SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("���� Ŭ���尡 Ȱ��ȭ�Ǿ� ���� �ʰų� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return false;
        }

        try
        {
            byte[] data = LoadFromCloud(cloudFileName);
            if (data == null)
            {
                Debug.LogError($"Ŭ���� ������ �������� ����: {cloudFileName}");
                return false;
            }

            // ���丮�� ������ ����
            string directory = Path.GetDirectoryName(localFilePath);
            if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(localFilePath, data);
            Debug.Log($"Ŭ���� ������ ���÷� ����ȭ ����: {cloudFileName} -> {localFilePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ����ȭ �� ���� �߻�: {e.Message}");
            return false;
        }
    }
    #endregion
}