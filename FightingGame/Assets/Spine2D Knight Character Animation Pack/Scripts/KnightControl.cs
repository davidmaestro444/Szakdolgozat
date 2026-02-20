using UnityEngine;
using System.Collections;

public class KnightControl : MonoBehaviour
{
    private Animator anim;
    private WeaponManager weaponManager;

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        weaponManager = GetComponentInParent<WeaponManager>();
        if (weaponManager == null) weaponManager = GetComponent<WeaponManager>();
    }

    public void idle()
    {
        if (anim == null) return;

        if (weaponManager != null && weaponManager.HasWeapon())
        {
            anim.Play(weaponManager.currentWeapon.idleAnim);
        }
        else
        {
            anim.Play("char_idle");
        }
    }

    public void running()
    {
        if (anim == null) return;

        if (weaponManager != null && weaponManager.HasWeapon())
            anim.Play(weaponManager.currentWeapon.runAnim);
        else
            anim.Play("char_run");
    }

    public void attack_1()
    {
        if (anim == null) return;

        if (weaponManager != null && weaponManager.HasWeapon())
        {
            anim.Play(weaponManager.currentWeapon.attackAnim);
        }
        else
        {
            anim.Play("char_punch");
        }
    }

    public void jump()
    {
        if (anim != null) anim.Play("char_jump");
    }

    public void death()
    {
        if (anim != null) anim.Play("char_death");

        if (weaponManager != null && weaponManager.HasWeapon())
        {
            weaponManager.currentWeapon.gameObject.SetActive(false);
        }
    }

    public void weapon_throw()
    {
        if (anim == null) return;

        if (weaponManager != null && weaponManager.HasWeapon() && !string.IsNullOrEmpty(weaponManager.currentWeapon.throwAnim))
        {
            anim.Play(weaponManager.currentWeapon.throwAnim);
        }
    }

    public void ResetState() => idle();
}
