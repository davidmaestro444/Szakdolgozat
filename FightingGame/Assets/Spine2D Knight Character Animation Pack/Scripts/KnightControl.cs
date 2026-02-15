using UnityEngine;
using System.Collections;
using Spine;
using Spine.Unity;

public class KnightControl : MonoBehaviour
{
    private Animator anim;
    private WeaponManager weaponManager;

    public class DummyTrack
    {
        public bool IsComplete = true;
    }
    private DummyTrack dummy = new DummyTrack();

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

        if (weaponManager == null || weaponManager.currentWeapon == null)
        {
            anim.Play("char_idle");
            return;
        }

        string wName = weaponManager.currentWeapon.name.ToLower();
        if (wName.Contains("sword") || wName.Contains("kard"))
            anim.Play("char_sword_idle");
        else if (wName.Contains("spear") || wName.Contains("landzsa"))
            anim.Play("char_spear_idle");
        else
            anim.Play("char_idle");
    }

    public void running()
    {
        if (anim == null) return;
        anim.Play("char_run");
    }

    public DummyTrack attack_1()
    {
        if (weaponManager != null && weaponManager.currentWeapon != null)
        {
            string wName = weaponManager.currentWeapon.name.ToLower();

            if (wName.Contains("sword") || wName.Contains("kard"))
                anim.Play("char_sword_attack");
            else if (wName.Contains("spear") || wName.Contains("landzsa"))
                anim.Play("char_spear_attack");
            else if (wName.Contains("bow"))
                anim.Play("char_bow_attack");
        }
        else
        {
            anim.Play("char_punch");
        }
        return dummy;
    }

    public DummyTrack jump()
    {
        anim.Play("char_jump");
        return dummy;
    }
    public void death()
    {
        if (anim != null) anim.Play("char_death");

        if (weaponManager != null && weaponManager.currentWeapon != null)
        {
            weaponManager.currentWeapon.SetActive(false);
        }
    }

    public void sword_throw()
    {
        if (anim != null) anim.Play("char_sword_throw");
    }
    public void ResetState() => idle();
}
