//// 페이즈 관리 중앙화
//using static IMonsterState;
//using System;
//using UnityEngine;
//using System.Collections;

//public class BossPhaseManager
//{
//    private BossAI owner;
//    private BossData bossData;
//    private PhaseData currentPhaseData;

//    public int CurrentPhase { get; private set; } = 1;
//    public bool IsInTransition { get; private set; }

//    public event Action<int> OnPhaseChanged;
//    public event Action OnPhaseTransitionStart;
//    public event Action OnPhaseTransitionEnd;

//    public BossPhaseManager(BossAI owner, BossData data)
//    {
//        this.owner = owner;
//        this.bossData = data;
//        currentPhaseData = data.phaseData[0];
//    }

//    public void CheckPhaseTransition()
//    {
//        if (IsInTransition) return;

//        float healthRatio = (float)owner.GetStatus().GetMonsterClass().CurrentHealth /
//                           owner.GetStatus().GetMonsterClass().MaxHealth;

//        if (CurrentPhase < bossData.phaseData.Count &&
//            healthRatio <= currentPhaseData.phaseTransitionThreshold)
//        {
//            StartPhaseTransition();
//        }
//    }

//    private void StartPhaseTransition()
//    {
//        IsInTransition = true;
//        OnPhaseTransitionStart?.Invoke();
//        owner.ChangeState(MonsterStateType.PhaseTransition);

//        // 전환 시간 후 완료 처리
//        owner.StartCoroutine(CompleteTransitionAfterDelay());
//    }

//    private IEnumerator CompleteTransitionAfterDelay()
//    {
//        yield return new WaitForSeconds(currentPhaseData.transitionDuration);

//        CurrentPhase++;
//        currentPhaseData = bossData.phaseData[CurrentPhase - 1];
//        currentPhaseData.isInvulnerableDuringTransition = false;
//        IsInTransition = false;
//        OnPhaseChanged?.Invoke(CurrentPhase);
//        OnPhaseTransitionEnd?.Invoke();
//    }

//    public PhaseData GetCurrentPhaseData() => currentPhaseData;
//}