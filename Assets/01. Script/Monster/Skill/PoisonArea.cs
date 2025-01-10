using UnityEngine;
/// <summary>
/// 독 프리팹에 붙은 스크립트
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
                    break; // 콜라이더가 여러개기에 브레이크로 멈춤
                }
            }
        }
    }
}