using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleTemporalDeviceUI : MonoBehaviour
{
    [Header("UI ���")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI crystalCountText;
    [SerializeField] private TextMeshProUGUI deviceCountText;
    [SerializeField] private Transform deviceContainer;
    [SerializeField] private GameObject deviceButtonPrefab;

    [Header("�� ���� �г�")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private TextMeshProUGUI deviceNameText;
    [SerializeField] private TextMeshProUGUI deviceDescriptionText;
    [SerializeField] private TextMeshProUGUI deviceCostText;
    [SerializeField] private Image deviceIconImage;
    [SerializeField] private Button unlockButton;
    [SerializeField] private TextMeshProUGUI unlockButtonText;
    [SerializeField] private GameObject lockedOverlay;

    [Header("�ر� ȿ��")]
    [SerializeField] private UnlockEffect unlockEffect;
    [SerializeField] private AudioSource unlockSound;

    // ��ġ ������ ��������Ʈ
    [SerializeField] private List<Sprite> deviceIcons = new List<Sprite>();

    private TemporalDeviceManager deviceManager;
    private InventorySystem inventorySystem;
    private TemporalDevice selectedDevice;
    private List<GameObject> deviceButtons = new List<GameObject>();

    private void Awake()
    {
        // �ʱ� ���� ����
        if (detailPanel != null) detailPanel.SetActive(false);

        // ��ư �̺�Ʈ ����
        if (closeButton != null) closeButton.onClick.AddListener(CloseUI);
        if (unlockButton != null) unlockButton.onClick.AddListener(UnlockSelectedDevice);
    }

    private void Start()
    {
        // �Ŵ��� ���� ��������
        deviceManager = TemporalDeviceManager.Instance;
        inventorySystem = InventorySystem.Instance;

        if (deviceManager == null || inventorySystem == null)
        {
            Debug.LogError("�ʿ��� �Ŵ����� ã�� �� �����ϴ�.");
            return;
        }

        // ��ġ ��ư �ʱ�ȭ
        InitializeDeviceButtons();

        // ũ����Ż ���� ������Ʈ
        UpdateCrystalCount();

        // ��ġ �ر� �̺�Ʈ ����
        deviceManager.OnDeviceUnlocked += OnDeviceUnlocked;
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        if (deviceManager != null)
        {
            deviceManager.OnDeviceUnlocked -= OnDeviceUnlocked;
        }
    }

    // UI ����
    public void OpenUI()
    {
        gameObject.SetActive(true);

        // ũ����Ż ���� ������Ʈ
        UpdateCrystalCount();

        // �رݵ� ��ġ ���� ������Ʈ
        UpdateDeviceCount();

        // ��ư ���� ������Ʈ
        UpdateDeviceButtons();
    }

    // UI �ݱ�
    public void CloseUI()
    {
        // �� �г� �ݱ�
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }

        // ��ü UI �ݱ�
        gameObject.SetActive(false);
    }

    // ��ġ ��ư �ʱ�ȭ
    public void InitializeDeviceButtons()
    {
        if (deviceContainer == null || deviceButtonPrefab == null) return;

        // ���� ��ư ����
        foreach (var button in deviceButtons)
        {
            Destroy(button);
        }
        deviceButtons.Clear();

        // �� ��ư ����
        List<TemporalDevice> devices = deviceManager.GetAllDevices();
        foreach (var device in devices)
        {
            GameObject buttonObj = Instantiate(deviceButtonPrefab, deviceContainer);
            deviceButtons.Add(buttonObj);

            // ��ư ����
            DeviceButton buttonScript = buttonObj.GetComponent<DeviceButton>();
            if (buttonScript != null)
            {
                // ������ ���� (����̽� ID�� ���� ������ ����)
                Sprite icon = null;
                if (device.ID < deviceIcons.Count)
                {
                    icon = deviceIcons[device.ID];
                }

                // ��ư �ʱ�ȭ
                buttonScript.Initialize(device, icon);
                buttonScript.OnButtonClicked += ShowDeviceDetail;
            }
        }

        //// ������ ũ�� ������Ʈ (��ũ�� �� ����)
        //Canvas.ForceUpdateCanvases();
        //LayoutRebuilder.ForceRebuildLayoutImmediate(deviceContainer as RectTransform);
    }

    // ��ġ ��ư ���� ������Ʈ
    private void UpdateDeviceButtons()
    {
        foreach (var buttonObj in deviceButtons)
        {
            DeviceButton buttonScript = buttonObj.GetComponent<DeviceButton>();
            if (buttonScript != null)
            {
                buttonScript.UpdateButtonState();
            }
        }
    }

    // ũ����Ż ���� ������Ʈ
    private void UpdateCrystalCount()
    {
        if (crystalCountText == null || inventorySystem == null) return;

        int count = inventorySystem.GetItemQuantity(3001); // �ð� ���� ������ ID
        crystalCountText.text = $"�ð� ����: {count}";
    }

    // �رݵ� ��ġ ���� ������Ʈ
    private void UpdateDeviceCount()
    {
        if (deviceCountText == null || deviceManager == null) return;

        int unlocked = deviceManager.GetUnlockedDeviceCount();
        int total = deviceManager.GetAllDevices().Count;
        deviceCountText.text = $"�رݵ� ��ġ: {unlocked}/{total}";
    }

    // ��ġ �� ���� ǥ��
    public void ShowDeviceDetail(TemporalDevice device)
    {
        if (detailPanel == null) return;

        selectedDevice = device;

        // �г� Ȱ��ȭ
        detailPanel.SetActive(true);

        // ���� ����
        if (deviceNameText != null)
            deviceNameText.text = device.DeviceName;

        if (deviceDescriptionText != null)
            deviceDescriptionText.text = device.Description;

        if (deviceCostText != null)
            deviceCostText.text = $"���: {device.TimeCrystalCost} �ð� ����";

        // ������ ����
        if (deviceIconImage != null && device.ID < deviceIcons.Count)
        {
            deviceIconImage.sprite = deviceIcons[device.ID];
        }

        // ��ư ���� ������Ʈ
        UpdateUnlockButton();
    }

    // �ر� ��ư ���� ������Ʈ
    private void UpdateUnlockButton()
    {
        if (unlockButton == null || selectedDevice == null) return;

        bool isUnlocked = selectedDevice.IsUnlocked;
        int crystalCount = inventorySystem.GetItemQuantity(3001);
        bool hasEnoughCrystals = crystalCount >= selectedDevice.TimeCrystalCost;

        // ��� �������� ����
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!isUnlocked);
        }

        // ��ư Ȱ��ȭ ���� ����
        unlockButton.interactable = !isUnlocked && hasEnoughCrystals;

        // ��ư �ؽ�Ʈ ����
        if (unlockButtonText != null)
        {
            if (isUnlocked)
            {
                unlockButtonText.text = "�رݵ�";
            }
            else if (!hasEnoughCrystals)
            {
                unlockButtonText.text = "�ð� ���� ����";
            }
            else
            {
                unlockButtonText.text = "�ر��ϱ�";
            }
        }
    }

    // ���õ� ��ġ �ر� �õ�
    private void UnlockSelectedDevice()
    {
        if (selectedDevice == null || deviceManager == null) return;

        bool success = deviceManager.TryUnlockDevice(selectedDevice.ID);

        if (success)
        {

            // �ر� ȿ�� ǥ��
            ShowUnlockEffect();
            // UI ������Ʈ
            UpdateCrystalCount();
            UpdateDeviceCount();
            UpdateUnlockButton();
            UpdateDeviceButtons();
        }
    }

    // �ر� ȿ�� ǥ��
    private void ShowUnlockEffect()
    {
        unlockEffect.gameObject.SetActive(true);
        // ���� ���
        if (unlockSound != null)
        {
            unlockSound.Play();
        }

        // UnlockEffect Ȱ��
        if (unlockEffect != null && selectedDevice != null)
        {
            // ������ ��������
            Sprite icon = null;
            if (selectedDevice.ID < deviceIcons.Count)
            {
                icon = deviceIcons[selectedDevice.ID];
            }

            // ȿ�� ���
            unlockEffect.PlayEffect(selectedDevice.DeviceName, icon);
        }
    }

    // ��ġ �ر� �̺�Ʈ ó��
    private void OnDeviceUnlocked(TemporalDevice device)
    {
        // UI ������Ʈ
        UpdateDeviceButtons();
        UpdateDeviceCount();

        // ���� ���õ� ��ġ�� �رݵ� ��ġ�� ������ ��ư ���� ������Ʈ
        if (selectedDevice != null && selectedDevice.ID == device.ID)
        {
            UpdateUnlockButton();
        }
    }
}