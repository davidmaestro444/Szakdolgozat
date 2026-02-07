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

        bool actuallyHasWeapon = false;
        if (weaponManager != null && weaponManager.handSocket != null)
        {
            actuallyHasWeapon = weaponManager.handSocket.childCount > 0;
        }

        if (actuallyHasWeapon)
        {
            anim.Play("char_sword_idle");
        }
        else
        {
            anim.Play("char_idle");
        }
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
            if (weaponManager.currentWeapon.name.Contains("Sword"))
                anim.Play("char_sword_attack");
            else
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
    public void death() {}
    public void ResetState() => idle();
}
