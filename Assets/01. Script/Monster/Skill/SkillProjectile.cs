
using UnityEngine;
/// <summary>
/// 트리거 후 영향을 주는 발사체
/// </summary>
public class SkillProjectile : BaseProjectile
{
    private bool hasDamageApplied = false;
    [SerializeField] GameObject particle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster") || hasDamageApplied) return;  // 몬스터는 무시

        // 충돌 지점에 효과 생성
        impactEffect?.OnImpact(transform.position, damage);
        OnImpact(other);
        Debug.Log("독구름펑");
        hasDamageApplied = true;
        // 발사체 제거
        Destroy(gameObject);
    }

    protected override void OnImpact(Collider other)
    {
        GameObject effect = Instantiate(hitEffect, transform.position, transform.rotation);
    }

}