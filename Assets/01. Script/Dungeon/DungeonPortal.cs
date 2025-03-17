using UnityEngine;

public class DungeonPortal : MonoBehaviour
{
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private GameObject interactionPrompt;

    private bool playerInRange = false;

    private void Start()
    {
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (selectionPanel != null) selectionPanel.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            ToggleSelectionPanel();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
            if (selectionPanel != null && selectionPanel.activeSelf) selectionPanel.SetActive(false);
        }
    }

    private void ToggleSelectionPanel()
    {
        if (selectionPanel != null) selectionPanel.SetActive(!selectionPanel.activeSelf);
    }
}