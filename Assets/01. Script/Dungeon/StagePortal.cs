using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StagePortal : MonoBehaviour
{
    [Header("UI ���")]
    [SerializeField] public GameObject interactionPrompt;
    [SerializeField] private Image interactionProgressBar;

    [Header("��Ż ����")]
    [SerializeField] private float interactionDuration = 1.5f;
    [SerializeField] private GameObject portalUseEffectPrefab;
    public string targetStageID;

    private bool playerInRange = false;
    private bool isInteracting = false;
    private Coroutine interactionCoroutine;

    private void Start()
    {
      
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (interactionProgressBar != null) interactionProgressBar.fillAmount = 0f;
    }

    private void Update()
    {
        if (playerInRange)
        {
            // FŰ ������ ������ ��ȣ�ۿ� ����
            if (Input.GetKey(KeyCode.F) && !isInteracting)
            {
                StartInteraction();
            }
            // FŰ�� ���� ��ȣ�ۿ� ���
            else if (Input.GetKeyUp(KeyCode.F) && isInteracting)
            {
                CancelInteraction();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
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
            if (isInteracting)
            {
                CancelInteraction();
            }
        }
    }

    // ��Ż ��ȣ�ۿ� ����
    private void StartInteraction()
    {
        isInteracting = true;
        interactionCoroutine = StartCoroutine(InteractionProgress());
    }

    // ��Ż ��ȣ�ۿ� ���
    private void CancelInteraction()
    {
        if (interactionCoroutine != null)
        {
            StopCoroutine(interactionCoroutine);
        }

        isInteracting = false;
        if (interactionProgressBar != null)
        {
            interactionProgressBar.fillAmount = 0f;
        }
    }

    // ��ȣ�ۿ� ���� �� ó��
    private IEnumerator InteractionProgress()
    {
        float interactionTimer = 0f;

        while (interactionTimer < interactionDuration)
        {
            interactionTimer += Time.deltaTime;

            // ���� �� ������Ʈ
            if (interactionProgressBar != null)
            {
                interactionProgressBar.fillAmount = interactionTimer / interactionDuration;
            }

            yield return null;
        }

        // ��ȣ�ۿ� �Ϸ�
        isInteracting = false;
        if (interactionProgressBar != null)
        {
            interactionProgressBar.fillAmount = 0f;
        }

        // ��Ż ��� ����Ʈ
        if (portalUseEffectPrefab != null)
        {
            Instantiate(portalUseEffectPrefab, transform.position, Quaternion.identity);
        }

        // ���� �Ŵ����� ���� �� ��ȯ
        UsePortal();
    }

    // ��Ż ���
    private void UsePortal()
    {
        if (string.IsNullOrEmpty(targetStageID))
        {
            Debug.LogError("��Ż�� Ÿ�� �������� ID�� �������� �ʾҽ��ϴ�.");
            return;
        }
        interactionPrompt.SetActive(false);
        // Ʈ������ ȿ��
        if (SceneTransitionManager.Instance != null)
        {
            // ���̵� �� (��ο���)
            SceneTransitionManager.Instance.FadeIn(() => {
                // ���̵� �Ϸ� �� ���� �������� �ε�
                DungeonManager.Instance.LoadStage(targetStageID);

                // �ε��� �Ϸ�Ǹ� ���̵� �ƿ� (�����)
                StartCoroutine(DelayedFadeOut());
            });
        }
        else
        {
            // Ʈ������ �Ŵ����� ���� ��� ���� �ε�
            DungeonManager.Instance.LoadStage(targetStageID);
        }
    }

    // ���̵� �ƿ� ���� ó��
    private IEnumerator DelayedFadeOut()
    {
        // �ణ�� ���� �� ���̵� �ƿ�
        yield return new WaitForSeconds(1f);

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.FadeOut();
        }
    }
}