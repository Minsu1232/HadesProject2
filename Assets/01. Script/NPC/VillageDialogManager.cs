using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageDialogManager : MonoBehaviour
{
    [Header("다이얼로그 ID 설정")]
    [SerializeField] private string chronofactuerUnlockDialogID = "chronofactuer_unlock_dialog";
    [SerializeField] private string statsUpgradeUnlockDialogID = "stats_upgrade_unlock_dialog";
    

    [Header("사망 다이얼로그 설정")]
    [SerializeField]
    private string[] deathDialogIDs = {
        "first_death_tutorial",  // 1회 사망
        "second_death",          // 2회 사망        
        "many_deaths"            // 4회 이상 사망
    };
    [SerializeField] private float deathDialogDelay = 0.5f; // 사망 다이얼로그 표시 지연 시간

    [Header("일반 설정")]
    [SerializeField] private float dialogDisplayDelay = 0.5f; // 일반 다이얼로그 표시 지연 시간

    // 해금된 컨텐츠 상태 변수
    private bool isChronofactuerUnlocked = false;
    private bool isStatsUpgradeNPCUnlocked = false;
  

    // 4회 이상 사망 시 사용할 랜덤 대사 인덱스
    private List<int> manyDeathsLineIndices = new List<int>();

    private void Start()
    {
        // 다이얼로그 시스템 초기화 후 약간의 딜레이를 두고 다이얼로그 체크
        StartCoroutine(CheckAndShowDialogs());
    }

    // 해금된 컨텐츠 정보 설정 (VillageManager에서 호출)
    public void SetUnlockedContent(bool chronofactuerUnlocked, bool statsUpgradeUnlocked)
    {
        this.isChronofactuerUnlocked = chronofactuerUnlocked;
        this.isStatsUpgradeNPCUnlocked = statsUpgradeUnlocked;
        
    }

    // 다이얼로그 표시 지연 코루틴
    private IEnumerator CheckAndShowDialogs()
    {
        // 다이얼로그 시스템 초기화까지 기다림
        yield return new WaitForSeconds(dialogDisplayDelay);

        // 컨텐츠 해금에 따른 설명 다이얼로그 표시
        ShowUnlockContentDialogs();
    }

    // 컨텐츠 해금 다이얼로그 표시 메서드
    private void ShowUnlockContentDialogs()
    {
        if (DialogSystem.Instance == null || GameProgressManager.Instance == null) return;

        // 1. 크로노팩처 무기 해금 다이얼로그
        if (isChronofactuerUnlocked)
        {
            bool weaponDialogShown = GameProgressManager.Instance.IsDialogShown(chronofactuerUnlockDialogID);
            if (!weaponDialogShown)
            {
                // 무기 해금 다이얼로그 표시
                DialogSystem.Instance.StartDialog(chronofactuerUnlockDialogID);
                GameProgressManager.Instance.MarkDialogAsShown(chronofactuerUnlockDialogID);
                return; // 한 번에 하나의 다이얼로그만 표시
            }
        }

        // 2. 스탯 업그레이드 NPC 해금 다이얼로그
        if (isStatsUpgradeNPCUnlocked)
        {
            bool npcDialogShown = GameProgressManager.Instance.IsDialogShown(statsUpgradeUnlockDialogID);
            if (!npcDialogShown)
            {
                UIManager.Instance.ShowNotification("기억 회복 해금");
                // 스탯 NPC 해금 다이얼로그 표시
                DialogSystem.Instance.StartDialog(statsUpgradeUnlockDialogID);
                GameProgressManager.Instance.MarkDialogAsShown(statsUpgradeUnlockDialogID);
                return;
            }
        }


    }

    // 사망 다이얼로그 표시 (VillageManager에서 호출)
    public void ShowDeathDialog(int deathCount)
    {
        StartCoroutine(ShowDeathDialogAfterDelay(deathCount));
    }

    // 죽음 다이얼로그 표시 코루틴
    private IEnumerator ShowDeathDialogAfterDelay(int deathCount)
    {
        // 마을 진입 후 잠시 기다린 후 다이얼로그 표시
        yield return new WaitForSeconds(deathDialogDelay);

        if (DialogSystem.Instance != null)
        {
            int dialogIndex = Mathf.Min(deathCount - 1, deathDialogIDs.Length - 1);
            if (dialogIndex < 0) dialogIndex = 0;

            // 4회 이상 사망인 경우 랜덤 대사 선택
            if (dialogIndex == deathDialogIDs.Length - 1)
            {
                ShowRandomManyDeathsDialog();
            }
            else
            {
                // 일반적인 다이얼로그 표시
                DialogSystem.Instance.StartDialog(deathDialogIDs[dialogIndex]);
            }
        }
    }

    // 4회 이상 사망 시 랜덤 대사 표시
    private void ShowRandomManyDeathsDialog()
    {
        // DialogSystem에서 many_deaths 다이얼로그 시퀀스 가져오기
        DialogSystem.DialogSequence manyDeathsSequence = null;

        // 다이얼로그 시스템의 딕셔너리에서 직접 접근
        if (DialogSystem.Instance != null && DialogSystem.Instance.TryGetDialogSequence("many_deaths", out manyDeathsSequence))
        {
            // 원본 대화 라인
            DialogSystem.DialogLine[] originalLines = manyDeathsSequence.lines;

            if (originalLines.Length > 0)
            {
                // 항상 하나의 라인만 선택
                int lineCount = 1;

                // 새 다이얼로그 시퀀스 생성
                DialogSystem.DialogSequence customSequence = new DialogSystem.DialogSequence
                {
                    dialogID = "random_death_dialog",
                    dialogMode = manyDeathsSequence.dialogMode,
                    lines = new DialogSystem.DialogLine[lineCount]
                };

                // 사용 가능한 라인 인덱스 목록 생성/갱신
                if (manyDeathsLineIndices.Count == 0)
                {
                    for (int i = 0; i < originalLines.Length; i++)
                    {
                        manyDeathsLineIndices.Add(i);
                    }
                }

                // 랜덤 인덱스 선택
                int randomIndex = Random.Range(0, manyDeathsLineIndices.Count);
                int selectedLineIndex = manyDeathsLineIndices[randomIndex];

                // 선택한 인덱스 제거 (중복 방지)
                manyDeathsLineIndices.RemoveAt(randomIndex);

                // 선택한 라인 복사
                customSequence.lines[0] = originalLines[selectedLineIndex];

                // 선택된 라인에 이벤트 추가 (있었다면 유지, 없었다면 추가)
                if (string.IsNullOrEmpty(customSequence.lines[0].eventTrigger))
                {
                    customSequence.lines[0].eventTrigger = "StartRevival";
                }

                // 커스텀 다이얼로그 등록 및 시작
                DialogSystem.Instance.RegisterCustomDialog(customSequence);
                DialogSystem.Instance.StartDialog(customSequence.dialogID);
            }
            else
            {
                // 라인이 없으면 기본 다이얼로그 사용
                DialogSystem.Instance.StartDialog("many_deaths");
            }
        }
        else
        {
            // 시퀀스를 찾을 수 없으면 기본 다이얼로그 사용
            DialogSystem.Instance.StartDialog("many_deaths");
        }
    }
}