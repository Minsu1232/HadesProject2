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
        //    Debug.LogWarning("��ų ��� �Ұ�: ������ ����");
        //    return;
        //}      
   
       

        // ��ų ���� ȿ�� ����
        PerformSkillEffect();

        // ���� ������ �ʱ�ȭ
        weaponManager.ResetGage(WeaponresetGage);
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
