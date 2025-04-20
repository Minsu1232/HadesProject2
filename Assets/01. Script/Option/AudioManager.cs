using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static AudioManager Instance { get; private set; }

    [Header("������� ����")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float defaultMusicVolume = 0.7f;

    [Header("�⺻ ������� Ŭ��")]
    [SerializeField] private AudioClip lobbyMusic;
    [SerializeField] private AudioClip townMusic;

    [Header("���� ������� Ŭ��")]
    [SerializeField] private AudioClip chapter1Music; // �߽��� ����
    [SerializeField] private AudioClip chapter2Music; // ���� ����
    [SerializeField] private AudioClip chapter3Music; // ���� ����
    [SerializeField] private AudioClip chapter4Music; // ��Ʈ ����

    // ���� ����
    private float musicVolume;

    // ���� ��� ���� �� �̸�
    private string currentScene = "";

    // ���� Ŭ�� ���� ��ųʸ�
    private Dictionary<string, AudioClip> musicMappings = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        // �̱��� ���� ����
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // �ʱ�ȭ
        Initialize();
    }

    private void Initialize()
    {
        // ����� �ҽ��� �Ҵ���� ���� ��� �ڵ� ����
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("Music_Source");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        // ���� Ŭ�� ���� �ʱ�ȭ
        SetupMusicMappings();

        // ���� �ʱ�ȭ
        LoadVolumeSettings();

        // �� ���� �̺�Ʈ ����
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void SetupMusicMappings()
    {
        // �⺻ �� ���� ����
        if (lobbyMusic != null) musicMappings["Lobby"] = lobbyMusic;
        if (townMusic != null) musicMappings["Village"] = townMusic;

        // é�� ���� ���� ����
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
        // SaveManager���� ���� ���� �ε�
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

        // ���� ����
        ApplyMusicVolume();
    }

    private void ApplyMusicVolume()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * 0.3f; // ��ü ������ 30%�� ���� (�ʿ�� ����)
        }
    }

    // ���� �ε�� �� ȣ��Ǵ� �޼���
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // �̹� ���� ���� �´� ������ ��� ���̸� ����
        if (currentScene == scene.name)
            return;

        currentScene = scene.name;

        // ���� �´� ������� ���
        PlayMusicForScene(scene.name);

        Debug.Log($"����Ǵ� �� �̸�: {scene.name}");
    }

    // �� �̸��� ���� ������ ������� ���
    private void PlayMusicForScene(string sceneName)
    {
        AudioClip clipToPlay = null;

        // ��ųʸ����� ���� ���� Ȯ��
        if (musicMappings.TryGetValue(sceneName, out clipToPlay))
        {
            // ��ųʸ����� ��Ȯ�� �� �̸� ��Ī�� ã����
        }
        else
        {
            // ��Ȯ�� ��Ī�� ������ �κ� ���ڿ� ��
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

        // ���� ���
        if (clipToPlay != null && musicSource != null)
        {
            // ���� ��� ���� Ŭ���� �ٸ��� ��ü
            if (musicSource.clip != clipToPlay)
            {
                Debug.Log($"������� ����: {sceneName} -> {clipToPlay.name}");
                musicSource.clip = clipToPlay;
                musicSource.Play();

                // ���� ����
                ApplyMusicVolume();
            }
        }
        else
        {
            Debug.LogWarning($"�� '{sceneName}'�� ���� ��������� ã�� �� �����ϴ�.");
        }
    }

    // ���� ���� ���� �޼���
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyMusicVolume();

        // SaveManager�� ������� ����
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

    // Ư�� ���� ���� ��� �޼��� - é�� ����
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
            Debug.LogWarning($"é�� {chapterNumber}�� ���� ��������� ã�� �� �����ϴ�.");
        }
    }

    // Ư�� ���� ���� ��� �޼��� - ���ڿ� ����
    public void PlayMusic(string musicType)
    {
        AudioClip clipToPlay = null;

        // ���� Ÿ�Կ� ���� Ŭ�� ����
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

        // ���� ���
        if (clipToPlay != null && musicSource != null)
        {
            musicSource.clip = clipToPlay;
            musicSource.Play();
            ApplyMusicVolume();
        }
        else
        {
            Debug.LogWarning($"���� Ÿ�� '{musicType}'�� ���� ����� Ŭ���� ã�� �� �����ϴ�.");
        }
    }

    // ���� �Ͻ�����
    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    // ���� �簳
    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
    }

    // ���� ����
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    // ���� ��� ���� ������� Ŭ�� �̸� ��ȯ
    public string GetCurrentMusicName()
    {
        if (musicSource != null && musicSource.clip != null)
        {
            return musicSource.clip.name;
        }
        return "None";
    }

    // �� ���� �� ����
    private void OnDestroy()
    {
        // �� ���� �̺�Ʈ ���� ����
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}