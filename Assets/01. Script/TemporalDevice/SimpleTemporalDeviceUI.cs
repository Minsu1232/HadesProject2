using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleTemporalDeviceUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI crystalCountText;
    [SerializeField] private TextMeshProUGUI deviceCountText;
    [SerializeField] private Transform deviceContainer;
    [SerializeField] private GameObject deviceButtonPrefab;

    [Header("상세 정보 패널")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private TextMeshProUGUI deviceNameText;
    [SerializeField] private TextMeshProUGUI deviceDescriptionText;
    [SerializeField] private TextMeshProUGUI deviceCostText;
    [SerializeField] private Image deviceIconImage;
    [SerializeField] private Button unlockButton;
    [SerializeField] private TextMeshProUGUI unlockButtonText;
    [SerializeField] private GameObject lockedOverlay;

    [Header("해금 효과")]
    [SerializeField] private UnlockEffect unlockEffect;
    [SerializeField] private AudioSource unlockSound;

    // 장치 아이콘 스프라이트
    [SerializeField] private List<Sprite> deviceIcons = new List<Sprite>();

    private TemporalDeviceManager deviceManager;
    private InventorySystem inventorySystem;
    private TemporalDevice selectedDevice;
    private List<GameObject> deviceButtons = new List<GameObject>();

    private void Awake()
    {
        // 초기 상태 설정
        if (detailPanel != null) detailPanel.SetActive(false);

        // 버튼 이벤트 연결
        if (closeButton != null) closeButton.onClick.AddListener(CloseUI);
        if (unlockButton != null) unlockButton.onClick.AddListener(UnlockSelectedDevice);
    }

    private void Start()
    {
        // 매니저 참조 가져오기
        deviceManager = TemporalDeviceManager.Instance;
        inventorySystem = InventorySystem.Instance;

        if (deviceManager == null || inventorySystem == null)
        {
            Debug.LogError("필요한 매니저를 찾을 수 없습니다.");
            return;
        }

        // 장치 버튼 초기화
        InitializeDeviceButtons();

        // 크리스탈 개수 업데이트
        UpdateCrystalCount();

        // 장치 해금 이벤트 연결
        deviceManager.OnDeviceUnlocked += OnDeviceUnlocked;
    }

    private void OnDestroy()
    {
        // 이벤트 연결 해제
        if (deviceManager != null)
        {
            deviceManager.OnDeviceUnlocked -= OnDeviceUnlocked;
        }
    }

    // UI 열기
    public void OpenUI()
    {
        gameObject.SetActive(true);

        // 크리스탈 개수 업데이트
        UpdateCrystalCount();

        // 해금된 장치 개수 업데이트
        UpdateDeviceCount();

        // 버튼 상태 업데이트
        UpdateDeviceButtons();
    }

    // UI 닫기
    public void CloseUI()
    {
        // 상세 패널 닫기
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }

        // 전체 UI 닫기
        gameObject.SetActive(false);
    }

    // 장치 버튼 초기화
    public void InitializeDeviceButtons()
    {
        if (deviceContainer == null || deviceButtonPrefab == null) return;

        // 기존 버튼 제거
        foreach (var button in deviceButtons)
        {
            Destroy(button);
        }
        deviceButtons.Clear();

        // 새 버튼 생성
        List<TemporalDevice> devices = deviceManager.GetAllDevices();
        foreach (var device in devices)
        {
            GameObject buttonObj = Instantiate(deviceButtonPrefab, deviceContainer);
            deviceButtons.Add(buttonObj);

            // 버튼 설정
            DeviceButton buttonScript = buttonObj.GetComponent<DeviceButton>();
            if (buttonScript != null)
            {
                // 아이콘 설정 (디바이스 ID에 따라 아이콘 선택)
                Sprite icon = null;
                if (device.ID < deviceIcons.Count)
                {
                    icon = deviceIcons[device.ID];
                }

                // 버튼 초기화
                buttonScript.Initialize(device, icon);
                buttonScript.OnButtonClicked += ShowDeviceDetail;
            }
        }

        //// 컨텐츠 크기 업데이트 (스크롤 뷰 관련)
        //Canvas.ForceUpdateCanvases();
        //LayoutRebuilder.ForceRebuildLayoutImmediate(deviceContainer as RectTransform);
    }

    // 장치 버튼 상태 업데이트
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

    // 크리스탈 개수 업데이트
    private void UpdateCrystalCount()
    {
        if (crystalCountText == null || inventorySystem == null) return;

        int count = inventorySystem.GetItemQuantity(3001); // 시간 조각 아이템 ID
        crystalCountText.text = $"시간 조각: {count}";
    }

    // 해금된 장치 개수 업데이트
    private void UpdateDeviceCount()
    {
        if (deviceCountText == null || deviceManager == null) return;

        int unlocked = deviceManager.GetUnlockedDeviceCount();
        int total = deviceManager.GetAllDevices().Count;
        deviceCountText.text = $"해금된 장치: {unlocked}/{total}";
    }

    // 장치 상세 정보 표시
    public void ShowDeviceDetail(TemporalDevice device)
    {
        if (detailPanel == null) return;

        selectedDevice = device;

        // 패널 활성화
        detailPanel.SetActive(true);

        // 정보 설정
        if (deviceNameText != null)
            deviceNameText.text = device.DeviceName;

        if (deviceDescriptionText != null)
            deviceDescriptionText.text = device.Description;

        if (deviceCostText != null)
            deviceCostText.text = $"비용: {device.TimeCrystalCost} 시간 조각";

        // 아이콘 설정
        if (deviceIconImage != null && device.ID < deviceIcons.Count)
        {
            deviceIconImage.sprite = deviceIcons[device.ID];
        }

        // 버튼 상태 업데이트
        UpdateUnlockButton();
    }

    // 해금 버튼 상태 업데이트
    private void UpdateUnlockButton()
    {
        if (unlockButton == null || selectedDevice == null) return;

        bool isUnlocked = selectedDevice.IsUnlocked;
        int crystalCount = inventorySystem.GetItemQuantity(3001);
        bool hasEnoughCrystals = crystalCount >= selectedDevice.TimeCrystalCost;

        // 잠금 오버레이 설정
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!isUnlocked);
        }

        // 버튼 활성화 상태 설정
        unlockButton.interactable = !isUnlocked && hasEnoughCrystals;

        // 버튼 텍스트 설정
        if (unlockButtonText != null)
        {
            if (isUnlocked)
            {
                unlockButtonText.text = "해금됨";
            }
            else if (!hasEnoughCrystals)
            {
                unlockButtonText.text = "시간 조각 부족";
            }
            else
            {
                unlockButtonText.text = "해금하기";
            }
        }
    }

    // 선택된 장치 해금 시도
    private void UnlockSelectedDevice()
    {
        if (selectedDevice == null || deviceManager == null) return;

        bool success = deviceManager.TryUnlockDevice(selectedDevice.ID);

        if (success)
        {

            // 해금 효과 표시
            ShowUnlockEffect();
            // UI 업데이트
            UpdateCrystalCount();
            UpdateDeviceCount();
            UpdateUnlockButton();
            UpdateDeviceButtons();
        }
    }

    // 해금 효과 표시
    private void ShowUnlockEffect()
    {
        unlockEffect.gameObject.SetActive(true);
        // 사운드 재생
        if (unlockSound != null)
        {
            unlockSound.Play();
        }

        // UnlockEffect 활용
        if (unlockEffect != null && selectedDevice != null)
        {
            // 아이콘 가져오기
            Sprite icon = null;
            if (selectedDevice.ID < deviceIcons.Count)
            {
                icon = deviceIcons[selectedDevice.ID];
            }

            // 효과 재생
            unlockEffect.PlayEffect(selectedDevice.DeviceName, icon);
        }
    }

    // 장치 해금 이벤트 처리
    private void OnDeviceUnlocked(TemporalDevice device)
    {
        // UI 업데이트
        UpdateDeviceButtons();
        UpdateDeviceCount();

        // 현재 선택된 장치가 해금된 장치와 같으면 버튼 상태 업데이트
        if (selectedDevice != null && selectedDevice.ID == device.ID)
        {
            UpdateUnlockButton();
        }
    }
}