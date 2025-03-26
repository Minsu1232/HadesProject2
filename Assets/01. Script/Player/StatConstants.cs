// StatConstants.cs
public static class StatConstants
{
    // ���̽� ���� ���
    public const int BASE_HP = 20000;
    public const int BASE_GAGE = 0;
    public const int BASE_ATTACK_POWER = 10;
    public const float BASE_ATTACK_SPEED = 1f;
    public const float BASE_CRITICAL_CHANCE = 0.3f;
    public const float BASE_SPEED = 10f;
    public const float BASE_DAMAGE_RECEIVE_RATE = 1f;

    // ���׷��̵�� ������ ���
    public const int HP_PER_UPGRADE = 5;
    public const int GAGE_PER_UPGRADE = 2;
    public const int ATTACK_POWER_PER_UPGRADE = 2;
    public const float ATTACK_SPEED_PER_UPGRADE = 0.1f;
    public const float CRITICAL_CHANCE_PER_UPGRADE = 0.05f;
    public const float SPEED_PER_UPGRADE = 0.03f;
    public const float DAMAGE_REDUCE_PER_UPGRADE = 0.05f;

    // ��� ���� ���
    public const float BASE_DASH_FORCE = 20f;      // �⺻ ��� ��
    public const float BASE_DASH_DURATION = 0.2f;  // �⺻ ��� ���ӽð�
    public const float BASE_DASH_COOLDOWN = 1.5f;  // �⺻ ��� ��Ÿ��

    // ��� ���� ���
    public const float MIN_DASH_COOLDOWN = 0.2f;   // �ּ� ��� ��Ÿ��
    public const float MAX_DASH_FORCE = 40f;       // �ִ� ��� ��
    public const float MAX_DASH_DURATION = 0.5f;   // �ִ� ��� ���ӽð�

}