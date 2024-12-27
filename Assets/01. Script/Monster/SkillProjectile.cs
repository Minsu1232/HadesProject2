using UnityEngine;

public class SkillProjectile : MonoBehaviour
{
    private bool hasDamageApplied = false;  // 데미지 적용 여부 체크
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
        hasDamageApplied = false;  // 데미지 적용 상태 초기화
        transform.position = startPos;
        this.target = target;
        this.speed = speed;
        this.damage = damage;
        this.moveStrategy = moveStrategy;

        // 초기 방향 설정
        Vector3 direction = (target.position - transform.position).normalized;
        transform.forward = direction;

        Vector3 targetPoint = target.position + Vector3.up * 1.5f; // 타겟의 상체 높이로 조정
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
            return;  // 이미 데미지를 줬다면 리턴
        }
       

        if (other.CompareTag("Player"))
        {
            PlayerClass player = GameInitializer.Instance.GetPlayerClass();
            if (player != null)
            {
                player.TakeDamage((int)damage, AttackData.AttackType.Charge);
                hasDamageApplied = true;  // 데미지 적용 표시
                Destroy(gameObject);
            }
        }
        
    }
}
