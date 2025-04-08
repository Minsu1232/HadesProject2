using System.Collections.Generic;
using UnityEngine;

public class ContextualDialogTrigger : MonoBehaviour
{
    [System.Serializable]
    public class DialogContext
    {
        public string dialogID;
        public string requiredFlag = "";
        public bool requireFlagValue = true;
        public int minChapter, maxChapter;
        public int specificVisitCount;
        public bool consumeOnShow = true;
        public string requiredFragment = "";
        public string requiredWeapon = "";
    }

    [SerializeField] public List<DialogContext> possibleDialogs = new List<DialogContext>();
    [SerializeField] private string locationID = "";
    [SerializeField] private bool triggerOnStart = false;
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private float startDelay = 0.5f;

    private bool hasTriggered = false;

    private void Start()
    {
        if (triggerOnStart)
        {
            Invoke("CheckAndShowDialog", startDelay);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(locationID) && GameProgressManager.Instance != null)
            {
                int visits = GameProgressManager.Instance.GetLocationVisitCount(locationID) + 1;
                GameProgressManager.Instance.SetLocationVisitCount(locationID, visits);
            }

            if (!triggerOnce || !hasTriggered)
            {
                CheckAndShowDialog();
                hasTriggered = true;
            }
        }
    }

    public void CheckAndShowDialog()
    {
        if (GameProgressManager.Instance == null)
        {
            Debug.LogError("GameProgressManager가 초기화되지 않았습니다.");
            return;
        }

        foreach (DialogContext context in possibleDialogs)
        {
            bool conditionsMet = true;
            // 이미 표시된 다이얼로그인지 로그로 확인
            bool isAlreadyShown = GameProgressManager.Instance.IsDialogShown(context.dialogID);
            Debug.Log($"다이얼로그 {context.dialogID} 상태 확인: 이미 표시됨 = {isAlreadyShown}");
            // 플래그 조건
            if (!string.IsNullOrEmpty(context.requiredFlag))
            {
                bool flagValue = GameProgressManager.Instance.GetFlag(context.requiredFlag);
                if (flagValue != context.requireFlagValue)
                {
                    conditionsMet = false;
                    continue;
                }
            }
          
            // 챕터 조건
            int currentChapter = SaveManager.Instance?.GetPlayerData()?.currentChapter ?? 1;
            if (context.minChapter > 0 && currentChapter < context.minChapter)
            {
                conditionsMet = false;
                continue;
            }
            // 이미 표시된 다이얼로그는 건너뛰기 (consumeOnShow가 true인 경우)
            if (context.consumeOnShow && GameProgressManager.Instance.IsDialogShown(context.dialogID))
            {
                Debug.Log($"다이얼로그 {context.dialogID}는 이미 표시되었으므로 건너뜁니다.");
                conditionsMet = false;
                continue;
            }
            if (context.maxChapter > 0 && currentChapter > context.maxChapter)
            {
                conditionsMet = false;
                continue;
            }

            // 방문 횟수 조건
            if (context.specificVisitCount > 0 &&
                GameProgressManager.Instance.GetLocationVisitCount(locationID) != context.specificVisitCount)
            {
                conditionsMet = false;
                continue;
            }

            // 이미 표시된 다이얼로그인지 확인
            if (context.consumeOnShow &&
                GameProgressManager.Instance.IsDialogShown(context.dialogID))
            {
                conditionsMet = false;
                continue;
            }

            // 모든 조건 충족, 다이얼로그 표시
            if (conditionsMet)
            {
                if (DialogSystem.Instance != null)
                {
                    DialogSystem.Instance.StartDialog(context.dialogID);

                    if (context.consumeOnShow)
                        GameProgressManager.Instance.MarkDialogAsShown(context.dialogID);
                }

                break; // 첫 번째 매칭된 다이얼로그만 표시
            }
        }
    }

    public void TriggerDialogManually()
    {
        CheckAndShowDialog();
    }
}