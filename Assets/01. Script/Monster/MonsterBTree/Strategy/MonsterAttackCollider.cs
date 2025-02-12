using UnityEngine;

public class MonsterAttackCollider : MonoBehaviour
{
    private IMonsterClass monsterClass;
    private CreatureAI CreatureAI;
    private bool canDealDamage = false;
    PlayerClass player;

    private void Start()
    {
        CreatureAI = GetComponent<CreatureAI>();
        monsterClass = GetComponentInParent<MonsterStatus>().GetMonsterClass();
        player = GameInitializer.Instance.GetPlayerClass();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canDealDamage) return;
        if (other.gameObject.CompareTag("Player"))
        {
            if (player != null)
            {
                canDealDamage = false;  // �� ���� ������
                IAttackStrategy strategy = CreatureAI.GetAttackStrategy();
                strategy.ApplyDamage(player, monsterClass);
            }
        }
    }

    // �ִϸ��̼� ���� �������� ȣ��
    public void EnableDamage()
    {
        canDealDamage = true;
        var strategy = CreatureAI.GetAttackStrategy();
        
    }

    // �ִϸ��̼� ���� �������� ȣ��
    public void DisableDamage()
    {
        canDealDamage = false;
        var strategy = CreatureAI.GetAttackStrategy();
        if (strategy is BossMultiAttackStrategy physicalStrategy)
        {
            physicalStrategy.StopAttack();
            physicalStrategy.UpdateLastAttackTime();
            Debug.Log("�Ĵٺ�����" + physicalStrategy.IsAttacking);
        }
    }
}