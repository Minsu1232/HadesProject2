// ���� ������ �����ϴ� MonoBehaviour
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
        // ���� ����Ʈ ����
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // �� ���� Ÿ�Կ� ���� ������ ����
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
        // ���� ������ �ִ� ��� �ݶ��̴� �˻�
        Collider[] colliders = Physics.OverlapSphere(transform.position, dangerRadius);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                // �÷��̾���� �Ÿ� ���
                float distance = Vector3.Distance(transform.position, collider.transform.position);

                // ���� ���� �ۿ� �ִ��� Ȯ�� (���� ��� ����)
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
        // ��ü ������ ������
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

    // ����׿� ����� �׸���
    private void OnDrawGizmos()
    {
        if (isRingShaped)
        {
            // ���� ���� - ���
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, safeZoneRadius);

            // ���� ���� - ������
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, dangerRadius);
        }
        else
        {
            // �Ķ��� ���� ���� - �Ķ���
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, safeZoneRadius);
        }
    }
}