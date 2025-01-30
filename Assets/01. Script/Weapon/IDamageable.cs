using static AttackData;

public interface IDamageable
{
    void TakeDamage(int damage);
    void TakeDotDamage(int dotDamage);
}