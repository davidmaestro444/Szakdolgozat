using UnityEngine;
using System.Collections;
using Spine;
using Spine.Unity;

public class KnightControl : MonoBehaviour
{
    #region Inspector
    // [SpineAnimation] attribute allows an Inspector dropdown of Spine animation names coming form SkeletonAnimation.
    [SpineAnimation]
    public string runAnimationName;

    [SpineAnimation]
    public string idleAnimationName;

    [SpineAnimation]
    public string walkAnimationName;

    [SpineAnimation]
    public string atkAnimationName_1;

    [SpineAnimation]
    public string atkAnimationName_2;

    [SpineAnimation]
    public string jumpAnimationName;

    [SpineAnimation]
    public string hitAnimationName;

    [SpineAnimation]
    public string deathAnimationName;

    [SpineAnimation]
    public string stunAnimationName;

    [SpineAnimation]
    public string skillAnimationName_1;
    [SpineAnimation]
    public string skillAnimationName_2;
    [SpineAnimation]
    public string skillAnimationName_3;

    #endregion

    SkeletonAnimation skeletonAnimation;

    // Spine.AnimationState and Spine.Skeleton are not Unity-serialized objects. You will not see them as fields in the inspector.
    public Spine.AnimationState spineAnimationState;
    public Spine.Skeleton skeleton;
    public float attackAnimationSpeed = 2f;


    // Start is called before the first frame update
    void Awake()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation != null)
        {
            spineAnimationState = skeletonAnimation.AnimationState;
            skeleton = skeletonAnimation.Skeleton;
        }
    }

    public void running()
    {
        spineAnimationState.SetAnimation(0, runAnimationName, true);
    }
    public void walking()
    {
        spineAnimationState.SetAnimation(0, walkAnimationName, true);
    }
    public void idle()
    {
        spineAnimationState.SetAnimation(0, idleAnimationName, true);
    }
    public Spine.TrackEntry jump()
    {
        return spineAnimationState.SetAnimation(0, jumpAnimationName, false);
    }
    public void getHit()
    {
        spineAnimationState.SetAnimation(0, hitAnimationName, true);
    }
    public Spine.TrackEntry death()
    {
        return spineAnimationState.SetAnimation(0, deathAnimationName, false);
    }
    public void stun()
    {
        spineAnimationState.SetAnimation(0, stunAnimationName, true);
    }
    public Spine.TrackEntry attack_1()
    {
        TrackEntry trackEntry = spineAnimationState.SetAnimation(0, atkAnimationName_1, false);
        trackEntry.TimeScale = attackAnimationSpeed;
        return trackEntry;
    }
    public void attack_2()
    {
        spineAnimationState.SetAnimation(0, atkAnimationName_2, true);
    }
    public void skill_1()
    {
        spineAnimationState.SetAnimation(0, skillAnimationName_1, true);
    }
    public void skill_2()
    {
        spineAnimationState.SetAnimation(0, skillAnimationName_2, true);
    }
    public void skill_3()
    {
        spineAnimationState.SetAnimation(0, skillAnimationName_3, true);
    }

}
