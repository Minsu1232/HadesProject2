// 지연 폭발을 관리하는 MonoBehaviour
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    private float safeZoneRadius;
    private float dangerRadius;
    private float explosionDelay;
    private bool isRingShaped;
    private GameObject explosionEffect;
    private float damage;
    private float timer = 0f;

    public void Initialize(float safeZoneRadius, float dangerRadius, float explosionDelay, bool isRingShaped, GameObject explosionEffect, float damage)
    {
        this.safeZoneRadius = safeZoneRadius;
        this.dangerRadius = dangerRadius;
        this.explosionDelay = explosionDelay;
        this.isRingShaped = isRingShaped;
        this.explosionEffect = explosionEffect;
        this.damage = damage;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= explosionDelay)
        {
            Explode();
            Destroy(gameObject);
        }
    }

    private void Explode()
    {
        // 폭발 이펙트 생성
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // 각 폭발 타입에 따른 데미지 적용
        if (isRingShaped)
        {
            ApplyRingExplosionDamage();
        }
        else
        {
            ApplyFullExplosionDamage();
        }
    }

    private void ApplyRingExplosionDamage()
    {
        // 위험 구역에 있는 모든 콜라이더 검색
        Collider[] colliders = Physics.OverlapSphere(transform.position, dangerRadius);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                // 플레이어와의 거리 계산
                float distance = Vector3.Distance(transform.position, collider.transform.position);

                // 안전 구역 밖에 있는지 확인 (도넛 모양 영역)
                if (distance > safeZoneRadius)
                {
                    PlayerClass player = GameInitializer.Instance.GetPlayerClass();
                    if (player != null)
                    {
                        player.TakeDamage((int)damage);
                    }
                }
            }
        }
    }

    private void ApplyFullExplosionDamage()
    {
        // 전체 범위에 데미지
        Collider[] colliders = Physics.OverlapSphere(transform.position, safeZoneRadius);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                PlayerClass player = GameInitializer.Instance.GetPlayerClass();
                if (player != null)
                {
                    player.TakeDamage((int)damage);
                }
            }
        }
    }

    // 디버그용 기즈모 그리기
    private void OnDrawGizmos()
    {
        if (isRingShaped)
        {
            // 안전 영역 - 녹색
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, safeZoneRadius);

            // 위험 영역 - 빨간색
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, dangerRadius);
        }
        else
        {
            // 파란색 발톱 영역 - 파란색
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, safeZoneRadius);
        }
    }
}