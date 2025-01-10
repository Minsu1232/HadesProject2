using UnityEngine;
/// <summary>
/// �� �����տ� ���� ��ũ��Ʈ
/// </summary>
public class PoisonArea : BaseAreaEffect
{
    protected override void ApplyAreaDamage()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                PlayerClass player = GameInitializer.Instance.GetPlayerClass();
                if (player != null)
                {
                    player.TakeDamage((int)damage, AttackData.AttackType.Normal);
                    break; // �ݶ��̴��� �������⿡ �극��ũ�� ����
                }
            }
        }
    }
}