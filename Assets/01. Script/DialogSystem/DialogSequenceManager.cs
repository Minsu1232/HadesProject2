using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogSequenceManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogSequenceItem
    {
        public string dialogID;
        public GameObject nextTriggerToActivate;
        public float activationDelay = 0.5f;
        public bool deactivateCurrentTrigger = true;
    }

    [SerializeField] private List<DialogSequenceItem> dialogSequences = new List<DialogSequenceItem>();

    // ���� Ȱ��ȭ�� ��ȭ �������� �����ϱ� ���� ��ųʸ�
    private Dictionary<string, DialogSequenceItem> sequenceMap = new Dictionary<string, DialogSequenceItem>();

    private void Start()
    {
        // ���̾�α� �̺�Ʈ ����
        if (DialogSystem.Instance != null)
        {
            DialogSystem.OnDialogEvent += HandleDialogEvent;
        }

        // ������ �� �ʱ�ȭ
        foreach (var sequence in dialogSequences)
        {
            sequenceMap[sequence.dialogID] = sequence;

            // ���� Ʈ���Ŵ� ó������ ��Ȱ��ȭ
            if (sequence.nextTriggerToActivate != null)
            {
                sequence.nextTriggerToActivate.SetActive(false);
            }
        }

        // �̹� ǥ�õ� ���̾�α��� ���� Ʈ���� Ȱ��ȭ (���� �ε� ��)
        CheckAlreadyShownDialogs();
    }

    private void OnDestroy()
    {
        if (DialogSystem.Instance != null)
        {
            DialogSystem.OnDialogEvent -= HandleDialogEvent;
        }
    }

    // �̹� ǥ�õ� ���̾�α� Ȯ�� �� ó��
    private void CheckAlreadyShownDialogs()
    {
        if (GameProgressManager.Instance == null) return;

        // ���������� ǥ�õ� ���̾�α� ã��
        string lastDialogID = null;

        foreach (var sequence in dialogSequences)
        {
            if (GameProgressManager.Instance.IsDialogShown(sequence.dialogID))
            {
                lastDialogID = sequence.dialogID;
            }
        }

        // ������ ǥ�õ� ���̾�α��� ���� Ʈ���� Ȱ��ȭ
        if (lastDialogID != null && sequenceMap.ContainsKey(lastDialogID))
        {
            var sequence = sequenceMap[lastDialogID];
            if (sequence.nextTriggerToActivate != null)
            {
                sequence.nextTriggerToActivate.SetActive(true);
                Debug.Log($"������ ���̾�α� '{lastDialogID}' ���� Ʈ���� Ȱ��ȭ");
            }
        }
    }

    // ���̾�α� �̺�Ʈ ó��
    private void HandleDialogEvent(string eventName)
    {
        // ���̾�α� �Ϸ� �̺�Ʈ Ȯ��
        if (eventName.StartsWith("DialogComplete:"))
        {
            string dialogID = eventName.Split(':')[1];
            ActivateNextTrigger(dialogID);
        }
    }

    // ���� Ʈ���� Ȱ��ȭ
    private void ActivateNextTrigger(string completedDialogID)
    {
        if (!sequenceMap.ContainsKey(completedDialogID)) return;

        var sequence = sequenceMap[completedDialogID];

        // ���� Ʈ���� ��Ȱ��ȭ (�ʿ��� ���)
        if (sequence.deactivateCurrentTrigger)
        {
            ContextualDialogTrigger currentTrigger = FindTriggerForDialog(completedDialogID);
            if (currentTrigger != null)
            {
                currentTrigger.gameObject.SetActive(false);
                Debug.Log($"'{completedDialogID}' Ʈ���� ��Ȱ��ȭ");
            }
        }

        // ���� Ʈ���� Ȱ��ȭ (���� �ð� ��)
        if (sequence.nextTriggerToActivate != null)
        {
            StartCoroutine(DelayedActivation(sequence.nextTriggerToActivate, sequence.activationDelay));
        }
    }

    // ������ Ȱ��ȭ
    private IEnumerator DelayedActivation(GameObject trigger, float delay)
    {
        yield return new WaitForSeconds(delay);
        trigger.SetActive(true);
        Debug.Log($"���� Ʈ���� Ȱ��ȭ: {trigger.name}");
    }

    // dialogID�� �ش��ϴ� Ʈ���� ã��
    private ContextualDialogTrigger FindTriggerForDialog(string dialogID)
    {
        ContextualDialogTrigger[] allTriggers = FindObjectsOfType<ContextualDialogTrigger>();

        foreach (var trigger in allTriggers)
        {
            foreach (var context in trigger.possibleDialogs)
            {
                if (context.dialogID == dialogID)
                {
                    return trigger;
                }
            }
        }

        return null;
    }
}