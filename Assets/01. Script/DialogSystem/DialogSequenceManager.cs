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

    // 현재 활성화된 대화 시퀀스를 추적하기 위한 딕셔너리
    private Dictionary<string, DialogSequenceItem> sequenceMap = new Dictionary<string, DialogSequenceItem>();

    private void Start()
    {
        // 다이얼로그 이벤트 구독
        if (DialogSystem.Instance != null)
        {
            DialogSystem.OnDialogEvent += HandleDialogEvent;
        }

        // 시퀀스 맵 초기화
        foreach (var sequence in dialogSequences)
        {
            sequenceMap[sequence.dialogID] = sequence;

            // 다음 트리거는 처음에는 비활성화
            if (sequence.nextTriggerToActivate != null)
            {
                sequence.nextTriggerToActivate.SetActive(false);
            }
        }

        // 이미 표시된 다이얼로그의 다음 트리거 활성화 (게임 로드 시)
        CheckAlreadyShownDialogs();
    }

    private void OnDestroy()
    {
        if (DialogSystem.Instance != null)
        {
            DialogSystem.OnDialogEvent -= HandleDialogEvent;
        }
    }

    // 이미 표시된 다이얼로그 확인 및 처리
    private void CheckAlreadyShownDialogs()
    {
        if (GameProgressManager.Instance == null) return;

        // 마지막으로 표시된 다이얼로그 찾기
        string lastDialogID = null;

        foreach (var sequence in dialogSequences)
        {
            if (GameProgressManager.Instance.IsDialogShown(sequence.dialogID))
            {
                lastDialogID = sequence.dialogID;
            }
        }

        // 마지막 표시된 다이얼로그의 다음 트리거 활성화
        if (lastDialogID != null && sequenceMap.ContainsKey(lastDialogID))
        {
            var sequence = sequenceMap[lastDialogID];
            if (sequence.nextTriggerToActivate != null)
            {
                sequence.nextTriggerToActivate.SetActive(true);
                Debug.Log($"마지막 다이얼로그 '{lastDialogID}' 다음 트리거 활성화");
            }
        }
    }

    // 다이얼로그 이벤트 처리
    private void HandleDialogEvent(string eventName)
    {
        // 다이얼로그 완료 이벤트 확인
        if (eventName.StartsWith("DialogComplete:"))
        {
            string dialogID = eventName.Split(':')[1];
            ActivateNextTrigger(dialogID);
        }
    }

    // 다음 트리거 활성화
    private void ActivateNextTrigger(string completedDialogID)
    {
        if (!sequenceMap.ContainsKey(completedDialogID)) return;

        var sequence = sequenceMap[completedDialogID];

        // 현재 트리거 비활성화 (필요한 경우)
        if (sequence.deactivateCurrentTrigger)
        {
            ContextualDialogTrigger currentTrigger = FindTriggerForDialog(completedDialogID);
            if (currentTrigger != null)
            {
                currentTrigger.gameObject.SetActive(false);
                Debug.Log($"'{completedDialogID}' 트리거 비활성화");
            }
        }

        // 다음 트리거 활성화 (지연 시간 후)
        if (sequence.nextTriggerToActivate != null)
        {
            StartCoroutine(DelayedActivation(sequence.nextTriggerToActivate, sequence.activationDelay));
        }
    }

    // 지연된 활성화
    private IEnumerator DelayedActivation(GameObject trigger, float delay)
    {
        yield return new WaitForSeconds(delay);
        trigger.SetActive(true);
        Debug.Log($"다음 트리거 활성화: {trigger.name}");
    }

    // dialogID에 해당하는 트리거 찾기
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