public interface ICharacterAttack
{
    void BasicAttack(); // 기본 공격 정의
    void EquipWeapon(IWeapon currentWeapon);
    void SkillAttack(int skillIndex); // 스킬 공격 정의
}