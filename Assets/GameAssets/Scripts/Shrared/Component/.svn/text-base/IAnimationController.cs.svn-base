using System;
using System.Collections.Generic;

namespace DeltaCommon.Component
{
    public enum MirrorMode { None=0, Horizontal=1, Vertical=2, Both=3 }

    /// <summary>
    /// Common interface for a simple animation controller
    /// </summary>
    public interface IAnimationController
    {
        /// <summary>
        /// Step the animation
        /// </summary>
        /// <param name="dt"></param>
        void Activity(float dt);

        /// <summary>
        /// Play this animation, starting at frame = 0
        /// </summary>
        /// <param name="animationName"></param>
        void StartAnimation(string animationName, MirrorMode mode);
        
        /// <summary>
        /// If this anim is not playing, play it
        /// </summary>
        /// <param name="animationName"></param>
        void PlayAnimation(string animationName, MirrorMode mode);      

        /// <summary>
        /// Looping?
        /// </summary>
        MirrorMode IsMirrored { get; }

        /// <summary>
        /// Is the anim at the last frame
        /// </summary>
        bool IsLastFrame { get;  }

    }
}
