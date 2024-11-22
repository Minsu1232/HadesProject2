public interface ICharacterAttack
{
    void BasicAttack(); // 기본 공격 정의
    void EquipWeapon(IWeapon currentWeapon);
    void SpecialAttack(); // 스킬 공격 정의
}