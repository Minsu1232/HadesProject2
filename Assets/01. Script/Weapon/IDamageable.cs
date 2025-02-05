using static AttackData;
public enum DamageType
{
    Player,
    Monster,
    Boss,
    Structure // (예: 건물, 장애물)
}

public interface IDamageable
{
    DamageType GetDamageType();
    void TakeDamage(int damage);
    void TakeDotDamage(int dotDamage);

}