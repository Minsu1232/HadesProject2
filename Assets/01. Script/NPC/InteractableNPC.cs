// InteractableNPC.cs - 상호작용 가능한 NPC
using UnityEngine;

public class InteractableNPC : MonoBehaviour
{
    [SerializeField] private string npcName = "대장장이";
    [SerializeField] private string interactionPrompt = "F키를 눌러 대화하기";

    [SerializeField] private VillageInteractionManager interactionManager;
    [SerializeField] private NPCType npcType = NPCType.StatUpgrade;

    private bool playerInRange = false;
    private bool promptShown = false;

    public enum NPCType
    {
        StatUpgrade,
        Shop,
        Quest
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowInteractionPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideInteractionPrompt();
        }
    }

    private void Update()
    {
        if (/*playerInRange && */Input.GetKeyDown(KeyCode.F))
        {
            Interact();
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
                break;

                // 다른 NPC 타입들에 대한 처리도 추가 가능
        }
    }

    private void ShowInteractionPrompt()
    {
        if (promptShown) return;

        // TODO: UI에 상호작용 안내 표시
        Debug.Log($"{interactionPrompt} ({npcName})");
        promptShown = true;
    }

    private void HideInteractionPrompt()
    {
        if (!promptShown) return;

        // TODO: UI에서 상호작용 안내 숨기기
        promptShown = false;
    }
}