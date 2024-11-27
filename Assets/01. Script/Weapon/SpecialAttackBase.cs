using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpecialAttackBase
{
    protected WeaponManager weaponManager;
    public int WeaponresetGage { get; protected set; }
    public bool isSpecialAttack;
    Animator animator;
   

    public SpecialAttackBase(WeaponManager weapon)
    {
        this.weaponManager = weapon;
       
    }

    public virtual void Execute()
    {
        //if (!weaponManager.CanUseSpecialAttack)
        //{
        //    Debug.LogWarning("스킬 사용 불가: 게이지 부족");
        //    return;
        //}      
   
       

        // 스킬 고유 효과 실행
        PerformSkillEffect();

        // 무기 게이지 초기화
        weaponManager.ResetGage(WeaponresetGage);
    }

    public virtual void PlayVFX()
    {
        if (weaponManager.weaponData.vfxPrefab != null)
        {
            GameObject vfx = GameObject.Instantiate(weaponManager.weaponData.vfxPrefab, weaponManager.transform.position, Quaternion.identity);
            Debug.Log("솬");
           
        }
    }

    public virtual void PlaySound()
    {
        if (weaponManager.weaponData.soundEffect != null)
        {
            AudioSource.PlayClipAtPoint(weaponManager.weaponData.soundEffect, weaponManager.transform.position);
        }
    }

    protected abstract void PerformSkillEffect(); // 각 무기 고유 효과
}
