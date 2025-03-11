// 새 스크립트 생성: FragmentSlotUI.cs
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FragmentSlotUI : MonoBehaviour
{
    [SerializeField] private Image fragmentIcon;
    [SerializeField] private TextMeshProUGUI fragmentNameText;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private GameObject resonatedIndicator;
    [SerializeField] private GameObject selectedHighlight;

    private FragmentItem currentFragment;
    private Action onClickCallback;

    private Color[] rarityColors = {
        new Color(0.7f, 0.7f, 0.7f), // Common
        new Color(0.3f, 0.7f, 0.3f), // Uncommon
        new Color(0.3f, 0.3f, 0.9f), // Rare
        new Color(0.7f, 0.3f, 0.9f), // Epic
        new Color(1.0f, 0.8f, 0.0f)  // Legendary
    };

    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => {
                onClickCallback?.Invoke();
                SetSelected(true);
            });
        }

        if (selectedHighlight != null)
        {
            selectedHighlight.SetActive(false);
        }
    }

    public void SetFragment(FragmentItem fragment)
    {
        currentFragment = fragment;
        Debug.Log($"파편 슬롯 설정: {(fragment != null ? fragment.itemName : "없음")}, 아이콘: {(fragment != null && fragment.icon != null ? "있음" : "없음")}");
        if (fragment != null)
        {
            if (fragmentIcon != null)
            {
                fragmentIcon.sprite = fragment.icon;
                fragmentIcon.enabled = true;
            }

            if (fragmentNameText != null)
            {
                fragmentNameText.text = fragment.itemName;
                fragmentNameText.gameObject.SetActive(true);
            }

            if (rarityBorder != null)
            {
                rarityBorder.color = rarityColors[(int)fragment.rarity];
                rarityBorder.gameObject.SetActive(true);
            }

            if (resonatedIndicator != null)
            {
                resonatedIndicator.SetActive(fragment.isResonated);
            }
        }
        else
        {
            if (fragmentIcon != null)
            {
                fragmentIcon.enabled = false;
            }

            if (fragmentNameText != null)
            {
                fragmentNameText.gameObject.SetActive(false);
            }

            if (rarityBorder != null)
            {
                rarityBorder.gameObject.SetActive(false);
            }

            if (resonatedIndicator != null)
            {
                resonatedIndicator.SetActive(false);
            }
        }
    }

    public void SetClickCallback(Action callback)
    {
        onClickCallback = callback;
    }

    public void SetSelected(bool selected)
    {
        if (selectedHighlight != null)
        {
            selectedHighlight.SetActive(selected);
        }
    }

    public FragmentItem GetFragment()
    {
        return currentFragment;
    }
}