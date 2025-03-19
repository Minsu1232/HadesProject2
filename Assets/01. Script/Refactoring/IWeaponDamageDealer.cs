using System;

internal interface IWeaponDamageDealer
{
  int GetDamage();

  event Action<int, ICreatureStatus> OnFinalDamageCalculated;
}