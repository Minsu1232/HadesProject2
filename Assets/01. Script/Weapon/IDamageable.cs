using static AttackData;
public enum DamageType
{
    Player,
    Monster,
    Boss,
    Structure // (��: �ǹ�, ��ֹ�)
}

public interface IDamageable
{
    DamageType GetDamageType();
    void TakeDamage(int damage);
    void TakeDotDamage(int dotDamage);

}