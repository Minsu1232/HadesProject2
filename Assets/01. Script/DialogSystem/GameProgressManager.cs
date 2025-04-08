using System.Collections.Generic;
using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    // 캐시 데이터
    private HashSet<string> _shownDialogsCache = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

        
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        if (SaveManager.Instance != null)
        {
            LoadProgress();
        }
    }
    // 다이얼로그 관련 메서드
    public bool IsDialogShown(string dialogID)
    {
        return _shownDialogsCache.Contains(dialogID);
    }
   
    public int GetDeathCount()
    {
        PlayerSaveData playerData = SaveManager.Instance?.GetPlayerData();
        return playerData?.deathCount ?? 0;
    }

    public void IncrementDeathCount()
    {
        PlayerSaveData playerData = SaveManager.Instance?.GetPlayerData();
        if (playerData != null)
        {
            playerData.deathCount++;
            SaveManager.Instance.SavePlayerData();
            Debug.Log($"사망 횟수 증가: {playerData.deathCount}회");
        }
    }
    public void MarkDialogAsShown(string dialogID)
    {
        if (!_shownDialogsCache.Contains(dialogID))
        {
            _shownDialogsCache.Add(dialogID);

            PlayerSaveData playerData = SaveManager.Instance?.GetPlayerData();
            if (playerData != null)
            {
                if (playerData.shownDialogs == null)
                    playerData.shownDialogs = new List<string>();

                if (!playerData.shownDialogs.Contains(dialogID))
                    playerData.shownDialogs.Add(dialogID);

                SaveManager.Instance.SavePlayerData();
            }
        }
    }

    // 플래그 관련 메서드
    public void SetFlag(string flagName, bool value)
    {
        PlayerSaveData playerData = SaveManager.Instance?.GetPlayerData();
        if (playerData != null)
        {
            if (playerData.gameFlags == null)
                playerData.gameFlags = new Dictionary<string, bool>();

            playerData.gameFlags[flagName] = value;
            SaveManager.Instance.SavePlayerData();
        }
    }

    public bool GetFlag(string flagName)
    {
        PlayerSaveData playerData = SaveManager.Instance?.GetPlayerData();
        if (playerData != null && playerData.gameFlags != null)
        {
            return playerData.gameFlags.TryGetValue(flagName, out bool value) && value;
        }
        return false;
    }

    // 방문 횟수 관련
    public void SetLocationVisitCount(string locationID, int count)
    {
        PlayerSaveData playerData = SaveManager.Instance?.GetPlayerData();
        if (playerData != null)
        {
            if (playerData.locationVisits == null)
                playerData.locationVisits = new Dictionary<string, int>();

            playerData.locationVisits[locationID] = count;
            SaveManager.Instance.SavePlayerData();
        }
    }

    public int GetLocationVisitCount(string locationID)
    {
        PlayerSaveData playerData = SaveManager.Instance?.GetPlayerData();
        if (playerData != null && playerData.locationVisits != null)
        {
            return playerData.locationVisits.TryGetValue(locationID, out int count) ? count : 0;
        }
        return 0;
    }

    // 무기 관련
    public void UnlockWeapon(string weaponID)
    {
        PlayerSaveData playerData = SaveManager.Instance?.GetPlayerData();
        if (playerData != null)
        {
            if (playerData.unlockedWeapons == null)
                playerData.unlockedWeapons = new List<string>();

            if (!playerData.unlockedWeapons.Contains(weaponID))
            {
                playerData.unlockedWeapons.Add(weaponID);
                SaveManager.Instance.SavePlayerData();
            }
        }
    }

    // 파편 관련
    public void AcquireFragment(string fragmentID)
    {
        PlayerSaveData playerData = SaveManager.Instance?.GetPlayerData();
        if (playerData != null)
        {
            if (playerData.acquiredFragments == null)
                playerData.acquiredFragments = new List<string>();

            if (!playerData.acquiredFragments.Contains(fragmentID))
            {
                playerData.acquiredFragments.Add(fragmentID);
                SaveManager.Instance.SavePlayerData();
            }
        }
    }

    // 챕터 관련
    public void SetCurrentChapter(int chapter)
    {
        PlayerSaveData playerData = SaveManager.Instance?.GetPlayerData();
        if (playerData != null)
        {
            playerData.currentChapter = chapter;
            SaveManager.Instance.SavePlayerData();
        }
    }

    // 진행 상태 로드
    public void LoadProgress()
    {
        _shownDialogsCache.Clear();

        PlayerSaveData playerData = SaveManager.Instance?.GetPlayerData();
        if (playerData != null)
        {
            if (playerData.shownDialogs != null)
            {
                foreach (string dialog in playerData.shownDialogs)
                {
                    _shownDialogsCache.Add(dialog);
                    Debug.Log($"캐시에 추가된 다이얼로그: {dialog}");
                }
            }
        }
    }

    // 진행 초기화
    public void ResetProgress()
    {
        _shownDialogsCache.Clear();

        PlayerSaveData playerData = SaveManager.Instance?.GetPlayerData();
        if (playerData != null)
        {
            if (playerData.shownDialogs == null)
                playerData.shownDialogs = new List<string>();
            else
                playerData.shownDialogs.Clear();

            if (playerData.gameFlags == null)
                playerData.gameFlags = new Dictionary<string, bool>();
            else
                playerData.gameFlags.Clear();

            if (playerData.locationVisits == null)
                playerData.locationVisits = new Dictionary<string, int>();
            else
                playerData.locationVisits.Clear();

            if (playerData.unlockedWeapons == null)
                playerData.unlockedWeapons = new List<string>();
            else
                playerData.unlockedWeapons.Clear();

            if (playerData.acquiredFragments == null)
                playerData.acquiredFragments = new List<string>();
            else
                playerData.acquiredFragments.Clear();
        }
    }
}