using UnityEngine;
[CreateAssetMenu(menuName = "Weapon")]
public class WeaponScriptableObject : ScriptableObject
{
    public string weaponName;           // ���� �̸�
    public int baseDamage;              // �⺻ ������
    public int baseGagePerHit;          // �⺻ ������ ������
    public Vector3 defaultPosition;     // �ʱ� ��ġ
    public Vector3 defaultRotation;     // �ʱ� ȸ����
    public float maxChargeTime;         // �ִ� ���� �ð�
    public float chargeMultiplier;      // ���� �� ������ ����
    public int damageUpgradeCount;      // ������ ���׷��̵� Ƚ��
    public int gageUpgradeCount;        // ������ ���׷��̵� Ƚ��
    public int additionalDamage;        // �߰� ������
    public int additionalGagePerHit;    // �߰� ������ ������
    public GameObject vfxPrefab;        // ����Ⱦ��� VFX
    public AudioClip soundEffect;       // ����Ⱦ��� ����
    public Color gageColor;             // ���⺰ ������ �÷�
}