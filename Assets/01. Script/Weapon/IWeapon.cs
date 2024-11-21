using UnityEngine;

public interface IWeapon
{    // 초기화/로드
    GameObject WeaponLoad(Transform parentTransform);
    void InitializeWeapon(Animator animator);

    // 공격 관련
    void OnAttack(Transform origin, int comboStep);
    int GetDamage(int _baseDamage, int comboStep);
    // 게이지/스킬 시스템
    void GetGage(int amount);
    void SpecialAttack();  // 게이지 100 특수 스킬

    // 차징 시스템
    void StartCharge();
    void UpdateCharge(float deltaTime);
    void ReleaseCharge();


}