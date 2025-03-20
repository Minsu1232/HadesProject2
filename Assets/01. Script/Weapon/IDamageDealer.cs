using System;

public interface IDamageDealer
{
    void DealDamage(IDamageable target);
    int GetDamage();

    event Action<int, ICreatureStatus> OnFinalDamageCalculated;
}