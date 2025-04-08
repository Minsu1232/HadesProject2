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
            Debug.LogError("GameProgressManager�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        foreach (DialogContext context in possibleDialogs)
        {
            bool conditionsMet = true;
            // �̹� ǥ�õ� ���̾�α����� �α׷� Ȯ��
            bool isAlreadyShown = GameProgressManager.Instance.IsDialogShown(context.dialogID);
            Debug.Log($"���̾�α� {context.dialogID} ���� Ȯ��: �̹� ǥ�õ� = {isAlreadyShown}");
            // �÷��� ����
            if (!string.IsNullOrEmpty(context.requiredFlag))
            {
                bool flagValue = GameProgressManager.Instance.GetFlag(context.requiredFlag);
                if (flagValue != context.requireFlagValue)
                {
                    conditionsMet = false;
                    continue;
                }
            }
          
            // é�� ����
            int currentChapter = SaveManager.Instance?.GetPlayerData()?.currentChapter ?? 1;
            if (context.minChapter > 0 && currentChapter < context.minChapter)
            {
                conditionsMet = false;
                continue;
            }
            // �̹� ǥ�õ� ���̾�α״� �ǳʶٱ� (consumeOnShow�� true�� ���)
            if (context.consumeOnShow && GameProgressManager.Instance.IsDialogShown(context.dialogID))
            {
                Debug.Log($"���̾�α� {context.dialogID}�� �̹� ǥ�õǾ����Ƿ� �ǳʶݴϴ�.");
                conditionsMet = false;
                continue;
            }
            if (context.maxChapter > 0 && currentChapter > context.maxChapter)
            {
                conditionsMet = false;
                continue;
            }

            // �湮 Ƚ�� ����
            if (context.specificVisitCount > 0 &&
                GameProgressManager.Instance.GetLocationVisitCount(locationID) != context.specificVisitCount)
            {
                conditionsMet = false;
                continue;
            }

            // �̹� ǥ�õ� ���̾�α����� Ȯ��
            if (context.consumeOnShow &&
                GameProgressManager.Instance.IsDialogShown(context.dialogID))
            {
                conditionsMet = false;
                continue;
            }

            // ��� ���� ����, ���̾�α� ǥ��
            if (conditionsMet)
            {
                if (DialogSystem.Instance != null)
                {
                    DialogSystem.Instance.StartDialog(context.dialogID);

                    if (context.consumeOnShow)
                        GameProgressManager.Instance.MarkDialogAsShown(context.dialogID);
                }

                break; // ù ��° ��Ī�� ���̾�α׸� ǥ��
            }
        }
    }

    public void TriggerDialogManually()
    {
        CheckAndShowDialog();
    }
}