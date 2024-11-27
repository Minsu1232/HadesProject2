using UnityEngine;
[CreateAssetMenu(menuName = "Weapon")]
public class WeaponScriptableObject : ScriptableObject
{
    public string weaponName;           // 무기 이름
    public int baseDamage;              // 기본 데미지
    public int baseGagePerHit;          // 기본 게이지 증가량
    public Vector3 defaultPosition;     // 초기 위치
    public Vector3 defaultRotation;     // 초기 회전값
    public float maxChargeTime;         // 최대 차지 시간
    public float chargeMultiplier;      // 차지 시 데미지 배율
    public int damageUpgradeCount;      // 데미지 업그레이드 횟수
    public int gageUpgradeCount;        // 게이지 업그레이드 횟수
    public int additionalDamage;        // 추가 데미지
    public int additionalGagePerHit;    // 추가 게이지 증가량
    public GameObject vfxPrefab;        // 스페셜어택 VFX
    public AudioClip soundEffect;       // 스페셜어택 사운드
    public Color gageColor;             // 무기별 게이지 컬러
}