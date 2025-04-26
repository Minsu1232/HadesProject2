using System;
using System.IO;
using UnityEngine;
using Steamworks;

// 스팀 클라우드를 사용하는 저장 시스템 구현
public class SteamCloudSaveSystem : ISaveSystem 
{
    private const string CloudFilenamePrefix = "cloud_";
    private int currentSlot = 0;

    // 로컬 저장 시스템 (백업용)
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

    // 클라우드 파일명 형식: cloud_slot{슬롯번호}_{데이터ID}.json
    private string GetCloudFileName(string dataId)
    {
        return $"{CloudFilenamePrefix}slot{currentSlot}_{dataId}.json";
    }

    // 데이터 저장 (로컬 + 클라우드)
    public void SaveData<T>(T data, string dataId) where T : class
    {
        try
        {
            // 로컬에 우선 저장 (백업)
            localSaveSystem.SaveData(data, dataId);

            // 스팀 매니저 존재 및 초기화 여부 확인
            if (SteamworksManager.Instance == null || !SteamworksManager.Instance.Initialized)
            {
                Debug.LogWarning("스팀 매니저가 초기화되지 않았습니다. 로컬에만 저장합니다.");
                return;
            }

            // 데이터 직렬화
            string json = JsonUtility.ToJson(data, true);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

            // 클라우드에 저장
            string cloudFileName = GetCloudFileName(dataId);
            bool success = SteamworksManager.Instance.SaveToCloud(cloudFileName, bytes);

            if (success)
            {
                Debug.Log($"데이터가 클라우드에 저장됨: {cloudFileName}, 크기: {bytes.Length} 바이트");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"클라우드 저장 중 오류 발생: {e.Message}");
        }
    }

    // 데이터 로드 (우선 클라우드, 실패 시 로컬)
    public T LoadData<T>(string dataId) where T : class, new()
    {
        try
        {
            // 스팀 매니저 존재 및 초기화 여부 확인
            if (SteamworksManager.Instance == null || !SteamworksManager.Instance.Initialized)
            {
                Debug.LogWarning("스팀 매니저가 초기화되지 않았습니다. 로컬에서만 로드합니다.");
                return localSaveSystem.LoadData<T>(dataId);
            }

            // 클라우드에서 로드 시도
            string cloudFileName = GetCloudFileName(dataId);
            byte[] data = SteamworksManager.Instance.LoadFromCloud(cloudFileName);

            if (data != null && data.Length > 0)
            {
                string json = System.Text.Encoding.UTF8.GetString(data);
                T result = JsonUtility.FromJson<T>(json);

                if (result != null)
                {
                    Debug.Log($"데이터가 클라우드에서 로드됨: {cloudFileName}");

                    // 클라우드 데이터를 로컬에도 저장 (동기화 목적)
                    SyncCloudToLocal(result, dataId);

                    return result;
                }
            }

            Debug.Log($"클라우드에서 데이터를 찾을 수 없음: {cloudFileName}, 로컬에서 시도합니다.");

            // 클라우드에서 실패한 경우 로컬에서 로드
            return localSaveSystem.LoadData<T>(dataId);
        }
        catch (Exception e)
        {
            Debug.LogError($"클라우드 로드 중 오류 발생: {e.Message}, 로컬에서 시도합니다.");
            return localSaveSystem.LoadData<T>(dataId);
        }
    }

    // 클라우드 데이터를 로컬로 동기화
    private void SyncCloudToLocal<T>(T data, string dataId) where T : class
    {
        try
        {
            localSaveSystem.SaveData(data, dataId);
            Debug.Log($"클라우드 데이터가 로컬로 동기화됨: {dataId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"클라우드->로컬 동기화 중 오류 발생: {e.Message}");
        }
    }

    // 데이터 존재 여부 확인 (클라우드 또는 로컬)
    public bool HasData(string dataId)
    {
        // 스팀 매니저 초기화 확인
        if (SteamworksManager.Instance != null && SteamworksManager.Instance.Initialized)
        {
            // 클라우드 파일 존재 확인
            string cloudFileName = GetCloudFileName(dataId);
            bool cloudExists = SteamRemoteStorage.FileExists(cloudFileName);

            if (cloudExists)
            {
                return true;
            }
        }

        // 클라우드에 없으면 로컬 확인
        return localSaveSystem.HasData(dataId);
    }

    // 데이터 삭제 (클라우드 + 로컬)
    public void DeleteData(string dataId)
    {
        // 로컬 데이터 삭제
        localSaveSystem.DeleteData(dataId);

        // 스팀 매니저 초기화 확인
        if (SteamworksManager.Instance != null && SteamworksManager.Instance.Initialized)
        {
            // 클라우드 파일 삭제
            string cloudFileName = GetCloudFileName(dataId);
            bool success = SteamworksManager.Instance.DeleteCloudFile(cloudFileName);

            if (success)
            {
                Debug.Log($"클라우드 데이터 삭제됨: {cloudFileName}");
            }
        }
    }

    // 현재 슬롯 삭제
    public void DeleteSlot()
    {
        Debug.Log($"슬롯 {currentSlot} 삭제 시작 (로컬 및 클라우드)");

        // 로컬 슬롯 삭제
        localSaveSystem.DeleteSlot();

        // 스팀 매니저 초기화 확인
        if (SteamworksManager.Instance != null && SteamworksManager.Instance.Initialized)
        {
            try
            {
                // 클라우드 파일 목록 가져오기
                string[] cloudFiles = SteamworksManager.Instance.GetCloudFileList();
                Debug.Log($"클라우드 파일 목록 가져옴: {cloudFiles.Length}개 파일");

                // 현재 슬롯의 모든 파일 찾아 삭제
                string slotPrefix = $"{CloudFilenamePrefix}slot{currentSlot}_";
                int deletedCount = 0;

                foreach (string fileName in cloudFiles)
                {
                    if (fileName.StartsWith(slotPrefix))
                    {
                        Debug.Log($"클라우드 파일 삭제 시도: {fileName}");
                        bool success = SteamworksManager.Instance.DeleteCloudFile(fileName);

                        if (success)
                        {
                            deletedCount++;
                            Debug.Log($"클라우드 슬롯 파일 삭제 성공: {fileName}");
                        }
                        else
                        {
                            Debug.LogError($"클라우드 슬롯 파일 삭제 실패: {fileName}");
                        }
                    }
                }

                // 메타데이터에서도 슬롯 정보 초기화
                // 메타데이터 파일이 완전히 삭제되지 않을 수 있으므로 내용을 초기화
                UpdateMetadataForDeletedSlot();

                Debug.Log($"클라우드에서 총 {deletedCount}개 파일 삭제됨");
            }
            catch (Exception e)
            {
                Debug.LogError($"클라우드 슬롯 삭제 중 오류 발생: {e.Message}");
            }
        }
    }
    // 새로 추가할 메서드
    private void UpdateMetadataForDeletedSlot()
    {
        try
        {
            // 전체 메타데이터 파일 이름
            string metadataFileName = $"{CloudFilenamePrefix}slotMetadata.json";

            // 클라우드에서 메타데이터 파일 로드
            byte[] data = SteamworksManager.Instance.LoadFromCloud(metadataFileName);
            if (data != null && data.Length > 0)
            {
                string json = System.Text.Encoding.UTF8.GetString(data);

                // 메타데이터에서 현재 슬롯 정보 초기화
                if (json.Contains("slots") && json.Contains("playerName"))
                {
                    // 메타데이터 파일 구조로 역직렬화
                    // 실제 구조에 맞게 수정 필요
                    SaveManager.SlotMetadataWrapper wrapper =
                        JsonUtility.FromJson<SaveManager.SlotMetadataWrapper>(json);

                    if (wrapper != null && wrapper.slots != null &&
                        currentSlot < wrapper.slots.Count && currentSlot >= 0)
                    {
                        // 슬롯 메타데이터 초기화
                        wrapper.slots[currentSlot] = new SaveManager.SlotMetadata();

                        // 다시 클라우드에 저장
                        string updatedJson = JsonUtility.ToJson(wrapper);
                        byte[] updatedData = System.Text.Encoding.UTF8.GetBytes(updatedJson);

                        bool success = SteamworksManager.Instance.SaveToCloud(metadataFileName, updatedData);
                        Debug.Log($"클라우드 메타데이터 업데이트 {(success ? "성공" : "실패")}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"메타데이터 업데이트 중 오류: {e.Message}");
        }
    }
    // 슬롯 존재 여부 확인
    public bool SlotExists()
    {
        // 로컬 슬롯 존재 확인
        bool localExists = localSaveSystem.SlotExists();

        // 스팀 매니저 초기화 확인
        if (SteamworksManager.Instance != null && SteamworksManager.Instance.Initialized)
        {
            // 클라우드 파일 목록 가져오기
            string[] cloudFiles = SteamworksManager.Instance.GetCloudFileList();

            // 현재 슬롯의 파일이 있는지 확인
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

    // 모든 클라우드 저장 파일을 로컬로 동기화
    public void SyncAllCloudToLocal()
    {
        if (SteamworksManager.Instance == null || !SteamworksManager.Instance.Initialized)
        {
            Debug.LogWarning("스팀 매니저가 초기화되지 않았습니다. 동기화할 수 없습니다.");
            return;
        }

        try
        {
            // 클라우드 파일 목록 가져오기
            string[] cloudFiles = SteamworksManager.Instance.GetCloudFileList();

            foreach (string cloudFileName in cloudFiles)
            {
                if (cloudFileName.StartsWith(CloudFilenamePrefix))
                {
                    // 클라우드 파일명에서 정보 추출
                    string fileNameWithoutPrefix = cloudFileName.Substring(CloudFilenamePrefix.Length);
                    int slotEndIndex = fileNameWithoutPrefix.IndexOf('_');

                    if (slotEndIndex > 0)
                    {
                        string slotStr = fileNameWithoutPrefix.Substring(0, slotEndIndex);
                        int slot = -1;

                        if (slotStr.StartsWith("slot") && int.TryParse(slotStr.Substring(4), out slot))
                        {
                            // 데이터 ID 추출
                            string dataId = fileNameWithoutPrefix.Substring(slotEndIndex + 1);
                            if (dataId.EndsWith(".json"))
                            {
                                dataId = dataId.Substring(0, dataId.Length - 5);
                            }

                            // 현재 슬롯 저장
                            int originalSlot = currentSlot;

                            // 해당 슬롯으로 변경
                            SetCurrentSlot(slot);

                            // 로컬 경로 계산 (JsonSaveSystem과 동일한 경로 사용)
                            string localPath = Path.Combine(
                                Application.persistentDataPath,
                                "SaveFiles",
                                $"Slot{slot}",
                                $"{dataId}.json"
                            );

                            // 클라우드에서 로컬로 동기화
                            SteamworksManager.Instance.SyncCloudToLocal(cloudFileName, localPath);

                            // 원래 슬롯으로 복원
                            SetCurrentSlot(originalSlot);
                        }
                    }
                }
            }

            Debug.Log("모든 클라우드 파일을 로컬로 동기화 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"클라우드 동기화 중 오류 발생: {e.Message}");
        }
    }

    // 모든 로컬 저장 파일을 클라우드로 동기화
    public void SyncAllLocalToCloud()
    {
        if (SteamworksManager.Instance == null || !SteamworksManager.Instance.Initialized)
        {
            Debug.LogWarning("스팀 매니저가 초기화되지 않았습니다. 동기화할 수 없습니다.");
            return;
        }

        try
        {
            // 기존 슬롯 저장
            int originalSlot = currentSlot;

            // 모든 슬롯 탐색
            for (int slot = 0; slot < 3; slot++) // 최대 슬롯 수는 SaveManager와 동일하게 설정
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
                        // 슬롯 폴더의 모든 JSON 파일 가져오기
                        string[] jsonFiles = Directory.GetFiles(slotPath, "*.json");

                        foreach (string localPath in jsonFiles)
                        {
                            // 파일명에서 데이터 ID 추출
                            string fileName = Path.GetFileNameWithoutExtension(localPath);
                            string cloudFileName = GetCloudFileName(fileName);

                            // 로컬에서 클라우드로 동기화
                            SteamworksManager.Instance.SyncLocalToCloud(localPath, cloudFileName);
                        }
                    }
                }
            }

            // 원래 슬롯으로 복원
            SetCurrentSlot(originalSlot);

            Debug.Log("모든 로컬 파일을 클라우드로 동기화 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"로컬->클라우드 동기화 중 오류 발생: {e.Message}");
        }
    }

    // 게임 시작 시 로컬과 클라우드 데이터 비교 및 최신 데이터 사용
    public void CompareAndSelectBestData()
    {
        if (SteamworksManager.Instance == null || !SteamworksManager.Instance.Initialized)
        {
            Debug.LogWarning("스팀 매니저가 초기화되지 않았습니다. 데이터 비교를 수행할 수 없습니다.");
            return;
        }

        Debug.Log("로컬과 클라우드 데이터 비교 시작...");

        try
        {
            // 먼저 슬롯 메타데이터 동기화
            SyncSlotMetadata();

            // 각 슬롯마다 비교
            for (int slot = 0; slot < 3; slot++)
            {
                SetCurrentSlot(slot);

                // 슬롯 메타데이터 파일 확인
                string metadataId = "slotMetadata";
                string cloudMetadataFileName = GetCloudFileName(metadataId);

                // 클라우드에 메타데이터 파일이 있는지 확인
                bool cloudHasMetadata = SteamRemoteStorage.FileExists(cloudMetadataFileName);

                // 로컬에 메타데이터 파일이 있는지 확인
                bool localHasMetadata = localSaveSystem.HasData(metadataId);

                // 메타데이터 비교 및 동기화
                if (cloudHasMetadata && localHasMetadata)
                {
                    // 양쪽 다 있으면 날짜 비교 후 최신 것 사용
                    DateTime cloudLastModified = GetCloudFileLastModified(cloudMetadataFileName);
                    DateTime localLastModified = GetLocalFileLastModified(metadataId);

                    if (cloudLastModified > localLastModified)
                    {
                        // 클라우드가 더 최신이면 클라우드 데이터 사용
                        Debug.Log($"슬롯 {slot}: 클라우드 데이터가 더 최신입니다. 클라우드 데이터를 로컬로 동기화합니다.");
                        SyncCloudToLocalForSlot(slot);
                    }
                    else
                    {
                        // 로컬이 더 최신이면 로컬 데이터 사용
                        Debug.Log($"슬롯 {slot}: 로컬 데이터가 더 최신입니다. 로컬 데이터를 클라우드로 동기화합니다.");
                        SyncLocalToCloudForSlot(slot);
                    }
                }
                else if (cloudHasMetadata)
                {
                    // 클라우드에만 있으면 클라우드 데이터 사용
                    Debug.Log($"슬롯 {slot}: 로컬에 데이터가 없습니다. 클라우드 데이터를 로컬로 동기화합니다.");
                    SyncCloudToLocalForSlot(slot);
                }
                else if (localHasMetadata)
                {
                    // 로컬에만 있으면 로컬 데이터 사용
                    Debug.Log($"슬롯 {slot}: 클라우드에 데이터가 없습니다. 로컬 데이터를 클라우드로 동기화합니다.");
                    SyncLocalToCloudForSlot(slot);
                }
                // 둘 다 없으면 아무것도 안 함
            }

            Debug.Log("데이터 비교 및 동기화 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"데이터 비교 중 오류 발생: {e.Message}");
        }
    }
    // 슬롯 메타데이터 동기화 메서드 추가
    private void SyncSlotMetadata()
    {
        // 슬롯 메타데이터 파일 이름 (SaveManager에 정의된 값 사용)
        string metadataId = "slotMetadata";
        string cloudMetadataFileName = GetCloudFileName(metadataId);

        // 클라우드에 메타데이터 파일이 있는지 확인
        bool cloudHasMetadata = SteamRemoteStorage.FileExists(cloudMetadataFileName);

        // 로컬 경로
        string localPath = Path.Combine(
            Application.persistentDataPath,
            "SaveFiles",
            $"{metadataId}.json"
        );

        // 로컬에 메타데이터 파일이 있는지 확인
        bool localHasMetadata = File.Exists(localPath);

        // 메타데이터 비교 및 동기화
        if (cloudHasMetadata && localHasMetadata)
        {
            // 양쪽 다 있으면 날짜 비교
            DateTime cloudLastModified = GetCloudFileLastModified(cloudMetadataFileName);
            DateTime localLastModified = File.GetLastWriteTime(localPath);

            if (cloudLastModified > localLastModified)
            {
                // 클라우드가 더 최신이면 클라우드 데이터 사용
                Debug.Log("슬롯 메타데이터: 클라우드 데이터가 더 최신입니다. 클라우드 데이터를 로컬로 동기화합니다.");
                SteamworksManager.Instance.SyncCloudToLocal(cloudMetadataFileName, localPath);
            }
            else
            {
                // 로컬이 더 최신이면 로컬 데이터 사용
                Debug.Log("슬롯 메타데이터: 로컬 데이터가 더 최신입니다. 로컬 데이터를 클라우드로 동기화합니다.");
                SteamworksManager.Instance.SyncLocalToCloud(localPath, cloudMetadataFileName);
            }
        }
        else if (cloudHasMetadata)
        {
            // 클라우드에만 있으면 클라우드 데이터 사용
            Debug.Log("슬롯 메타데이터: 로컬에 데이터가 없습니다. 클라우드 데이터를 로컬로 동기화합니다.");
            SteamworksManager.Instance.SyncCloudToLocal(cloudMetadataFileName, localPath);
        }
        else if (localHasMetadata)
        {
            // 로컬에만 있으면 로컬 데이터 사용
            Debug.Log("슬롯 메타데이터: 클라우드에 데이터가 없습니다. 로컬 데이터를 클라우드로 동기화합니다.");
            SteamworksManager.Instance.SyncLocalToCloud(localPath, cloudMetadataFileName);
        }
        // 둘 다 없으면 아무것도 안 함
    }

    // 특정 슬롯의 클라우드 데이터를 로컬로 동기화
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
                // 데이터 ID 추출
                string fileNameWithoutPrefix = cloudFileName.Substring(slotPrefix.Length);
                string dataId = fileNameWithoutPrefix;

                if (dataId.EndsWith(".json"))
                {
                    dataId = dataId.Substring(0, dataId.Length - 5);
                }

                // 로컬 경로 계산
                string localPath = Path.Combine(
                    Application.persistentDataPath,
                    "SaveFiles",
                    $"Slot{slot}",
                    $"{dataId}.json"
                );

                // 클라우드에서 로컬로 동기화
                SteamworksManager.Instance.SyncCloudToLocal(cloudFileName, localPath);
            }
        }

        SetCurrentSlot(originalSlot);
    }

    // 특정 슬롯의 로컬 데이터를 클라우드로 동기화
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
            // 슬롯 폴더의 모든 JSON 파일 가져오기
            string[] jsonFiles = Directory.GetFiles(slotPath, "*.json");

            foreach (string localPath in jsonFiles)
            {
                // 파일명에서 데이터 ID 추출
                string fileName = Path.GetFileNameWithoutExtension(localPath);
                string cloudFileName = GetCloudFileName(fileName);

                // 로컬에서 클라우드로 동기화
                SteamworksManager.Instance.SyncLocalToCloud(localPath, cloudFileName);
            }
        }

        SetCurrentSlot(originalSlot);
    }

    // 클라우드 파일의 마지막 수정 시간 가져오기
    private DateTime GetCloudFileLastModified(string fileName)
    {
        // 스팀 API에서는 파일 수정 시간을 직접 가져오는 기능이 제한적이므로
        // 파일 내용을 로드하여 메타데이터에서 시간 추출
        try
        {
            byte[] data = SteamworksManager.Instance.LoadFromCloud(fileName);
            if (data != null && data.Length > 0)
            {
                string json = System.Text.Encoding.UTF8.GetString(data);

                // SlotMetadataWrapper에서 마지막 저장 시간 추출
                // 실제 구현은 SaveManager의 SlotMetadata 구조에 따라 다를 수 있음
                if (json.Contains("lastSaveTimeStr"))
                {
                    // 간단한 정규식 또는 문자열 파싱으로 날짜 추출
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

            // 파싱 실패 시 기본값 반환
            return DateTime.MinValue;
        }
        catch (Exception e)
        {
            Debug.LogError($"클라우드 파일 날짜 파싱 오류: {e.Message}");
            return DateTime.MinValue;
        }
    }

    // 로컬 파일의 마지막 수정 시간 가져오기
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
                // 파일 시스템에서 마지막 수정 시간 가져오기
                return File.GetLastWriteTime(path);
            }

            return DateTime.MinValue;
        }
        catch (Exception e)
        {
            Debug.LogError($"로컬 파일 날짜 확인 오류: {e.Message}");
            return DateTime.MinValue;
        }
    }
}