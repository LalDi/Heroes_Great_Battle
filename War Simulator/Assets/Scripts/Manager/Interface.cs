using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    #region [AnimInterface]
public interface AnimInterface
{
    void playingAnimationEvent();
    void transitionAnimationEvent();
    void endingAnimationEvent();
}
#endregion
