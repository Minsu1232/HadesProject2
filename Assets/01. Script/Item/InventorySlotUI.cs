// ItemSlotUI.cs - 인벤토리 아이템 슬롯 UI 컴포넌트
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemQuantityText;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private GameObject selectedHighlight;
    [SerializeField] private TextMeshProUGUI quantityText;
    private Item currentItem;
    private Action onClickCallback;

    private Color[] rarityColors = {
        new Color(0.7f, 0.7f, 0.7f), // Common - 회색
        new Color(0.3f, 0.7f, 0.3f), // Uncommon - 녹색
        new Color(0.3f, 0.3f, 0.9f), // Rare - 파란색
        new Color(0.7f, 0.3f, 0.9f), // Epic - 보라색
        new Color(1.0f, 0.8f, 0.0f)  // Legendary - 금색
    };
   

    // 초기화
    private void Start()
    {
        // 클릭 이벤트 설정
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => {
                onClickCallback?.Invoke();
                SetSelected(true);
            });
        }

        // 선택 강조 초기화
        if (selectedHighlight != null)
        {
            selectedHighlight.SetActive(false);
        }
    }

    // 아이템 설정
    // ItemSlotUI.cs의 SetItem 메서드 수정
    public void SetItem(Item item, int quantity)
    {
        currentItem = item;

        if (item != null)
        {
            // 아이템 있음: 아이콘 표시
            if (itemIcon != null)
            {
                itemIcon.sprite = item.icon;
                itemIcon.enabled = true;
            }

            // 수량 표시 (2개 이상일 때만 표시)
            if (quantityText != null)
            {
                if (quantity > 1)
                {
                    quantityText.text = "x" + quantity.ToString();
                    quantityText.gameObject.SetActive(true);
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // 빈 슬롯: 아이콘 숨김
            if (itemIcon != null)
            {
                itemIcon.enabled = false;
            }

            if (quantityText != null)
            {
                quantityText.gameObject.SetActive(false);
            }
        }
    }

    // 클릭 콜백 설정
    public void SetClickCallback(Action callback)
    {
        onClickCallback = callback;
    }

    // 선택 상태 설정
    public void SetSelected(bool selected)
    {
        if (selectedHighlight != null)
        {
            selectedHighlight.SetActive(selected);
        }

        // 다른 슬롯의 선택 해제
        if (selected)
        {
            ItemSlotUI[] allSlots = FindObjectsOfType<ItemSlotUI>();
            foreach (var slot in allSlots)
            {
                if (slot != this && slot.selectedHighlight != null)
                {
                    slot.selectedHighlight.SetActive(false);
                }
            }
        }
    }

    // 아이템 반환
    public Item GetItem()
    {
        return currentItem;
    }
}