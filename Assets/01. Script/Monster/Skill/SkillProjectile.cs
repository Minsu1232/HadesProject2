// ���� ��ų ������Ÿ�� ����
using UnityEngine;

public class SkillProjectile : BaseProjectile
{
    private bool hasDamageApplied = false;
    [SerializeField] GameObject particle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster")) return;  // ���ʹ� ����

        // �浹 ������ ȿ�� ����
        impactEffect?.OnImpact(transform.position, damage);
        OnImpact(other);
        Debug.Log("��������");
        // �߻�ü ����
        Destroy(gameObject);
    }

    protected override void OnImpact(Collider other)
    {
        GameObject effect = Instantiate(hitEffect, transform.position, transform.rotation);
    }

}