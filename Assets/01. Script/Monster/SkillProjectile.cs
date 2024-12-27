using UnityEngine;

public class SkillProjectile : MonoBehaviour
{
    private bool hasDamageApplied = false;  // ������ ���� ���� üũ
    private Transform target;
    private float speed;
    private float damage;
    private bool isLaunched = false;
    private IProjectileMovement moveStrategy;
    private void Start()
    {
       
    }
    public void Initialize(Vector3 startPos, Transform target, float speed, float damage, IProjectileMovement moveStrategy)
    {
        hasDamageApplied = false;  // ������ ���� ���� �ʱ�ȭ
        transform.position = startPos;
        this.target = target;
        this.speed = speed;
        this.damage = damage;
        this.moveStrategy = moveStrategy;

        // �ʱ� ���� ����
        Vector3 direction = (target.position - transform.position).normalized;
        transform.forward = direction;

        Vector3 targetPoint = target.position + Vector3.up * 1.5f; // Ÿ���� ��ü ���̷� ����
        transform.forward = (targetPoint - startPos).normalized;
    }

    public void Launch()
    {
        isLaunched = true;
    }

    private void Update()
    {
        if (!isLaunched || moveStrategy == null) return;
        moveStrategy.Move(transform, target, speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasDamageApplied)
        {
            return;  // �̹� �������� ��ٸ� ����
        }
       

        if (other.CompareTag("Player"))
        {
            PlayerClass player = GameInitializer.Instance.GetPlayerClass();
            if (player != null)
            {
                player.TakeDamage((int)damage, AttackData.AttackType.Charge);
                hasDamageApplied = true;  // ������ ���� ǥ��
                Destroy(gameObject);
            }
        }
        
    }
}
