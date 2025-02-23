using UnityEngine;
using UnityEngine.UI;

public class SoulStone : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionTime = 2f;
    [SerializeField] private float purifyAmount = 20f;
    [SerializeField] private KeyCode interactionKey = KeyCode.F;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject purifyEffect;
    [SerializeField] private Image isInRange;
    [SerializeField] private Image fillBarBackground;  // FillBar ��� �̹���
    [SerializeField] private Image interactionFill;    // ���� ä������ ����� �̹���

    [Header("UI Colors")]
    [SerializeField] private Color inRangeColor = Color.green;
    [SerializeField] private Color outOfRangeColor = Color.white;
    [SerializeField] private float colorTransitionSpeed = 5f;

    private bool isInteracting = false;
    private float currentInteractionTime = 0f;
    private IBossEssenceSystem essenceSystem;

    public void InitializeWithBoss(AlexanderBoss boss)
    {
        essenceSystem = boss.GetEssenceSystem();
    }

    private void Update()
    {
        bool playerInRange = IsPlayerInRange();
        UpdateInRangeIndicator(playerInRange);

        if (playerInRange)
        {
            if (Input.GetKey(interactionKey))
            {
                // FŰ�� ���� ���� FillBar ��� Ȱ��ȭ
                if (fillBarBackground != null && !fillBarBackground.gameObject.activeSelf)
                {
                    fillBarBackground.gameObject.SetActive(true);
                }
                HandleInteraction();
            }
            else
            {
                // FŰ�� ���� FillBar ��� ��Ȱ��ȭ �� ����� �ʱ�ȭ
                if (fillBarBackground != null && fillBarBackground.gameObject.activeSelf)
                {
                    fillBarBackground.gameObject.SetActive(false);
                }
                ResetInteraction();
            }
        }
        else
        {
            // �÷��̾ ������ ����� ��� ��Ȱ��ȭ
            if (fillBarBackground != null && fillBarBackground.gameObject.activeSelf)
            {
                fillBarBackground.gameObject.SetActive(false);
            }
        }

        UpdateInteractionFill();
    }

    private void UpdateInRangeIndicator(bool playerInRange)
    {
        if (isInRange != null)
        {
            Color targetColor = playerInRange ? inRangeColor : outOfRangeColor;
            isInRange.color = Color.Lerp(isInRange.color, targetColor, Time.deltaTime * colorTransitionSpeed);
        }
    }

    private void UpdateInteractionFill()
    {
        if (interactionFill != null)
        {
            float fillValue = isInteracting ? currentInteractionTime / interactionTime : 0f;
            interactionFill.fillAmount = Mathf.Clamp01(fillValue);
        }
    }

    private void HandleInteraction()
    {
        if (!isInteracting)
        {
            isInteracting = true;
            currentInteractionTime = 0f;
        }

        currentInteractionTime += Time.deltaTime;
        if (currentInteractionTime >= interactionTime)
        {
            PurifySoul();
            ResetInteraction();
        }
    }

    private void PurifySoul()
    {
        if (essenceSystem != null)
        {
            float currentEssence = essenceSystem.CurrentEssence;
            float decreaseAmount = Mathf.Min(currentEssence, purifyAmount);
            essenceSystem.DecreaseEssence(decreaseAmount);

            if (purifyEffect != null)
            {
                Instantiate(purifyEffect, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }

    private void ResetInteraction()
    {
        isInteracting = false;
        currentInteractionTime = 0f;
    }

    private bool IsPlayerInRange()
    {
        var player = GameInitializer.Instance.GetPlayerClass();
        float distance = Vector3.Distance(transform.position, player.playerTransform.position);
        return distance <= 3f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 3f);
    }
}
