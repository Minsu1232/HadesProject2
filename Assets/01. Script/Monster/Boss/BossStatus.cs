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
        if (!isDie)
        {
            Debug.Log(monsterClass.MONSTERNAME);
            Debug.Log("쭉었다");
            monsterClass.Die();
            isDie = true;
            Debug.Log(monsterClass.IsAlive);
            if (ItemDropSystem.Instance != null)
            {
                // 몬스터 데이터 확인
                ICreatureData monData = monsterClass.GetMonsterData();
                Debug.Log($"[Monster] Die - 몬스터 ID: {monData.MonsterID}, 드롭 아이템: {monData.dropItem}, 드롭 확률: {monData.dropChance}");

                // 드롭 테이블 확인
                if (DropTableManager.Instance != null)
                {
                    var dropTable = DropTableManager.Instance.GetBossDropTable(monData.MonsterID);
                    Debug.Log($"[Monster] 드롭 테이블 검색 결과: {monsterClass.MONSTERNAME}{(dropTable != null ? dropTable.Count + "개 항목" : "없음")}");
                }
                else
                {
                    Debug.LogError("[Monster] DropTableManager 인스턴스가 null입니다!");
                }

                // 아이템 드롭 실행
                ItemDropSystem.Instance.DropItemFromBoss(monData, transform.position);
                DungeonManager.Instance.OnMonsterDefeated(this);
            }

            Destroy(gameObject); // 몬스터 오브젝트 삭제
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
    public override DamageType GetDamageType()
    {
        Debug.Log("보스데미지타입호출");
        return DamageType.Boss;
    }

    
}