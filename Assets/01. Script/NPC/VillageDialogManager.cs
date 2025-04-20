using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageDialogManager : MonoBehaviour
{
    [Header("���̾�α� ID ����")]
    [SerializeField] private string chronofactuerUnlockDialogID = "chronofactuer_unlock_dialog";
    [SerializeField] private string statsUpgradeUnlockDialogID = "stats_upgrade_unlock_dialog";
    

    [Header("��� ���̾�α� ����")]
    [SerializeField]
    private string[] deathDialogIDs = {
        "first_death_tutorial",  // 1ȸ ���
        "second_death",          // 2ȸ ���        
        "many_deaths"            // 4ȸ �̻� ���
    };
    [SerializeField] private float deathDialogDelay = 0.5f; // ��� ���̾�α� ǥ�� ���� �ð�

    [Header("�Ϲ� ����")]
    [SerializeField] private float dialogDisplayDelay = 0.5f; // �Ϲ� ���̾�α� ǥ�� ���� �ð�

    // �رݵ� ������ ���� ����
    private bool isChronofactuerUnlocked = false;
    private bool isStatsUpgradeNPCUnlocked = false;
  

    // 4ȸ �̻� ��� �� ����� ���� ��� �ε���
    private List<int> manyDeathsLineIndices = new List<int>();

    private void Start()
    {
        // ���̾�α� �ý��� �ʱ�ȭ �� �ణ�� �����̸� �ΰ� ���̾�α� üũ
        StartCoroutine(CheckAndShowDialogs());
    }

    // �رݵ� ������ ���� ���� (VillageManager���� ȣ��)
    public void SetUnlockedContent(bool chronofactuerUnlocked, bool statsUpgradeUnlocked)
    {
        this.isChronofactuerUnlocked = chronofactuerUnlocked;
        this.isStatsUpgradeNPCUnlocked = statsUpgradeUnlocked;
        
    }

    // ���̾�α� ǥ�� ���� �ڷ�ƾ
    private IEnumerator CheckAndShowDialogs()
    {
        // ���̾�α� �ý��� �ʱ�ȭ���� ��ٸ�
        yield return new WaitForSeconds(dialogDisplayDelay);

        // ������ �رݿ� ���� ���� ���̾�α� ǥ��
        ShowUnlockContentDialogs();
    }

    // ������ �ر� ���̾�α� ǥ�� �޼���
    private void ShowUnlockContentDialogs()
    {
        if (DialogSystem.Instance == null || GameProgressManager.Instance == null) return;

        // 1. ũ�γ���ó ���� �ر� ���̾�α�
        if (isChronofactuerUnlocked)
        {
            bool weaponDialogShown = GameProgressManager.Instance.IsDialogShown(chronofactuerUnlockDialogID);
            if (!weaponDialogShown)
            {
                // ���� �ر� ���̾�α� ǥ��
                DialogSystem.Instance.StartDialog(chronofactuerUnlockDialogID);
                GameProgressManager.Instance.MarkDialogAsShown(chronofactuerUnlockDialogID);
                return; // �� ���� �ϳ��� ���̾�α׸� ǥ��
            }
        }

        // 2. ���� ���׷��̵� NPC �ر� ���̾�α�
        if (isStatsUpgradeNPCUnlocked)
        {
            bool npcDialogShown = GameProgressManager.Instance.IsDialogShown(statsUpgradeUnlockDialogID);
            if (!npcDialogShown)
            {
                UIManager.Instance.ShowNotification("��� ȸ�� �ر�");
                // ���� NPC �ر� ���̾�α� ǥ��
                DialogSystem.Instance.StartDialog(statsUpgradeUnlockDialogID);
                GameProgressManager.Instance.MarkDialogAsShown(statsUpgradeUnlockDialogID);
                return;
            }
        }


    }

    // ��� ���̾�α� ǥ�� (VillageManager���� ȣ��)
    public void ShowDeathDialog(int deathCount)
    {
        StartCoroutine(ShowDeathDialogAfterDelay(deathCount));
    }

    // ���� ���̾�α� ǥ�� �ڷ�ƾ
    private IEnumerator ShowDeathDialogAfterDelay(int deathCount)
    {
        // ���� ���� �� ��� ��ٸ� �� ���̾�α� ǥ��
        yield return new WaitForSeconds(deathDialogDelay);

        if (DialogSystem.Instance != null)
        {
            int dialogIndex = Mathf.Min(deathCount - 1, deathDialogIDs.Length - 1);
            if (dialogIndex < 0) dialogIndex = 0;

            // 4ȸ �̻� ����� ��� ���� ��� ����
            if (dialogIndex == deathDialogIDs.Length - 1)
            {
                ShowRandomManyDeathsDialog();
            }
            else
            {
                // �Ϲ����� ���̾�α� ǥ��
                DialogSystem.Instance.StartDialog(deathDialogIDs[dialogIndex]);
            }
        }
    }

    // 4ȸ �̻� ��� �� ���� ��� ǥ��
    private void ShowRandomManyDeathsDialog()
    {
        // DialogSystem���� many_deaths ���̾�α� ������ ��������
        DialogSystem.DialogSequence manyDeathsSequence = null;

        // ���̾�α� �ý����� ��ųʸ����� ���� ����
        if (DialogSystem.Instance != null && DialogSystem.Instance.TryGetDialogSequence("many_deaths", out manyDeathsSequence))
        {
            // ���� ��ȭ ����
            DialogSystem.DialogLine[] originalLines = manyDeathsSequence.lines;

            if (originalLines.Length > 0)
            {
                // �׻� �ϳ��� ���θ� ����
                int lineCount = 1;

                // �� ���̾�α� ������ ����
                DialogSystem.DialogSequence customSequence = new DialogSystem.DialogSequence
                {
                    dialogID = "random_death_dialog",
                    dialogMode = manyDeathsSequence.dialogMode,
                    lines = new DialogSystem.DialogLine[lineCount]
                };

                // ��� ������ ���� �ε��� ��� ����/����
                if (manyDeathsLineIndices.Count == 0)
                {
                    for (int i = 0; i < originalLines.Length; i++)
                    {
                        manyDeathsLineIndices.Add(i);
                    }
                }

                // ���� �ε��� ����
                int randomIndex = Random.Range(0, manyDeathsLineIndices.Count);
                int selectedLineIndex = manyDeathsLineIndices[randomIndex];

                // ������ �ε��� ���� (�ߺ� ����)
                manyDeathsLineIndices.RemoveAt(randomIndex);

                // ������ ���� ����
                customSequence.lines[0] = originalLines[selectedLineIndex];

                // ���õ� ���ο� �̺�Ʈ �߰� (�־��ٸ� ����, �����ٸ� �߰�)
                if (string.IsNullOrEmpty(customSequence.lines[0].eventTrigger))
                {
                    customSequence.lines[0].eventTrigger = "StartRevival";
                }

                // Ŀ���� ���̾�α� ��� �� ����
                DialogSystem.Instance.RegisterCustomDialog(customSequence);
                DialogSystem.Instance.StartDialog(customSequence.dialogID);
            }
            else
            {
                // ������ ������ �⺻ ���̾�α� ���
                DialogSystem.Instance.StartDialog("many_deaths");
            }
        }
        else
        {
            // �������� ã�� �� ������ �⺻ ���̾�α� ���
            DialogSystem.Instance.StartDialog("many_deaths");
        }
    }
}