using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static AudioManager Instance { get; private set; }

    [Header("배경음악 설정")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float defaultMusicVolume = 0.7f;

    [Header("기본 배경음악 클립")]
    [SerializeField] private AudioClip lobbyMusic;
    [SerializeField] private AudioClip townMusic;

    [Header("던전 배경음악 클립")]
    [SerializeField] private AudioClip chapter1Music; // 야스오 던전
    [SerializeField] private AudioClip chapter2Music; // 용족 던전
    [SerializeField] private AudioClip chapter3Music; // 데스 던전
    [SerializeField] private AudioClip chapter4Music; // 하트 던전

    // 볼륨 설정
    private float musicVolume;

    // 현재 재생 중인 씬 이름
    private string currentScene = "";

    // 음악 클립 매핑 딕셔너리
    private Dictionary<string, AudioClip> musicMappings = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 초기화
        Initialize();
    }

    private void Initialize()
    {
        // 오디오 소스가 할당되지 않은 경우 자동 생성
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("Music_Source");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        // 음악 클립 매핑 초기화
        SetupMusicMappings();

        // 볼륨 초기화
        LoadVolumeSettings();

        // 씬 변경 이벤트 구독
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void SetupMusicMappings()
    {
        // 기본 씬 음악 매핑
        if (lobbyMusic != null) musicMappings["Lobby"] = lobbyMusic;
        if (townMusic != null) musicMappings["Village"] = townMusic;

        // 챕터 던전 음악 매핑
        if (chapter1Music != null)
        {
            musicMappings["Chapter1"] = chapter1Music;
            musicMappings["YasuoChapter"] = chapter1Music;
        }

        if (chapter2Music != null)
        {
            musicMappings["Chapter2"] = chapter2Music;
            musicMappings["YongzokChapter"] = chapter2Music;
        }

        if (chapter3Music != null)
        {
            musicMappings["Chapter3"] = chapter3Music;
            musicMappings["DeathChapter"] = chapter3Music;
        }

        if (chapter4Music != null)
        {
            musicMappings["Chapter4"] = chapter4Music;
            musicMappings["HeartChapter"] = chapter4Music;
        }
    }

    private void LoadVolumeSettings()
    {
        // SaveManager에서 볼륨 설정 로드
        if (SaveManager.Instance != null)
        {
            var settings = SaveManager.Instance.GetSettingsData();

            if (settings != null)
            {
                musicVolume = settings.musicVolume;
            }
            else
            {
                musicVolume = defaultMusicVolume;
            }
        }
        else
        {
            musicVolume = defaultMusicVolume;
        }

        // 볼륨 적용
        ApplyMusicVolume();
    }

    private void ApplyMusicVolume()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * 0.3f; // 전체 볼륨의 30%로 제한 (필요시 조정)
        }
    }

    // 씬이 로드될 때 호출되는 메서드
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 이미 현재 씬에 맞는 음악이 재생 중이면 무시
        if (currentScene == scene.name)
            return;

        currentScene = scene.name;

        // 씬에 맞는 배경음악 재생
        PlayMusicForScene(scene.name);

        Debug.Log($"변경되는 씬 이름: {scene.name}");
    }

    // 씬 이름에 따라 적절한 배경음악 재생
    private void PlayMusicForScene(string sceneName)
    {
        AudioClip clipToPlay = null;

        // 딕셔너리에서 직접 매핑 확인
        if (musicMappings.TryGetValue(sceneName, out clipToPlay))
        {
            // 딕셔너리에서 정확한 씬 이름 매칭을 찾았음
        }
        else
        {
            // 정확한 매칭이 없으면 부분 문자열 비교
            if (sceneName.Contains("Lobby"))
            {
                clipToPlay = lobbyMusic;
            }
            else if (sceneName.Contains("Village"))
            {
                clipToPlay = townMusic;
            }
            else if (sceneName.Contains("YasuoChapter") || sceneName.Contains("Chapter1"))
            {
                clipToPlay = chapter1Music;
            }
            else if (sceneName.Contains("YongzokChapter") || sceneName.Contains("Chapter2"))
            {
                clipToPlay = chapter2Music;
            }
            else if (sceneName.Contains("DeathChapter") || sceneName.Contains("Chapter3"))
            {
                clipToPlay = chapter3Music;
            }
            else if (sceneName.Contains("HeartChapter") || sceneName.Contains("Chapter4"))
            {
                clipToPlay = chapter4Music;
            }
        }

        // 음악 재생
        if (clipToPlay != null && musicSource != null)
        {
            // 현재 재생 중인 클립과 다르면 교체
            if (musicSource.clip != clipToPlay)
            {
                Debug.Log($"배경음악 변경: {sceneName} -> {clipToPlay.name}");
                musicSource.clip = clipToPlay;
                musicSource.Play();

                // 볼륨 적용
                ApplyMusicVolume();
            }
        }
        else
        {
            Debug.LogWarning($"씬 '{sceneName}'에 대한 배경음악을 찾을 수 없습니다.");
        }
    }

    // 음악 볼륨 설정 메서드
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyMusicVolume();

        // SaveManager에 변경사항 저장
        if (SaveManager.Instance != null)
        {
            var settings = SaveManager.Instance.GetSettingsData();
            if (settings != null)
            {
                settings.musicVolume = musicVolume;
                SaveManager.Instance.SaveSettingsData();
            }
        }
    }

    // 특정 음악 수동 재생 메서드 - 챕터 기준
    public void PlayChapterMusic(int chapterNumber)
    {
        string chapterKey = $"Chapter{chapterNumber}";

        if (musicMappings.TryGetValue(chapterKey, out AudioClip clipToPlay) && clipToPlay != null)
        {
            musicSource.clip = clipToPlay;
            musicSource.Play();
            ApplyMusicVolume();
        }
        else
        {
            Debug.LogWarning($"챕터 {chapterNumber}에 대한 배경음악을 찾을 수 없습니다.");
        }
    }

    // 특정 음악 수동 재생 메서드 - 문자열 기준
    public void PlayMusic(string musicType)
    {
        AudioClip clipToPlay = null;

        // 음악 타입에 따라 클립 선택
        switch (musicType.ToLower())
        {
            case "lobby":
                clipToPlay = lobbyMusic;
                break;
            case "town":
            case "village":
                clipToPlay = townMusic;
                break;
            case "chapter1":
            case "yasuochapter":
                clipToPlay = chapter1Music;
                break;
            case "chapter2":
            case "yongzokchapter":
                clipToPlay = chapter2Music;
                break;
            case "chapter3":
            case "deathchapter":
                clipToPlay = chapter3Music;
                break;
            case "chapter4":
            case "heartchapter":
                clipToPlay = chapter4Music;
                break;
        }

        // 음악 재생
        if (clipToPlay != null && musicSource != null)
        {
            musicSource.clip = clipToPlay;
            musicSource.Play();
            ApplyMusicVolume();
        }
        else
        {
            Debug.LogWarning($"음악 타입 '{musicType}'에 대한 오디오 클립을 찾을 수 없습니다.");
        }
    }

    // 음악 일시정지
    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    // 음악 재개
    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
    }

    // 음악 정지
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    // 현재 재생 중인 배경음악 클립 이름 반환
    public string GetCurrentMusicName()
    {
        if (musicSource != null && musicSource.clip != null)
        {
            return musicSource.clip.name;
        }
        return "None";
    }

    // 앱 종료 시 정리
    private void OnDestroy()
    {
        // 씬 변경 이벤트 구독 해제
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}