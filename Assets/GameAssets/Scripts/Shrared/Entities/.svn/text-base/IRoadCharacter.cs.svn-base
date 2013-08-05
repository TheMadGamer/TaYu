using System;
using System.Collections.Generic;

using DeltaCommon.Component;

namespace DeltaCommon.Entities
{
    /// <summary>
    /// Properties: X,Y,ICharacterController,IAnimationController
    ///
    /// Method:Activity, Destroy, IsDead
    /// </summary>
    public interface IRoadCharacter
    {
        float X { get; set; }
        float Y { get; set; }
        bool StartFeeding { get; set; }
        int TargetFood { get; set; }
        CardinalPoint CardinalDirection { get; set; }
        bool IsFed { get; set; }
        ICharacterController BehaviorController { get; }
        IAnimationController AnimController { get; }

        void Activity(float dt, BoardController boardController);
        void Destroy();

        bool IsDead();
#if (WINDOWS_PHONE &&!SILVERLIGHT)
        bool Visibility { get; set; }
#endif
    }

    public class RoadAnimationNames
    {
        public const string kIdle = "idle";
        public const string kWalkRight = "walk_right";
        public const string kWalkForward = "walk_forward";
        public const string kWalkBack = "walk_back";
        public const string kRunRight = "run_right";
        public const string kRunForward = "run_forward";
        public const string kRunBack = "run_back";
        public const string kSpawnIn = "spawn_in";
        public const string kSpawnOut = "spawn_out";
        public const string kWalkBackRight = "walk_back_right";
        public const string kWalkForwardRight = "walk_forward_right";
        public const string kEatCarrot = "eat_carrot";
        public const string kEatFlower = "eat_flower";
        public const string kWinDance = "win_dance";
    }

}
