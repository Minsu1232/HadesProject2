// AbilitySelectionPanel.cs - 능력 선택 패널 관리자
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class AbilitySelectionPanel : MonoBehaviour
{
    [Header("패널 구성 요소")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private GameObject abilityCardPrefab;

    [Header("애니메이션 설정")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float cardAppearDelay = 0.15f;
    [SerializeField] private float cardAppearDuration = 0.4f;
    [SerializeField] private float panelScaleDuration = 0.4f;

    [Header("사운드 효과")]
    [SerializeField] private AudioClip panelOpenSound;
    [SerializeField] private AudioClip confirmSound;

    private AudioSource audioSource;
    private List<GameObject> activeCards = new List<GameObject>();
    private System.Action onSelectionCompleted;

    private void Awake()
    {
        // 오디오 소스 컴포넌트 가져오거나 생성
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 초기 상태 설정
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // 패널 초기 설정
        gameObject.SetActive(false);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    // 능력 선택 패널 표시
    public void ShowSelectionPanel(List<DungeonAbility> abilities, System.Action onCompleted = null)
    {
        Debug.Log($"ShowSelectionPanel 호출됨: 능력 수 = {abilities.Count}");

        onSelectionCompleted = onCompleted;

        // 기존 카드 정리
        ClearCards();

        // 패널 활성화
        gameObject.SetActive(true);

        // 계층구조의 모든 자식 오브젝트 활성화
        SetAllChildrenActive(transform, true);

        Debug.Log("패널 및 자식 게임오브젝트 활성화 완료");

        // 오프닝 사운드
        if (panelOpenSound != null && audioSource != null)
            audioSource.PlayOneShot(panelOpenSound);

        // 패널 애니메이션
        StartPanelAnimation();

        // 카드 컨테이너 확인
        if (cardContainer == null)
        {
            Debug.LogError("카드 컨테이너가 null입니다!");
            cardContainer = transform;
        }

        // 카드 생성 확인
        Debug.Log($"카드 생성 시작: container = {(cardContainer != null ? "있음" : "없음")}, prefab = {(abilityCardPrefab != null ? "있음" : "없음")}");

        // 카드 생성 및 애니메이션
        StartCoroutine(CreateCardsWithAnimation(abilities));
    }

    // 패널 열리는 애니메이션
    private void StartPanelAnimation()
    {
       
        canvasGroup.gameObject.SetActive(true);
        
        // 캔버스 그룹 페이드 인
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            canvasGroup.DOFade(1f, fadeInDuration)
                .SetEase(Ease.OutQuad);
        }

        // 패널 스케일 애니메이션
        if (panelRect != null)
        {
            panelRect.localScale = Vector3.one * 0.9f;
            panelRect.DOScale(Vector3.one, panelScaleDuration)
                .SetEase(Ease.OutBack);
        }
    }

    // 카드 생성 및 애니메이션
    private System.Collections.IEnumerator CreateCardsWithAnimation(List<DungeonAbility> abilities)
    { // 첫 번째 카드 로드 전에 약간의 지연 추가 (모든 아이콘 로드 완료 대기)
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < abilities.Count; i++)
        {
            // 카드 생성
            GameObject cardObj = Instantiate(abilityCardPrefab, cardContainer);
            AbilityCardUI cardUI = cardObj.GetComponent<AbilityCardUI>();

            // 초기 설정 (보이지 않게)
            cardObj.transform.localScale = Vector3.zero;

            // 카드 설정
            if (cardUI != null)
            {
                cardUI.Setup(abilities[i], OnAbilitySelected);
                activeCards.Add(cardObj);
            }

            // 카드 등장 애니메이션
            cardObj.transform.DOScale(Vector3.one, cardAppearDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(i * cardAppearDelay);

            // 다음 카드 생성 전 잠시 대기
            yield return new WaitForSeconds(cardAppearDelay);
        }
    }

    // 능력 선택 처리
    private void OnAbilitySelected(DungeonAbility ability)
    {
        // 선택 효과음
        if (confirmSound != null && audioSource != null)
            audioSource.PlayOneShot(confirmSound);

        // 능력 적용
        DungeonAbilityManager.Instance.AcquireAbility(ability);
        Debug.Log("어빌리티 선택됨: " + ability.name);
        // 패널 닫기 애니메이션
        ClosePanelWithAnimation();
    }

    // 패널 닫기 애니메이션
    // 패널 닫기 애니메이션 (수정)
    private void ClosePanelWithAnimation()
    {
        // 인터랙션 방지 (gameObject 비활성화 안함)
        if (canvasGroup != null)
        {
            // canvasGroup.gameObject.SetActive(false); // 이 줄 제거
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // 선택되지 않은 카드 페이드 아웃
        foreach (var card in activeCards)
        {
            card.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack);
        }

        // 패널 페이드 아웃
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, fadeInDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => {
                    // 패널 비활성화
                    gameObject.SetActive(false);

                    // 완료 콜백 호출
                    onSelectionCompleted?.Invoke();
                });
        }
    }
    // 모든 자식 오브젝트 활성화/비활성화하는 도우미 메서드 (새로 추가)
    private void SetAllChildrenActive(Transform parent, bool active)
    {
        // 현재 Transform의 게임 오브젝트 활성화
        parent.gameObject.SetActive(active);

        // 모든 자식 오브젝트에 대해 재귀적으로 호출
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            child.gameObject.SetActive(active);

            // 자식이 또 있으면 재귀적으로 처리
            if (child.childCount > 0)
            {
                SetAllChildrenActive(child, active);
            }
        }
    }
    // 카드 정리
    private void ClearCards()
    {
        foreach (var card in activeCards)
        {
            Destroy(card);
        }
        activeCards.Clear();
    }

    // 타이틀 설정
    public void SetTitle(string title)
    {
        if (titleText != null)
            titleText.text = title;
    }
}