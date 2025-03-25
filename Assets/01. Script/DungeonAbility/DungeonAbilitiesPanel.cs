using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DungeonAbilitiesPanel : MonoBehaviour
{
    [Header("패널 설정")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private Button toggleButton;
    [SerializeField] private TextMeshProUGUI toggleButtonText;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Transform abilitiesContainer;
    [SerializeField] private GameObject abilityItemPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    [Header("희귀도 색상")]
    [SerializeField] private Color commonColor = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color uncommonColor = new Color(0.3f, 0.8f, 0.3f);
    [SerializeField] private Color rareColor = new Color(0.3f, 0.3f, 0.8f);
    [SerializeField] private Color epicColor = new Color(0.8f, 0.3f, 0.8f);
    [SerializeField] private Color legendaryColor = new Color(1.0f, 0.8f, 0.2f);

    private bool isPanelOpen = false;
    private float panelWidth;
    private Vector2 openPosition;
    private Vector2 closedPosition;
    private List<GameObject> abilityItems = new List<GameObject>();

    private void Awake()
    {
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();

        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        // 패널 너비 계산 및 위치 설정
        panelWidth = panelRect.sizeDelta.x;
        openPosition = panelRect.anchoredPosition;
        closedPosition = new Vector2(openPosition.x + panelWidth, openPosition.y);

        // 초기 상태: 닫힘
        panelRect.anchoredPosition = closedPosition;
        isPanelOpen = false;

        if (toggleButtonText != null)
            toggleButtonText.text = "◀";

        // 그리드 레이아웃 설정
        if (gridLayoutGroup == null && abilitiesContainer != null)
        {
            gridLayoutGroup = abilitiesContainer.GetComponent<GridLayoutGroup>();
            if (gridLayoutGroup == null)
            {
                gridLayoutGroup = abilitiesContainer.gameObject.AddComponent<GridLayoutGroup>();
                // 기본 그리드 설정
                gridLayoutGroup.cellSize = new Vector2(130, 180);
                gridLayoutGroup.spacing = new Vector2(10, 10);
                gridLayoutGroup.padding = new RectOffset(10, 10, 10, 10);
                gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayoutGroup.constraintCount = 2; // 한 행에 2개 표시
            }
        }
        if (DungeonAbilityManager.Instance != null)
        {
            DungeonAbilityManager.Instance.OnAbilityAcquired += RefreshAbilitiesList;
            DungeonAbilityManager.Instance.OnAbilityLevelUp += RefreshAbilitiesList;

            // 초기 업데이트
            RefreshAbilitiesList();
        }
       
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        if (DungeonAbilityManager.Instance != null)
        {
            DungeonAbilityManager.Instance.OnAbilityAcquired -= RefreshAbilitiesList;
            DungeonAbilityManager.Instance.OnAbilityLevelUp -= RefreshAbilitiesList;
        }
    }

    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;

        // 패널 애니메이션
        Vector2 targetPosition = isPanelOpen ? openPosition : closedPosition;
        panelRect.DOAnchorPos(targetPosition, animationDuration).SetEase(Ease.OutBack);

        // 버튼 텍스트 변경
        if (toggleButtonText != null)
            toggleButtonText.text = isPanelOpen ? "▶" : "◀";
    }

    // 어빌리티 목록 새로고침
    public void RefreshAbilitiesList()
    {
        // 기존 항목 정리
        ClearAbilityItems();

        // DungeonAbilityManager에서 현재 어빌리티 가져오기
        List<DungeonAbility> abilities = new List<DungeonAbility>();
        if (DungeonAbilityManager.Instance != null)
        {
            abilities = DungeonAbilityManager.Instance.GetCurrentAbilities();
        }
        else
        {
          
            Debug.Log("DungeonAbilityManager가 없습니다.");
           
        }

        // 각 어빌리티에 대한 UI 항목 생성
        foreach (var ability in abilities)
        {
            CreateAbilityItem(ability);
        }

        // 스크롤 위치 초기화
        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }
    }
    
   
    // 어빌리티 항목 생성
    private void CreateAbilityItem(DungeonAbility ability)
    {
        if (abilityItemPrefab == null || abilitiesContainer == null)
            return;

        // 프리팹 인스턴스 생성
        GameObject itemObj = Instantiate(abilityItemPrefab, abilitiesContainer, false);
        abilityItems.Add(itemObj);

        // 컴포넌트 참조
        DungeonAbilityItemUI itemUI = itemObj.GetComponent<DungeonAbilityItemUI>();
        if (itemUI != null)
        {
            // 희귀도에 따른 색상 가져오기
            Color rarityColor = GetRarityColor(ability.rarity);

            // 어빌리티 정보 설정
            itemUI.SetupAbility(ability, rarityColor);
        }
        else
        {
            Debug.LogError("DungeonAbilityItemUI 컴포넌트를 찾을 수 없습니다.");
        }
    }

    // 희귀도에 따른 색상 반환
    private Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return commonColor;
            case Rarity.Uncommon:
                return uncommonColor;
            case Rarity.Rare:
                return rareColor;
            case Rarity.Epic:
                return epicColor;
            case Rarity.Legendary:
                return legendaryColor;
            default:
                return commonColor;
        }
    }

    // 기존 항목 정리
    private void ClearAbilityItems()
    {
        foreach (var item in abilityItems)
        {
            Destroy(item);
        }
        abilityItems.Clear();
    }
}