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

//            // ���� �̺�Ʈ ����
//            var boss = monster as BossMonster;
//            boss.OnPhaseChanged += HandlePhaseChanged;
//            boss.OnRageModeEntered += HandleRageModeEntered;
//            boss.OnRageModeEnded += HandleRageModeEnded;

//            // ���� UI �ʱ�ȭ
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
//        // UI ������Ʈ
//        bossUIManager?.UpdatePhase(newPhase);

//        // ������ ��ȯ ����Ʈ
//        PlayPhaseTransitionEffects(newPhase);

//        // ī�޶� ȿ��
//        PlayPhaseCameraEffects(newPhase);
//    }

//    private void HandleRageModeEntered()
//    {
//        // UI ������Ʈ
//        bossUIManager?.SetRageModeUI(true);

//        // ��������� ����Ʈ
//        PlayRageModeEffects();
//    }

//    private void HandleRageModeEnded()
//    {
//        // UI ������Ʈ
//        bossUIManager?.SetRageModeUI(false);

//        // ����Ʈ ����
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
//            // ī�޶� ȿ�� ����...
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
//        // ������ ��� ����Ʈ ����
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

//        // ���� ���� �ǰ� ȿ��
//        if (bossUIManager != null)
//        {
//            bossUIManager.UpdatePhaseUI(); // ü�¿� ���� ������ UI ������Ʈ
//        }
//    }

//    public override void Die()
//    {
//        if (!IsDead)
//        {
//            // ���� ��� ����
//            StartCoroutine(BossDeathSequence());
//        }
//    }

//    private IEnumerator BossDeathSequence()
//    {
//        // ���� ��� ����...
//        if (bossMonster?.GetMonsterData() is BossData bossData)
//        {
//            // ��� ����Ʈ
//            // ī�޶� ȿ��
//            // UI ȿ��
//            yield return new WaitForSeconds(bossData.deathDuration);
//        }

//        base.Die();
//    }
//}