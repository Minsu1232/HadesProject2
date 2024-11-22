using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpecialAttackBase
{
    protected WeaponManager weaponManager;  
    Animator animator;
   

    public SpecialAttackBase(WeaponManager weapon)
    {
        this.weaponManager = weapon;
       
    }

    public virtual void Execute()
    {
        //if (!weaponManager.CanUseSpecialAttack)
        //{
        //    Debug.LogWarning("��ų ��� �Ұ�: ������ ����");
        //    return;
        //}      
   
        // ���� ������ �ʱ�ȭ
        weaponManager.ResetGage();

        // ��ų ���� ȿ�� ����
        PerformSkillEffect();
    }

    public virtual void PlayVFX()
    {
        if (weaponManager.weaponData.vfxPrefab != null)
        {
            GameObject vfx = GameObject.Instantiate(weaponManager.weaponData.vfxPrefab, weaponManager.transform.position, Quaternion.identity);
            Debug.Log("��");
           
        }
    }

    public virtual void PlaySound()
    {
        if (weaponManager.weaponData.soundEffect != null)
        {
            AudioSource.PlayClipAtPoint(weaponManager.weaponData.soundEffect, weaponManager.transform.position);
        }
    }

    protected abstract void PerformSkillEffect(); // �� ���� ���� ȿ��
}
