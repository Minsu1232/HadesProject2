using UnityEngine;
using TMPro;

public class InteractableWeapon : MonoBehaviour
{
    [Header("무기 설정")]
    [SerializeField] private string weaponName = "GreatSword"; // 장착할 무기 이름 (WeaponFactory에 등록된 이름)
    [SerializeField] private string interactionPrompt = "F키를 눌러 무기 장착하기";
    [SerializeField] private float interactionRange = 3f; // 상호작용 범위

    [Header("UI 요소")]
    [SerializeField] private GameObject promptUI; // 프롬프트 UI 게임 오브젝트
    [SerializeField] private TextMeshProUGUI promptText; // 프롬프트 텍스트

    private Transform playerTransform;
    private bool playerInRange = false;
    private bool promptShown = false;
    private WeaponService weaponService;

    // 무기 장착 이벤트
    public event System.Action OnWeaponEquipped;

    private void Start()
    {
        // 플레이어 찾기
        playerTransform = GameInitializer.Instance.GetPlayerClass().playerTransform;
        if (playerTransform == null)
        {
            Debug.LogError("플레이어를 찾을 수 없습니다.");
        }

        // WeaponService 참조 가져오기
        weaponService = GameInitializer.Instance.GetWeaponService();
        if (weaponService == null)
        {
            Debug.LogError("WeaponService를 찾을 수 없습니다.");
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
            EquipWeapon();
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

    private async void EquipWeapon()
    {
        if (weaponService == null) return;

        // 무기 장착 시도
        bool success = await weaponService.EquipWeapon(weaponName);

        if (success)
        {
            Debug.Log($"{weaponName} 장착 성공!");

            // 이벤트 발생
            OnWeaponEquipped?.Invoke();
        }
        else
        {
            Debug.LogError($"{weaponName} 장착 실패!");
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

        Debug.Log($"{interactionPrompt} ({weaponName})");
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
        Gizmos.color = Color.blue; // 무기는 파란색으로 표시
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}