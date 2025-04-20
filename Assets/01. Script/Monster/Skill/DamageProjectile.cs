using UnityEngine;
/// <summary>
/// 직접적인 데미지
/// </summary>
public class DamageProjectile : BaseProjectile
{     


    private bool hasDamageApplied = false;
    private void OnTriggerEnter(Collider other)
    {
        // 이미 데미지가 적용되었다면 반환
        if (hasDamageApplied) return;

        Debug.Log("닿앗는디");
        OnImpact(other);
    }

    protected override void OnImpact(Collider other)
    {
        // 이미 데미지가 적용되었다면 함수 실행 중단
        if (hasDamageApplied) return;

        // 이펙트를 생성하되, 부모를 따로 지정하지 않음
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