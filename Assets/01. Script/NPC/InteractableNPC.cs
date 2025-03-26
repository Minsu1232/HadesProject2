// InteractableNPC.cs - ��ȣ�ۿ� ������ NPC
using UnityEngine;

public class InteractableNPC : MonoBehaviour
{
    [SerializeField] private string npcName = "��������";
    [SerializeField] private string interactionPrompt = "FŰ�� ���� ��ȭ�ϱ�";

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
                Debug.LogError("VillageInteractionManager�� ã�� �� �����ϴ�.");
                return;
            }
        }

        switch (npcType)
        {
            case NPCType.StatUpgrade:
                interactionManager.OpenStatUpgradeShop();
                break;

                // �ٸ� NPC Ÿ�Ե鿡 ���� ó���� �߰� ����
        }
    }

    private void ShowInteractionPrompt()
    {
        if (promptShown) return;

        // TODO: UI�� ��ȣ�ۿ� �ȳ� ǥ��
        Debug.Log($"{interactionPrompt} ({npcName})");
        promptShown = true;
    }

    private void HideInteractionPrompt()
    {
        if (!promptShown) return;

        // TODO: UI���� ��ȣ�ۿ� �ȳ� �����
        promptShown = false;
    }
}