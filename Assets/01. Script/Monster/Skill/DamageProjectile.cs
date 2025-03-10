using UnityEngine;
/// <summary>
/// 직접적인 데미지
/// </summary>
public class DamageProjectile : BaseProjectile
{
    private bool hasDamageApplied = false;

    private void OnTriggerEnter(Collider other)
    {
        GameObject effect = Instantiate(hitEffect, transform.position, transform.rotation);
        Debug.Log("닿앗는디");
        OnImpact(other);
        if (hasDamageApplied) return;
       
    }

    protected override void OnImpact(Collider other)
    {
        
        // 이펙트를 생성하되, 부모를 따로 지정하지 않음

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