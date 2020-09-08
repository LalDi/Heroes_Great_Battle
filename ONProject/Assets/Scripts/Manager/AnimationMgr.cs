using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationMgr
{
    #region [Animation Manager]
    public static IEnumerator checkAnimationState(string clipname, Animator anim, GameObject obj)
    {
        AnimInterface mgr = obj.GetComponent<AnimInterface>();

        // 애니메이션을 재생 중일 때
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            // 플레이 중
            mgr.playingAnimationEvent();
            yield return null;
        }

        // 애니메이션을 바꿀 때
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName(clipname))
        {
            // 전환 중
            mgr.transitionAnimationEvent();
            yield return null;
        }
        // 애니메이션 완료
        mgr.endingAnimationEvent();
    }
    #endregion
}
