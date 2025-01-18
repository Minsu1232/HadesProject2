using UnityEngine;

public class DamageProjectile : BaseProjectile
{
    private bool hasDamageApplied = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasDamageApplied) return;
        OnImpact(other);
    }

    protected override void OnImpact(Collider other)
    {
        // ����Ʈ�� �����ϵ�, �θ� ���� �������� ����
        
        if (other.CompareTag("Player"))
        {
            GameObject effect = Instantiate(hitEffect, transform.position, transform.rotation);
            ApplyDamageToPlayer(other);
           
        }
        Destroy(gameObject);
    }

    private void ApplyDamageToPlayer(Collider other)
    {
        PlayerClass player = GameInitializer.Instance.GetPlayerClass();
        if (player != null)
        {
            player.TakeDamage((int)damage, AttackData.AttackType.Normal);
            hasDamageApplied = true;
        }
    }
}