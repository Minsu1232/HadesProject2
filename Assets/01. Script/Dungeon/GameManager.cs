using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // �� �̸� ���
    private const string LOBBY_SCENE = "Lobby";
    private const string VILLAGE_SCENE = "Village";

    [SerializeField] private List<GameObject> toggleableUIElements;
    private List<GameObject> activeUIElements = new List<GameObject>();

    // ���� �÷��� �ð� ����
    private float sessionStartTime;
    private float totalPlayTime = 0f;

    // ��ũ���� ĸó �ػ�
    [SerializeField] private int screenshotWidth = 256;
    [SerializeField] private int screenshotHeight = 192;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ���� ���� �ð� ���
            sessionStartTime = Time.time;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (activeUIElements.Count > 0)
            {
                // �������� Ȱ��ȭ�� UI ��Ȱ��ȭ
                GameObject lastActiveUI = activeUIElements[activeUIElements.Count - 1];
                lastActiveUI.SetActive(false);
                activeUIElements.Remove(lastActiveUI);
            }
            else
            {
                // Ȱ��ȭ�� UI�� ���� �� ESC�� ������ ���� �Ͻ����� �޴� ǥ�� ��
                //ShowPauseMenu();
            }
        }
    }

    // �� UI ����� Ȱ��ȭ ���°� ����� �� ȣ��
    public void OnUIStateChanged(GameObject uiElement, bool isActive)
    {
        if (isActive)
        {
            if (!activeUIElements.Contains(uiElement))
                activeUIElements.Add(uiElement);
        }
        else
        {
            activeUIElements.Remove(uiElement);
        }
    }

    // �� ���� ����
    public void StartNewGame()
    {
        // �ε� ȭ�� ǥ��
        if (LoadingScreen.Instance != null)
        {
            // �� ������ Village ������ ����
            LoadingScreen.Instance.ShowLoading(VILLAGE_SCENE, () => {
                // �ʿ��� �ʱ�ȭ ����
                Debug.Log("�� ���� ���� �Ϸ�");

                // ��ũ���� ĸó (�ʿ��)
                CaptureScreenshot();

                // ���� ������ �ʱ� ����
                SaveGameData();
            });
        }
        else
        {
            // �ε� ȭ���� ���� ��� ���� �� �ε�
            SceneManager.LoadScene(VILLAGE_SCENE);
        }
    }

    // ���� �ε�
    public void LoadGameScene()
    {
        // �ε� ȭ�� ǥ��
        if (LoadingScreen.Instance != null)
        {
            // ���� �ҷ������ Village ������ �̵�
            LoadingScreen.Instance.ShowLoading(VILLAGE_SCENE, () => {
                // �ʿ��� �ʱ�ȭ ����
                Debug.Log("���� �ε� �Ϸ�");

                // ���� ������ ����
                ApplyGameData();
            });
        }
        else
        {
            // �ε� ȭ���� ���� ��� ���� �� �ε�
            SceneManager.LoadScene(VILLAGE_SCENE);
        }
    }

    // ���� ȭ�� ��ũ���� ĸó
    public void CaptureScreenshot()
    {
        StartCoroutine(CaptureScreenshotCoroutine());
    }

    private IEnumerator CaptureScreenshotCoroutine()
    {
        // �� ������ ����Ͽ� UI�� ������ �������ǵ��� ��
        yield return new WaitForEndOfFrame();

        // ���� ���� ��ȣ ��������
        int currentSlot = SaveManager.Instance.GetCurrentSlot();

        // ��ũ���� ��� ����
        string screenshotDir = System.IO.Path.Combine(
            Application.persistentDataPath,
            "SaveFiles",
            $"Slot{currentSlot}"
        );

        string screenshotPath = System.IO.Path.Combine(screenshotDir, "screenshot.png");

        // ���丮�� ������ ����
        if (!System.IO.Directory.Exists(screenshotDir))
        {
            System.IO.Directory.CreateDirectory(screenshotDir);
        }

        // ���� �ؽ�ó ���� �� ĸó
        RenderTexture rt = new RenderTexture(screenshotWidth, screenshotHeight, 24);
        RenderTexture prevRT = RenderTexture.active;
        Camera.main.targetTexture = rt;
        RenderTexture.active = rt;
        Camera.main.Render();

        Texture2D screenShot = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
        screenShot.Apply();

        // ���� ���� Ÿ�� ����
        Camera.main.targetTexture = null;
        RenderTexture.active = prevRT;
        Destroy(rt);

        // ��ũ���� ����
        byte[] bytes = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(screenshotPath, bytes);

        Debug.Log($"��ũ���� ���� �Ϸ�: {screenshotPath}");
    }

    // ���� ������ ����
    public void SaveGameData()
    {
        // �÷��� Ÿ�� ������Ʈ
        UpdatePlayTime();

        // ���� ���� ������ ����
        SaveManager.Instance.SaveAllData();
    }

    // �÷��� Ÿ�� ������Ʈ
    private void UpdatePlayTime()
    {
        // ���� ���� �÷��� �ð� ���
        float sessionTime = Time.time - sessionStartTime;

        // ����� ��ü �÷��� �ð��� ���� ���� �ð� �߰�
        totalPlayTime += sessionTime;

        // ���� ������ ���� ���� �ð� �缳��
        sessionStartTime = Time.time;

        // �÷��� Ÿ�� ������ ������Ʈ (SaveManager�� �޼��� �߰� �ʿ�)
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.UpdatePlayTime((int)totalPlayTime);
        }
    }

    // ���� ������ ����
    private void ApplyGameData()
    {
        // PlayerClass, InventorySystem, FragmentManager ������Ʈ ã��
        PlayerClass playerClass = GameInitializer.Instance.GetPlayerClass();
        InventorySystem inventorySystem = FindObjectOfType<InventorySystem>();
        FragmentManager fragmentManager = FindObjectOfType<FragmentManager>();

        // ������ ����
        if (SaveManager.Instance != null && playerClass != null)
        {
            SaveManager.Instance.ApplyGameData(playerClass, inventorySystem, fragmentManager);
        }
    }

    // ���ø����̼� ���� ��
    private void OnApplicationQuit()
    {
        // ���� ���� �� ���� �÷��� Ÿ�� ����
        UpdatePlayTime();

        // ��� ������ ����
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveAllData();
        }
    }

    // ���� �Ͻ� ������ (�� ��ȯ ��)
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // ������ ��׶���� �� �� ������ ����
            UpdatePlayTime();

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveAllData();
            }
        }
        else
        {
            // ������ �ٽ� ���׶���� ���ƿ� �� ���� ���� �ð� �缳��
            sessionStartTime = Time.time;
        }
    }
}