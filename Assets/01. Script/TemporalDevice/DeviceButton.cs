using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeviceButton : MonoBehaviour
{
    [SerializeField] private Image deviceIcon;
    [SerializeField] private TextMeshProUGUI deviceNameText;
    [SerializeField] private TextMeshProUGUI deviceCostText;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private GameObject selectedIndicator;
    [SerializeField] private Button button;

    // 버튼 클릭 이벤트
    public event Action<TemporalDevice> OnButtonClicked;

    private TemporalDevice device;
    private bool isSelected = false;

    private void Awake()
    {
        // 버튼 이벤트 연결
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }

        // 선택 표시 초기 상태
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(false);
        }
    }

    // 장치 정보로 버튼 초기화
    public void Initialize(TemporalDevice deviceData, Sprite icon)
    {
        device = deviceData;

        // 아이콘 설정
        if (deviceIcon != null && icon != null)
        {
            deviceIcon.sprite = icon;
        }

        // 상태 업데이트
        UpdateButtonState();
    }

    // 버튼 상태 업데이트
    public void UpdateButtonState()
    {
        if (device == null) return;

        // 이름 설정
        if (deviceNameText != null)
        {
            deviceNameText.text = device.DeviceName;
        }

        // 비용 설정
        if (deviceCostText != null)
        {
            deviceCostText.text = device.TimeCrystalCost.ToString();
        }

        // 잠금 오버레이 설정
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!device.IsUnlocked);
        }

        // 해금 상태에 따른 아이콘 색상 설정
        if (deviceIcon != null)
        {
            Color iconColor = device.IsUnlocked ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.8f);
            deviceIcon.color = iconColor;
        }
    }

    // 선택 상태 설정
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(selected);
        }
    }

    // 버튼 클릭 처리
    private void OnClick()
    {
        // 클릭 이벤트 발생
        OnButtonClicked?.Invoke(device);

        // 자동으로 선택 상태로 설정
        SetSelected(true);

        // 다른 버튼들의 선택 상태 해제
        DeviceButton[] otherButtons = transform.parent.GetComponentsInChildren<DeviceButton>();
        foreach (var otherButton in otherButtons)
        {
            if (otherButton != this)
            {
                otherButton.SetSelected(false);
            }
        }
    }
}