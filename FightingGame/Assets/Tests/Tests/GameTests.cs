using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameTests
{
    [Test]
    public void InitialState_Test()
    {
        GameObject go = new GameObject();
        DamageBox db = go.AddComponent<DamageBox>();

        Assert.IsFalse(db.isDead, "A karakternek élve kell kezdenie.");
        Assert.IsFalse(db.hasShield, "A karakternek alapból nincs pajzsa.");
    }

    [Test]
    public void GetHitWithoutShield_Test()
    {
        GameObject go = new GameObject();
        DamageBox db = go.AddComponent<DamageBox>();
        db.GetHit();

        Assert.IsTrue(db.isDead, "A pajzs nélküli találat = halál.");
    }

    [Test]
    public void EnableShield_Test()
    {
        GameObject go = new GameObject();
        DamageBox db = go.AddComponent<DamageBox>();
        db.EnableShield();

        Assert.IsTrue(db.hasShield, "A pajzs aktív.");
    }

    [Test]
    public void GetHitWithShield_Test()
    {
        GameObject go = new GameObject();
        DamageBox db = go.AddComponent<DamageBox>();
        db.EnableShield();
        db.GetHit();

        Assert.IsFalse(db.isDead, "A karakter nem hal meg.");
        Assert.IsFalse(db.hasShield, "A pajzs megsemmisül.");
    }

    [Test]
    public void ResetCharacter_Test()
    {
        GameObject go = new GameObject();
        DamageBox db = go.AddComponent<DamageBox>();
        db.EnableShield();
        db.GetHit();
        db.GetHit();
        db.ResetCharacter();

        Assert.IsFalse(db.isDead, "A karakter újra éled.");
        Assert.IsFalse(db.hasShield, "Nem marad rajta pajzs.");
    }

    [Test]
    public void HasWeaponReturnsFalse_Test()
    {
        GameObject go = new GameObject();
        WeaponManager wm = go.AddComponent<WeaponManager>();
        wm.currentWeapon = null;
        bool result = wm.HasWeapon();

        Assert.IsFalse(result, "Ha nincs fegyver a kézben akkor hamis.");
    }

    [Test]
    public void HasWeaponReturnsTrue_Test()
    {
        GameObject go = new GameObject();
        WeaponManager wm = go.AddComponent<WeaponManager>();
        GameObject sword = new GameObject("TestSword");
        wm.currentWeapon = sword.AddComponent<SwordController>();
        bool result = wm.HasWeapon();

        Assert.IsTrue(result, "Ha van beállítva fegyver akkor igaz.");
    }

    [Test]
    public void HasBowReturnsTrue_Test()
    {
        GameObject go = new GameObject();
        WeaponManager wm = go.AddComponent<WeaponManager>();
        GameObject bowObj = new GameObject("LongBow");
        wm.currentWeapon = bowObj.AddComponent<BowController>();
        bool result = wm.HasBow();

        Assert.IsTrue(result, "A fegyver nevében szerepel a Bow szó.");
    }

    [Test]
    public void HasBowReturnsFalse_Test()
    {
        GameObject go = new GameObject();
        WeaponManager wm = go.AddComponent<WeaponManager>();
        GameObject swordObj = new GameObject("Heavy_Sword");
        wm.currentWeapon = swordObj.AddComponent<SwordController>();
        bool result = wm.HasBow();

        Assert.IsFalse(result, "Ha nem íj van a kezünkben akkor hamis.");
    }

    [Test]
    public void PrepareForThrow_Test()
    {
        GameObject go = new GameObject();
        HitBox hitBox = go.AddComponent<HitBox>();
        string testTag = "Player1";
        hitBox.PrepareForThrow(testTag);

        Assert.AreEqual(testTag, hitBox.ownerTag, "A metódusnak helyesen be kell állítania az ownerTag változót.");
    }
}
