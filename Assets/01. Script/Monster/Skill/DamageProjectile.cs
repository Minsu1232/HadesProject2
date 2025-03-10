using UnityEngine;
/// <summary>
/// �������� ������
/// </summary>
public class DamageProjectile : BaseProjectile
{
    private bool hasDamageApplied = false;

    private void OnTriggerEnter(Collider other)
    {
        GameObject effect = Instantiate(hitEffect, transform.position, transform.rotation);
        Debug.Log("��Ѵµ�");
        OnImpact(other);
        if (hasDamageApplied) return;
       
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
            player.TakeDamage((int)damage);
            hasDamageApplied = true;
        }
    }
}