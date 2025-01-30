using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using static AttackData;

public class BossStatus : MonsterStatus
{


    [SerializeField] private BossUIManager bossUIManager;

    [FoldoutGroup("Boss Stats"), ReadOnly]
    [ShowInInspector]
    public int CurrentPhase => (monsterClass as BossMonster)?.CurrentPhase ?? 0;

    [FoldoutGroup("Boss Stats"), ReadOnly]
    [ShowInInspector]
    public bool IsInRageMode => (monsterClass as BossMonster)?.IsInRageMode ?? false;

    private BossMonster bossMonster => monsterClass as BossMonster;

    public override void Initialize(IMonsterClass monster)
    {
        if (monster is BossMonster boss)
        {
            monsterClass = boss;
            InitializeUI(boss);
        }
        else
        {
            Debug.LogError("Trying to initialize BossStatus with non-boss monster!");
        }
    }

    private void InitializeUI(BossMonster boss)
    {
        if (bossUIManager == null)
        {
            bossUIManager = GetComponent<BossUIManager>();
        }

        if (bossUIManager != null)
        {
            bossUIManager.Initialize(boss);
        }
    }

    public MonsterClass Monster() => bossMonster;

    public IMonsterClass GetBossMonster() => monsterClass;
    public BossUIManager GetBossUIManager() => bossUIManager;
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (bossUIManager != null)
        {
            bossUIManager.UpdatePhaseUI();
        }
    }
    

    public override void Die()
    {
        if (!IsDead)
        {
            base.Die();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}