using UnityEngine;
using Steamworks;
using System;
using System.IO;
using GSpawn_Pro;

// 스팀 API를 초기화하고 관리하는 싱글톤 클래스
public class SteamworksManager : Singleton<SteamworksManager>
{
    // 스팀 API가 초기화되었는지 여부
    private bool _initialized = false;
    public bool Initialized => _initialized;

    // 콜백 처리를 위한 객체들
    private CallResult<RemoteStorageFileShareResult_t> m_FileShareResult;

    // 업적 관련 Callback 처리
    protected Callback<UserStatsReceived_t> m_UserStatsReceived;
    protected Callback<UserStatsStored_t> m_UserStatsStored;
    protected Callback<UserAchievementStored_t> m_AchievementStored;

    // 클라우드 관련 Callback 처리
    protected Callback<RemoteStorageFileWriteAsyncComplete_t> m_FileWriteAsyncComplete;
    protected Callback<RemoteStorageFileReadAsyncComplete_t> m_FileReadAsyncComplete;

    // 앱 ID
    private AppId_t m_AppID;

    protected override void Awake()
    {
        base.Awake();

        // 씬 전환 시에도 유지
        DontDestroyOnLoad(gameObject);

        // 스팀 API 초기화
        InitializeSteam();
    }

    private void InitializeSteam()
    {
        Debug.Log("스팀 API 초기화 시도...");

        try
        {
            // 스팀이 실행 중인지 확인
            if (!Packsize.Test())
            {
                Debug.LogError("스팀 클라이언트가 실행되지 않았거나 Steamworks 패키지 크기가 올바르지 않습니다.");
                return;
            }

            // 스팀 API 초기화
            if (!SteamAPI.Init())
            {
                Debug.LogError("스팀 API 초기화 실패");
                return;
            }

            // 앱 ID 저장
            m_AppID = SteamUtils.GetAppID();
            Debug.Log($"스팀 AppID: {m_AppID}");

            // 콜백 객체 초기화
            m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
            m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
            m_AchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

            m_FileWriteAsyncComplete = Callback<RemoteStorageFileWriteAsyncComplete_t>.Create(OnFileWriteAsyncComplete);
            m_FileReadAsyncComplete = Callback<RemoteStorageFileReadAsyncComplete_t>.Create(OnFileReadAsyncComplete);

            // 스팀 통계 및 업적 데이터 요청
     

            // 클라우드 사용 가능 여부 확인
            if (!SteamRemoteStorage.IsCloudEnabledForAccount())
            {
                Debug.LogWarning("스팀 클라우드가 활성화되어 있지 않습니다.");
            }
            else
            {
                Debug.Log("스팀 클라우드가 활성화되어 있습니다.");
                int fileCount = SteamRemoteStorage.GetFileCount();
                Debug.Log($"클라우드에 {fileCount}개의 파일이 있습니다.");
            }

            _initialized = true;
            Debug.Log("스팀 API 초기화 성공");
        }
        catch (Exception e)
        {
            Debug.LogError($"스팀 API 초기화 중 오류 발생: {e.Message}");
        }
    }
    private void Start()
    {
        //bool statsResult = SteamUserStats.RequestCurrentStats(); 업적은 achievementManager에서 관리됨
        //Debug.Log($"스팀 통계 요청 결과: {statsResult}");
    }
    private void OnDestroy()
    {
        // 애플리케이션 종료 시 스팀 API 종료
        if (_initialized)
        {
            SteamAPI.Shutdown();
            Debug.Log("스팀 API 종료");
        }
    }

    private void Update()
    {
        // 스팀 API 콜백 처리
        if (_initialized)
        {
            SteamAPI.RunCallbacks();
        }
    }

    #region 업적 처리
    private void OnUserStatsReceived(UserStatsReceived_t pCallback)
    {
        if (m_AppID.m_AppId == pCallback.m_nGameID)
        {
            if (pCallback.m_eResult == EResult.k_EResultOK)
            {
                Debug.Log("스팀 통계 및 업적 로드 성공");
            }
            else
            {
                Debug.LogError($"스팀 통계 로드 실패: {pCallback.m_eResult}");
            }
        }
    }

    private void OnUserStatsStored(UserStatsStored_t pCallback)
    {
        if (m_AppID.m_AppId == pCallback.m_nGameID)
        {
            if (pCallback.m_eResult == EResult.k_EResultOK)
            {
                Debug.Log("스팀 통계 저장 성공");
            }
            else
            {
                Debug.LogError($"스팀 통계 저장 실패: {pCallback.m_eResult}");
            }
        }
    }

    private void OnAchievementStored(UserAchievementStored_t pCallback)
    {
        if (m_AppID.m_AppId == pCallback.m_nGameID)
        {
            Debug.Log($"업적 '{pCallback.m_rgchAchievementName}' 저장됨");
        }
    }

    // 업적 달성 처리
    public bool UnlockAchievement(string achievementId)
    {
        if (!_initialized) return false;

        bool alreadyAchieved = false;
        SteamUserStats.GetAchievement(achievementId, out alreadyAchieved);

        if (alreadyAchieved)
        {
            Debug.Log($"이미 달성한 업적: {achievementId}");
            return false;
        }

        bool result = SteamUserStats.SetAchievement(achievementId);
        if (result)
        {
            // 업적 정보를 즉시 서버에 전송
            SteamUserStats.StoreStats();
            Debug.Log($"업적 달성: {achievementId}");
        }
        else
        {
            Debug.LogError($"업적 설정 실패: {achievementId}");
        }

        return result;
    }

    // 업적 진행 상황 업데이트 (점진적 업적)
    public bool UpdateStat(string statName, int value)
    {
        if (!_initialized) return false;

        bool result = SteamUserStats.SetStat(statName, value);
        if (result)
        {
            SteamUserStats.StoreStats();
            Debug.Log($"통계 업데이트: {statName} = {value}");
        }
        else
        {
            Debug.LogError($"통계 업데이트 실패: {statName}");
        }

        return result;
    }

    // 업적 초기화 (디버깅용)
    public bool ResetAchievement(string achievementId)
    {
        if (!_initialized) return false;

        bool result = SteamUserStats.ClearAchievement(achievementId);
        if (result)
        {
            SteamUserStats.StoreStats();
            Debug.Log($"업적 초기화: {achievementId}");
        }
        else
        {
            Debug.LogError($"업적 초기화 실패: {achievementId}");
        }

        return result;
    }
    #endregion

    #region 클라우드 저장소 처리
    // 클라우드에 파일 저장
    public bool SaveToCloud(string fileName, byte[] data)
    {
        if (!_initialized || !SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("스팀 클라우드가 활성화되어 있지 않거나 초기화되지 않았습니다.");
            return false;
        }

        bool result = SteamRemoteStorage.FileWrite(fileName, data, data.Length);

        if (result)
        {
            Debug.Log($"클라우드에 파일 저장 성공: {fileName}, 크기: {data.Length} 바이트");
        }
        else
        {
            Debug.LogError($"클라우드에 파일 저장 실패: {fileName}");
        }

        return result;
    }

    // 비동기 파일 저장 (대용량 파일용)
    public SteamAPICall_t SaveToCloudAsync(string fileName, byte[] data)
    {
        if (!_initialized || !SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("스팀 클라우드가 활성화되어 있지 않거나 초기화되지 않았습니다.");
            return (SteamAPICall_t)0;
        }

        return SteamRemoteStorage.FileWriteAsync(fileName, data, (uint)data.Length);
    }

    private void OnFileWriteAsyncComplete(RemoteStorageFileWriteAsyncComplete_t pCallback)
    {
        if (pCallback.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log($"비동기 파일 저장 성공");
        }
        else
        {
            Debug.LogError($"비동기 파일 저장 실패: {pCallback.m_eResult}");
        }
    }

    // 클라우드에서 파일 로드
    public byte[] LoadFromCloud(string fileName)
    {
        if (!_initialized || !SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("스팀 클라우드가 활성화되어 있지 않거나 초기화되지 않았습니다.");
            return null;
        }

        if (!SteamRemoteStorage.FileExists(fileName))
        {
            Debug.LogWarning($"클라우드에 파일이 존재하지 않음: {fileName}");
            return null;
        }

        int fileSize = SteamRemoteStorage.GetFileSize(fileName);
        if (fileSize <= 0)
        {
            Debug.LogWarning($"클라우드 파일 크기가 0 또는 음수: {fileName}, 크기: {fileSize}");
            return null;
        }

        byte[] data = new byte[fileSize];
        int bytesRead = SteamRemoteStorage.FileRead(fileName, data, fileSize);

        if (bytesRead > 0)
        {
            Debug.Log($"클라우드에서 파일 로드 성공: {fileName}, 크기: {bytesRead} 바이트");
            return data;
        }
        else
        {
            Debug.LogError($"클라우드에서 파일 로드 실패: {fileName}");
            return null;
        }
    }

    // 비동기 파일 로드 (대용량 파일용)
    public SteamAPICall_t LoadFromCloudAsync(string fileName)
    {
        if (!_initialized || !SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("스팀 클라우드가 활성화되어 있지 않거나 초기화되지 않았습니다.");
            return (SteamAPICall_t)0;
        }

        return SteamRemoteStorage.FileReadAsync(fileName, 0, (uint)SteamRemoteStorage.GetFileSize(fileName));
    }

    private void OnFileReadAsyncComplete(RemoteStorageFileReadAsyncComplete_t pCallback)
    {
        if (pCallback.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log($"비동기 파일 읽기 성공: {pCallback.m_cubRead} 바이트");
            // 여기서 데이터 처리 가능
        }
        else
        {
            Debug.LogError($"비동기 파일 읽기 실패: {pCallback.m_eResult}");
        }
    }

    // 클라우드 파일 삭제
    public bool DeleteCloudFile(string fileName)
    {
        if (!_initialized || !SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("스팀 클라우드가 활성화되어 있지 않거나 초기화되지 않았습니다.");
            return false;
        }

        bool result = SteamRemoteStorage.FileDelete(fileName);

        if (result)
        {
            Debug.Log($"클라우드 파일 삭제 성공: {fileName}");
        }
        else
        {
            Debug.LogError($"클라우드 파일 삭제 실패: {fileName}");
        }

        return result;
    }

    // 클라우드 파일 목록 가져오기
    public string[] GetCloudFileList()
    {
        if (!_initialized || !SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("스팀 클라우드가 활성화되어 있지 않거나 초기화되지 않았습니다.");
            return new string[0];
        }

        int fileCount = SteamRemoteStorage.GetFileCount();
        string[] fileList = new string[fileCount];

        for (int i = 0; i < fileCount; i++)
        {
            int fileSizeBytes;
            string fileName = SteamRemoteStorage.GetFileNameAndSize(i, out fileSizeBytes);
            fileList[i] = fileName;
            Debug.Log($"클라우드 파일 {i + 1}/{fileCount}: {fileName}, 크기: {fileSizeBytes} 바이트");
        }

        return fileList;
    }

    // 로컬 파일을 클라우드와 동기화
    public bool SyncLocalToCloud(string localFilePath, string cloudFileName)
    {
        if (!_initialized || !SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("스팀 클라우드가 활성화되어 있지 않거나 초기화되지 않았습니다.");
            return false;
        }

        try
        {
            if (!File.Exists(localFilePath))
            {
                Debug.LogError($"로컬 파일이 존재하지 않음: {localFilePath}");
                return false;
            }

            byte[] data = File.ReadAllBytes(localFilePath);
            return SaveToCloud(cloudFileName, data);
        }
        catch (Exception e)
        {
            Debug.LogError($"파일 동기화 중 오류 발생: {e.Message}");
            return false;
        }
    }

    // 클라우드 파일을 로컬로 동기화
    public bool SyncCloudToLocal(string cloudFileName, string localFilePath)
    {
        if (!_initialized || !SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("스팀 클라우드가 활성화되어 있지 않거나 초기화되지 않았습니다.");
            return false;
        }

        try
        {
            byte[] data = LoadFromCloud(cloudFileName);
            if (data == null)
            {
                Debug.LogError($"클라우드 파일이 존재하지 않음: {cloudFileName}");
                return false;
            }

            // 디렉토리가 없으면 생성
            string directory = Path.GetDirectoryName(localFilePath);
            if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(localFilePath, data);
            Debug.Log($"클라우드 파일을 로컬로 동기화 성공: {cloudFileName} -> {localFilePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"파일 동기화 중 오류 발생: {e.Message}");
            return false;
        }
    }
    #endregion
}