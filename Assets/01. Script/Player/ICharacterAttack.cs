public interface ICharacterAttack
{
    void BasicAttack(); // �⺻ ���� ����
    void EquipWeapon(IWeapon currentWeapon);
    void SkillAttack(int skillIndex); // ��ų ���� ����
}