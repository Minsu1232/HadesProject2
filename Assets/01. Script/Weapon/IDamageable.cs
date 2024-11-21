using static AttackData;

public interface IDamageable
{
    void TakeDamage(int damage, AttackType attackType);
    void TakeDotDamage(int dotDamage);
}