// WeaponTutorialManager.cs
using UnityEngine;
using UnityEngine.UI;

public class WeaponTutorialManager : MonoBehaviour
{
    [SerializeField] private string nextDialogID = "combat_tutorial";
    [SerializeField] private InteractableWeapon weaponInteractable;
    [SerializeField] private GameObject trainingDummy;
    [SerializeField] private float dummyHighlightDelay = 2f;

    private bool weaponEquipped = false;
    private bool tutorialComplete = false;

    private void Start()
    {
        // DialogSystem에서 이벤트 리스닝
        DialogSystem.OnDialogEvent += HandleDialogEvent;

        // 무기 장착 이벤트 리스너 등록
        if (weaponInteractable != null)
        {
            weaponInteractable.OnWeaponEquipped += OnWeaponEquipped;
        }

        // 처음에는 더미 하이라이트 비활성화
        if (trainingDummy != null)
        {
            SetDummyHighlight(false);
        }
    }

    private void OnDestroy()
    {
        DialogSystem.OnDialogEvent -= HandleDialogEvent;

        if (weaponInteractable != null)
        {
            weaponInteractable.OnWeaponEquipped -= OnWeaponEquipped;
        }
    }

    // 다이얼로그 이벤트 처리
    private void HandleDialogEvent(string eventName)
    {
        if (eventName == "EnableMovement")
        {
            // 마지막 weapon_tutorial 대화 이후, 더미 하이라이트 활성화
            Invoke("HighlightDummy", dummyHighlightDelay);
        }
    }

    // 무기 장착 이벤트 처리
    private void OnWeaponEquipped()
    {
        Debug.Log("무기가 장착되었습니다!");
        weaponEquipped = true;

        // 무기 장착 확인
        CheckTutorialProgress();
    }

    // 더미 하이라이트 표시
    private void HighlightDummy()
    {
        if (trainingDummy != null && weaponEquipped)
        {
            SetDummyHighlight(true);
        }
    }

    // 더미 하이라이트 효과 설정
    private void SetDummyHighlight(bool active)
    {
        // 하이라이트 효과 (Outline 컴포넌트 등)를 활성화/비활성화
        Outline outlineComponent = trainingDummy.GetComponent<Outline>();
        if (outlineComponent != null)
        {
            outlineComponent.enabled = active;
        }

        // 또는 별도의 하이라이트 오브젝트 활성화/비활성화
        Transform highlightObj = trainingDummy.transform.Find("Highlight");
        if (highlightObj != null)
        {
            highlightObj.gameObject.SetActive(active);
        }
    }

    // 플레이어가 더미에 접근했는지 체크
    private void Update()
    {
        if (tutorialComplete || !weaponEquipped) return;

        // 플레이어가 더미에 충분히 가까이 왔는지 확인
        if (trainingDummy != null && Vector3.Distance(GameInitializer.Instance.GetPlayerClass().playerTransform.position, trainingDummy.transform.position) < 3f)
        {
            // 전투 튜토리얼 시작
            tutorialComplete = true;
            StartCombatTutorial();
        }
    }

    // 전투 튜토리얼 시작
    private void StartCombatTutorial()
    {
        if (DialogSystem.Instance != null)
        {
            DialogSystem.Instance.StartDialog(nextDialogID);
        }
    }

    // 튜토리얼 진행 상태 체크
    private void CheckTutorialProgress()
    {
        if (weaponEquipped && !tutorialComplete)
        {
            // 무기 장착 후 더미 하이라이트
            HighlightDummy();
        }
    }
}

