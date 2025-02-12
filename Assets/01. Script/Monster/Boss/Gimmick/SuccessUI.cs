using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class SuccessUI : MonoBehaviour, ISuccessUI
{
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private Transform iconParent;
    // 단일 스프라이트 사용
    [SerializeField] private Sprite baseSprite;

    // 빈 상태와 채워진 상태의 색상을 지정합니다.
    [SerializeField] private Color emptyColor = Color.white;
    [SerializeField] private Color filledColor = Color.green;

    private List<Image> successIcons = new List<Image>();

    public void InitializeSuccessUI(int maxSuccessCount)
    {
        // 기존 아이콘 제거
        foreach (Transform child in iconParent)
        {
            Destroy(child.gameObject);
        }
        successIcons.Clear();

        // maxSuccessCount 만큼 동적 생성
        for (int i = 0; i < maxSuccessCount; i++)
        {
            GameObject iconObj = Instantiate(iconPrefab, iconParent);
            Image iconImage = iconObj.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = baseSprite;
                // 초기 상태는 emptyColor로 설정
                iconImage.color = emptyColor;
                successIcons.Add(iconImage);
            }
        }
    }

    public void UIOff()
    {
     
        gameObject.SetActive(false);
        
    }

    public void UpdateSuccessCount(int currentSuccessCount)
    {
        for (int i = 0; i < successIcons.Count; i++)
        {
            if (i < currentSuccessCount)
            {
                // 성공한 아이콘은 filledColor로 변경 (Yoyo 루프 없이)
                successIcons[i].DOColor(filledColor, 0.2f).SetEase(Ease.OutBack);
            }
            else
            {
                // 나머지는 emptyColor 유지
                successIcons[i].DOColor(emptyColor, 0.2f).SetEase(Ease.OutBack);
            }
        }
    }
}
