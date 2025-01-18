//using Sirenix.OdinInspector;
//using System.Collections;
//using UnityEngine;
//using static AttackData;

//public class BossStatus : MonsterStatus
//{
//    [SerializeField] private BossUIManager bossUIManager;

//    [FoldoutGroup("Boss Stats"), ReadOnly]
//    [ShowInInspector]
//    public int CurrentPhase => (monsterClass as BossMonster)?.CurrentPhase ?? 0;

//    [FoldoutGroup("Boss Stats"), ReadOnly]
//    [ShowInInspector]
//    public bool IsInRageMode => (monsterClass as BossMonster)?.IsInRageMode ?? false;

//    private BossMonster bossMonster => monsterClass as BossMonster;

//    public override void Initialize(MonsterClass monster)
//    {
//        if (monster is BossMonster)
//        {
//            base.Initialize(monster);

//            // 보스 이벤트 구독
//            var boss = monster as BossMonster;
//            boss.OnPhaseChanged += HandlePhaseChanged;
//            boss.OnRageModeEntered += HandleRageModeEntered;
//            boss.OnRageModeEnded += HandleRageModeEnded;

//            // 보스 UI 초기화
//            if (bossUIManager != null)
//            {
//                bossUIManager.Initialize(boss);
//            }
//        }
//        else
//        {
//            Debug.LogError("Trying to initialize BossStatus with non-boss monster!");
//        }
//    }

//    private void Update()
//    {
//        if (bossMonster != null)
//        {
//            bossMonster.UpdateRageMode();
//        }
//    }

//    private void HandlePhaseChanged(int newPhase)
//    {
//        // UI 업데이트
//        bossUIManager?.UpdatePhase(newPhase);

//        // 페이즈 전환 이펙트
//        PlayPhaseTransitionEffects(newPhase);

//        // 카메라 효과
//        PlayPhaseCameraEffects(newPhase);
//    }

//    private void HandleRageModeEntered()
//    {
//        // UI 업데이트
//        bossUIManager?.SetRageModeUI(true);

//        // 레이지모드 이펙트
//        PlayRageModeEffects();
//    }

//    private void HandleRageModeEnded()
//    {
//        // UI 업데이트
//        bossUIManager?.SetRageModeUI(false);

//        // 이펙트 제거
//        CleanupRageModeEffects();
//    }

//    private void PlayPhaseTransitionEffects(int phase)
//    {
//        if (bossMonster?.GetMonsterData() is BossData bossData)
//        {
//            if (bossData.phaseTransitionEffects != null &&
//                phase < bossData.phaseTransitionEffects.Length)
//            {
//                var effectPrefab = bossData.phaseTransitionEffects[phase];
//                if (effectPrefab != null)
//                {
//                    Instantiate(effectPrefab, transform.position, Quaternion.identity);
//                }
//            }
//        }
//    }

//    private void PlayPhaseCameraEffects(int phase)
//    {
//        var currentPhaseData = bossMonster?.CurrentPhaseData;
//        if (currentPhaseData != null)
//        {
//            // 카메라 효과 적용...
//        }
//    }

//    private void PlayRageModeEffects()
//    {
//        if (bossMonster?.GetMonsterData() is BossData bossData &&
//            bossData.rageEffect != null)
//        {
//            Instantiate(bossData.rageEffect, transform.position, Quaternion.identity);
//        }
//    }

//    private void CleanupRageModeEffects()
//    {
//        // 레이지 모드 이펙트 정리
//    }

//    protected override void OnDestroy()
//    {
//        if (bossMonster != null)
//        {
//            bossMonster.OnPhaseChanged -= HandlePhaseChanged;
//            bossMonster.OnRageModeEntered -= HandleRageModeEntered;
//            bossMonster.OnRageModeEnded -= HandleRageModeEnded;
//        }
//        base.OnDestroy();
//    }

//    public override void TakeDamage(int damage, AttackType attackType)
//    {
//        base.TakeDamage(damage, attackType);

//        // 보스 전용 피격 효과
//        if (bossUIManager != null)
//        {
//            bossUIManager.UpdatePhaseUI(); // 체력에 따른 페이즈 UI 업데이트
//        }
//    }

//    public override void Die()
//    {
//        if (!IsDead)
//        {
//            // 보스 사망 연출
//            StartCoroutine(BossDeathSequence());
//        }
//    }

//    private IEnumerator BossDeathSequence()
//    {
//        // 보스 사망 연출...
//        if (bossMonster?.GetMonsterData() is BossData bossData)
//        {
//            // 사망 이펙트
//            // 카메라 효과
//            // UI 효과
//            yield return new WaitForSeconds(bossData.deathDuration);
//        }

//        base.Die();
//    }
//}