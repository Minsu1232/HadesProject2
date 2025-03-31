// InteractableNPC.cs - 상호작용 가능한 NPC
using UnityEngine;
using TMPro;

public class InteractableNPC : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] private string npcName = "대장장이";
    [SerializeField] private string interactionPrompt = "F키를 눌러 대화하기";
    [SerializeField] private float interactionRange = 3f; // 상호작용 범위 추가
    [SerializeField] private VillageInteractionManager interactionManager;
    [SerializeField] private NPCType npcType = NPCType.StatUpgrade;

    [Header("UI 요소")]
    [SerializeField] private GameObject promptUI; // 프롬프트 UI 게임 오브젝트
    [SerializeField] private TextMeshProUGUI promptText; // 프롬프트 텍스트 (선택 사항)

    private Transform playerTransform; // 플레이어 트랜스폼 추가
    private bool playerInRange = false;
    private bool promptShown = false;

    public enum NPCType
    {
        StatUpgrade,
        Shop,
        Quest
    }

    private void Start()
    {
        // 플레이어 찾기
        playerTransform = GameInitializer.Instance.GetPlayerClass().playerTransform;
        if (playerTransform == null)
        {
            Debug.LogError("플레이어를 찾을 수 없습니다.");
        }

        // 시작 시 프롬프트 숨기기
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }

  
    }

    private void Update()
    {
        // 플레이어와의 거리 체크
        CheckPlayerDistance();

        // 플레이어가 범위 내에 있고 F 키를 눌렀을 때 상호작용
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            Interact();
        }
    }

    // 플레이어와의 거리 체크
    private void CheckPlayerDistance()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;

        // 범위 진입/이탈 시 프롬프트 표시/숨김
        if (playerInRange != wasInRange)
        {
            if (playerInRange)
            {
                ShowInteractionPrompt();
            }
            else
            {
                HideInteractionPrompt();
            }
        }
    }

    private void Interact()
    {
        if (interactionManager == null)
        {
            interactionManager = FindObjectOfType<VillageInteractionManager>();
            if (interactionManager == null)
            {
                Debug.LogError("VillageInteractionManager를 찾을 수 없습니다.");
                return;
            }
        }

        switch (npcType)
        {
            case NPCType.StatUpgrade:
                interactionManager.OpenStatUpgradeShop();
                // 프롬프트 숨기기 (UI 열릴 때)
                HideInteractionPrompt();
                break;
                // 다른 NPC 타입들에 대한 처리도 추가 가능
        }
    }

    private void ShowInteractionPrompt()
    {
        if (promptShown) return;
        // 프롬프트 텍스트 설정 (있는 경우)
        if (promptText != null)
        {
            promptText.text = $"{interactionPrompt}";
        }
        // UI에 상호작용 안내 표시
        if (promptUI != null)
        {
            promptUI.SetActive(true);
        }

        Debug.Log($"{interactionPrompt} ({npcName})");
        promptShown = true;
    }

    private void HideInteractionPrompt()
    {
        if (!promptShown) return;

        // UI에서 상호작용 안내 숨기기
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }

        promptShown = false;
    }

    // 에디터에서 상호작용 범위 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}