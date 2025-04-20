using UnityEngine;
/// <summary>
/// �������� ������
/// </summary>
public class DamageProjectile : BaseProjectile
{     


    private bool hasDamageApplied = false;
    private void OnTriggerEnter(Collider other)
    {
        // �̹� �������� ����Ǿ��ٸ� ��ȯ
        if (hasDamageApplied) return;

        Debug.Log("��Ѵµ�");
        OnImpact(other);
    }

    protected override void OnImpact(Collider other)
    {
        // �̹� �������� ����Ǿ��ٸ� �Լ� ���� �ߴ�
        if (hasDamageApplied) return;

        // ����Ʈ�� �����ϵ�, �θ� ���� �������� ����
        GameObject effect = Instantiate(hitEffect, transform.position, transform.rotation);

        if (other.CompareTag("Player"))
        {
            ApplyDamageToPlayer(other);
            hasDamageApplied = true;
        }

        Destroy(gameObject);
    }

    private void ApplyDamageToPlayer(Collider other)
    {
        PlayerClass player = GameInitializer.Instance.GetPlayerClass();
        if (player != null)
        {
            player.TakeDamage((int)damage);
        }
    }
}