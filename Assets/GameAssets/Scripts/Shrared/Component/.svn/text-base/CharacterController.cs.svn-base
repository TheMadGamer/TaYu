#undef DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;

using DeltaCommon.Entities;
using DeltaCommon.Managers;

namespace DeltaCommon.Component
{
    // terrible idea to build your own vector class
    public class Vec2
    {
        float mX;
        public float X { get { return mX; } set { mX = value; } }
        float mY;
        public float Y { get { return mY; } set { mY = value; } }
        public Vec2()
        {
            mX = 0;
            mY = 0;
        }

        public Vec2(float x, float y)
        {
            mX = x;
            mY = y;
        }

        static public Vec2 operator +(Vec2 vec0, Vec2 vec1)
        {
            Vec2 returnVec = new Vec2(vec0.X+vec1.X, vec0.Y+vec1.Y);
            return returnVec;
        }

        static public Vec2 operator *(Vec2 vec0, float s)
        {
            Vec2 returnVec = new Vec2(vec0.X * s, vec0.Y * s);
            return returnVec;
        }
        
        static public Vec2 operator -(Vec2 vec0, Vec2 vec1)
        {
            Vec2 returnVec = new Vec2(vec0.X-vec1.X, vec0.Y-vec1.Y);
            return returnVec;
        }

        static public bool operator ==(Vec2 vec0, Vec2 vec1)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(vec0, vec1))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)vec0 == null) || ((object)vec1 == null))
            {
                return false;
            }

            if ((vec0.X == vec1.X) && (vec0.Y == vec1.Y))
            {
                return true;
            }
            else 
            {
                return false;
            }
        }


        static public bool operator !=(Vec2 vec0, Vec2 vec1)
        {
            if (System.Object.ReferenceEquals(vec0, vec1))
            {
                return false;
            }

            // If one is null, but not both, return true.
            if (((object)vec0 == null) || ((object)vec1 == null))
            {
                return true;
            }

            if ((vec0.X != vec1.X) || (vec0.Y != vec1.Y))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Vec2 p = obj as Vec2;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (mY == p.mX) && (mY == p.mY);
        }

        public bool Equals(Vec2 p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (mX == p.mX) && (mY == p.mY);
        }



        public override int GetHashCode()
        {
            return ((int)mX) ^ ((int)mY);
        }
        

        public void Normalize()
        {
            float len = Length();

            mX /= len;
            mY /= len;

        }
        
        static public float Dot(Vec2 vec0, Vec2 vec1)
        {
            return vec0.X * vec1.X + vec0.Y * vec1.Y;
        }

        public float Length()
        {
            float len = Dot(this, this);
            return Convert.ToSingle(Math.Sqrt(len));
        }

    }


    public class CharacterController : ICharacterController
    {
        #region STATE DESCRIPTION
        //The Parent of all States is the Character Controller.
        //The Parent of the CharacterController is the RoadCharacter
        class IdleState : State
        {
            float mDuration = 2.0f;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="eventData"></param>
            public override void Activate(object eventData)
            {
                Debug.WriteLine("Idle State");
                
                if (eventData == null)
                {
                    mDuration = 2.0f;
                }
                else
                {
                    mDuration = (eventData as Vec2).X;
                }
                //Parent is initialized as the CharacterController by the function InitializeStateMachine()
                //((this.Parent as CharacterController).Parent as IRoadCharacter).AnimController.StartAnimation(RoadAnimationNames.kIdle, MirrorMode.None);

                CharacterController controller = (this.Parent as CharacterController);
                IRoadCharacter parent = controller.Parent as IRoadCharacter;
                parent.AnimController.StartAnimation(RoadAnimationNames.kIdle, MirrorMode.None);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="dt"></param>
            public override void Update(float dt)
            {
                if (mDuration > 0)
                {
                    mDuration -= dt;
                }
                else
                {
                    CharacterController controller = (this.Parent as CharacterController);
                    controller.HandleEvent((int) CharacterEvents.To_GetTarget, null);
                }
            }
        }

        class GetWalkTarget : State
        {
            public override void Activate(object eventData)
            {
                CharacterController controller = (this.Parent as CharacterController);
                IRoadCharacter parent = controller.Parent as IRoadCharacter;

                Vec2 direction = controller.GetNextTarget();
                Debug.WriteLine("GetwalkTarget State");
                // no more targets, spawn out
                if (direction == null)
                {
                    if (!parent.IsFed && parent.TargetFood>-1)
                    {
                        if (parent.CardinalDirection == CardinalPoint.E)
                        {
                            parent.StartFeeding = true;
                            controller.HandleEvent((int)CharacterController.CharacterEvents.To_EatFlower, null);
                        }
                        else if (parent.CardinalDirection == CardinalPoint.N)
                        {
                            parent.StartFeeding = true;
                            controller.HandleEvent((int)CharacterController.CharacterEvents.To_EatCarrot, null);
                        }
                        else
                        {
                            controller.HandleEvent((int)CharacterController.CharacterEvents.To_SpawnOut, null);
                        }

                    }
                    else 
                    {
                        controller.HandleEvent((int)CharacterController.CharacterEvents.To_SpawnOut, null);
                    }
                }
                else
                {
                    Debug.WriteLine("Position" + parent.X + " " + parent.Y + " Target" + direction.X + " " + direction.Y);
                    controller.HandleEvent((int)CharacterController.CharacterEvents.To_Walk, direction);
                }
            }

            public override void Update(float dt) {}
        }

        class LocomotionState : State
        {

            protected Vec2 mTarget = null;
            protected float mSpeed = 15.0f;

            public override void Update(float dt)
            {
                CharacterController controller = (this.Parent as CharacterController);
                IRoadCharacter parent = controller.Parent as IRoadCharacter;
                Vec2 direction = mTarget - new Vec2(parent.X, parent.Y);
                Debug.WriteLine("elapsed time" + dt);
          
                float distance = direction.Length();

                if (distance < 0.01)
                {
                    // at target
                    Debug.WriteLine("At Target, Raise Event");
                    controller.HandleEvent((int)CharacterController.CharacterEvents.To_GetTarget, null);
                }
                else
                {
                    direction.Normalize();
                    Debug.WriteLine("mSpeed*dt :"+mSpeed*dt);
                    direction *= Convert.ToSingle(Math.Min(mSpeed * dt, distance));

                    parent.X += direction.X;
                    parent.Y += direction.Y;
                    //Debug.WriteLine("Parent posn {0} {1}", parent.X, parent.Y);
                }
            }
        }

        class WalkState : LocomotionState
        {

            public WalkState() 
            {
                mSpeed = 30.0f;
#if !(WINDOWS_PHONE&& !SILVERLIGHT)
                
                {
                    mSpeed = 45.0f;
                }
#else
                mSpeed = 30.0f;
#endif

            }

            public override void Activate(object eventData)
            {
                
                Debug.WriteLine("Walk State");
                
                Vec2 target = eventData as Vec2;
#if (WINDOWS_PHONE &&!SILVERLIGHT)
                float speed=(Parent as CharacterController).WalkSpeed;
                if ( speed>0)
                {
                    mSpeed = speed;
                }
                ; 
#endif                
                if (target == null)
                {
                    Debug.WriteLine("No event data");
                }
                else
                {
                    mTarget = target;

                    CharacterController controller = (this.Parent as CharacterController);
                    IRoadCharacter parent = controller.Parent as IRoadCharacter;
                    Vec2 direction = mTarget - new Vec2(parent.X, parent.Y);
                    Debug.WriteLine("direction length "+direction.Length());
                    if (direction.Length() < 0.01)
                    {
                        // at target
                        controller.HandleEvent((int)CharacterController.CharacterEvents.To_GetTarget, null);
                    }
                    else
                    {

                        direction.Normalize();
                        ///*
                        if (direction.X > 0.99f)
                        {
                            parent.AnimController.PlayAnimation( RoadAnimationNames.kWalkRight,  MirrorMode.None);
                        }
                        else if (direction.X < -0.99f)
                        {
                            parent.AnimController.PlayAnimation(  RoadAnimationNames.kWalkRight,   MirrorMode.Horizontal);
                        }
                        else if (direction.Y > 0.99f)
                        {
                            parent.AnimController.PlayAnimation(  RoadAnimationNames.kWalkBack,    MirrorMode.None);
                        }
                        else if (direction.Y < -0.99f)
                        {
                            parent.AnimController.PlayAnimation(  RoadAnimationNames.kWalkForward , MirrorMode.None);
                        }
                         //*/
                    }
                }
            }

        }

        class RunState : LocomotionState
        {
            RunState() { mSpeed = 2.0f; }

            public override void Update(float dt)
            {

            }
        }

        class SpawnInState : State
        {
            public override void Activate(object eventData)
            {
                Debug.WriteLine("Spawn In State");
                
                CharacterController controller = (this.Parent as CharacterController);
                IRoadCharacter parent = controller.Parent as IRoadCharacter;
#if (WINDOWS_PHONE &&!SILVERLIGHT)
                parent.Visibility = true;
                controller.mDead = false;
#endif
                parent.AnimController.StartAnimation(RoadAnimationNames.kSpawnIn, MirrorMode.None);
            }

            public override void Update(float dt)
            {
                CharacterController controller = (this.Parent as CharacterController);
                IRoadCharacter parent = controller.Parent as IRoadCharacter;

                // when parent anim is done, transition to GetTarget
                if (parent.AnimController.IsLastFrame)
                {
                    controller.HandleEvent((int)CharacterController.CharacterEvents.To_GetTarget, null);
                }

            }
        }

        
        class SpawnOutState : State
        {
            public override void Activate(object eventData)
            {
                CharacterController controller = (this.Parent as CharacterController);
                IRoadCharacter parent = controller.Parent as IRoadCharacter;
                Debug.WriteLine("Spawn Out State");
                 
                // adding simple offsets to shift character to proper position so that 
                // animation lines up with holes.
                parent.Y -= 4;
                parent.X += 2;
                parent.AnimController.StartAnimation(RoadAnimationNames.kSpawnOut, MirrorMode.None);
            }
            public override void Deactivate()
            {
                CharacterController controller = (this.Parent as CharacterController);
                IRoadCharacter parent = controller.Parent as IRoadCharacter;
                parent.Y += 4;
                parent.X -= 2;
           }

            public override void Update(float dt)
            {
                CharacterController controller = (this.Parent as CharacterController);
                IRoadCharacter parent = controller.Parent as IRoadCharacter;

                // when parent anim is done, transition to GetTarget
                if (parent.AnimController.IsLastFrame)
                {
                    controller.HandleEvent((int)CharacterController.CharacterEvents.To_Dead, null);
#if (WINDOWS_PHONE &&  !SILVERLIGHT)
if (parent.CardinalDirection==CardinalPoint.N)
                    {    
                    AnimationManager.Instance.CountAliveCharactersPlayer1--;
                    }
                    else
                    {
                    AnimationManager.Instance.CountAliveCharactersPlayer2--;
                    }
#endif
                }
            }
        }

        class EatCarrotState : State
        {
            public override void Activate(object eventData)
            {
                CharacterController controller = (this.Parent as CharacterController);
                IRoadCharacter parent = controller.Parent as IRoadCharacter;
                parent.AnimController.StartAnimation(RoadAnimationNames.kEatCarrot, MirrorMode.None);
            }

            public override void Update(float dt)
            {
                CharacterController controller = (this.Parent as CharacterController);
                IRoadCharacter parent = controller.Parent as IRoadCharacter;

                // when parent anim is done, transition to GetTarget
                if (parent.AnimController.IsLastFrame)
                {
                    parent.IsFed = true;
                    controller.BuildReturnPath();
                    controller.HandleEvent((int)CharacterController.CharacterEvents.To_GetTarget, null);
                }
            }
        }
        class EatFlowerState : State
        {
            public override void Activate(object eventData)
            {
                CharacterController controller = (this.Parent as CharacterController);
                IRoadCharacter parent = controller.Parent as IRoadCharacter;
                parent.AnimController.StartAnimation(RoadAnimationNames.kEatFlower, MirrorMode.None);
            }

            public override void Update(float dt)
            {
                CharacterController controller = (this.Parent as CharacterController);
                IRoadCharacter parent = controller.Parent as IRoadCharacter;

                // when parent anim is done, transition to GetTarget
                if (parent.AnimController.IsLastFrame)
                {
                    parent.IsFed = true;
                    controller.BuildReturnPath();
                    controller.HandleEvent((int)CharacterController.CharacterEvents.To_GetTarget, null);
                }
            }
        }

        class WinDanceState : State
        {
            public override void Activate(object eventData)
            {
                CharacterController controller = (this.Parent as CharacterController);
                IRoadCharacter parent = controller.Parent as IRoadCharacter;
                
                parent.AnimController.StartAnimation(RoadAnimationNames.kWinDance, MirrorMode.None);
            }
            public override void Update(float dt)
            {
            }
        }
        class LostState : State
        {
            public override void Activate(object eventData)
            {
                Debug.WriteLine("Lost State");
                
                CharacterController controller = (this.Parent as CharacterController);
                IRoadCharacter parent = controller.Parent as IRoadCharacter;
                parent.AnimController.StartAnimation(RoadAnimationNames.kIdle, MirrorMode.None);
            }
            public override void Update(float dt)
            {
            }
        }

        class DeadState : State
        {
            // notify parent
            public override void Activate(object eventData)
            {
                Debug.WriteLine("Dead State");
                CharacterController controller = (this.Parent as CharacterController);
                IRoadCharacter parent = controller.Parent as IRoadCharacter;
                
                controller.Dead = true;
#if (WINDOWS_PHONE&&!SILVERLIGHT)
                parent.Visibility = false;
                parent.IsFed = false;
#endif 

                // maybe disable parent anim

            }

            public override void Update(float dt)
            {

            }

        }
        #endregion
#if (WINDOWS_PHONE && !SILVERLIGHT)
public enum CharacterEvents { To_Walk = 0, To_Run, To_GetTarget, TimerElapsed, To_Idle, To_SpawnIn,To_SpawnOut, To_EatCarrot,To_EatFlower,To_Dead, To_WinDance, To_Lost }

#else
        public enum CharacterEvents { To_Walk = 0, To_Run, To_GetTarget, TimerElapsed, To_Idle, To_SpawnOut, To_EatCarrot,To_EatFlower,To_Dead, To_WinDance, To_Lost }
#endif

        public IRoadCharacter Parent { get; set; }
#if (WINDOWS_PHONE && !SILVERLIGHT)
float mWalkSpeed;
        public float WalkSpeed
        {
            get
            {
                return mWalkSpeed;
            }
            set
            {
                mWalkSpeed = value;
            }
        }
#endif
        
        StateMachine mStateMachine = new StateMachine();
        Queue<Vec2> mTargets = new Queue<Vec2>();
        List<Vec2> mReverseTargets = new List<Vec2>();
        bool mDead = false;
        public bool Dead { get { return mDead; } set { mDead = value; } }

        // gets next target
        public Vec2 GetNextTarget()
        {
            if (mTargets.Count > 0)
            {
                Vec2 target = mTargets.Dequeue();
                return target;
            }
            else
            {
                return null;
            }
        }
        public void BuildReturnPath()
        {
            for (int i = 0; i < mReverseTargets.Count; i++) 
            {
                mTargets.Enqueue(mReverseTargets[mReverseTargets.Count - 1-i]);
            }
        }

        public CharacterController()
        {
            InitializeStateMachine();
        }

        /// <summary>
        /// Initialize controlling state machine
        /// </summary>
        private void InitializeStateMachine()
        {
            //states
            IdleState idleState = new IdleState();
            idleState.Parent = this;
            mStateMachine.AddState(idleState);

            WalkState walkState = new WalkState();
            walkState.Parent = this;
            mStateMachine.AddState(walkState);

            /*
            RunState runState = new RunState();
            runState.Parent = this;
            mStateMachine.AddState(runState);
            */

            GetWalkTarget getWalkTargetState = new GetWalkTarget();
            getWalkTargetState.Parent = this;
            mStateMachine.AddState(getWalkTargetState);
                
            SpawnInState spawnInState = new SpawnInState();
            spawnInState.Parent = this;
            mStateMachine.AddState(spawnInState);

            SpawnOutState spawnOutState = new SpawnOutState();
            spawnOutState.Parent = this;
            mStateMachine.AddState(spawnOutState);

            DeadState deadState = new DeadState();
            deadState.Parent = this;
            mStateMachine.AddState(deadState);

            EatCarrotState eatCarrotState = new EatCarrotState();
            eatCarrotState.Parent = this;
            mStateMachine.AddState(eatCarrotState);


            EatFlowerState eatFlowerState = new EatFlowerState();
            eatFlowerState.Parent = this;
            mStateMachine.AddState(eatFlowerState);

            WinDanceState winDanceState = new WinDanceState();
            winDanceState.Parent = this;
            mStateMachine.AddState(winDanceState);
            LostState lostState = new LostState();
            lostState.Parent = this;
            mStateMachine.AddState(lostState);
            //transitions
            mStateMachine.AddTransition((int)CharacterEvents.To_Walk, idleState, walkState);
            mStateMachine.AddTransition((int)CharacterEvents.To_GetTarget, idleState, getWalkTargetState);

            mStateMachine.AddTransition((int)CharacterEvents.To_Idle, walkState, idleState);
            mStateMachine.AddTransition((int)CharacterEvents.To_GetTarget, walkState, getWalkTargetState);

            mStateMachine.AddTransition((int)CharacterEvents.To_Walk, getWalkTargetState, walkState);
            mStateMachine.AddTransition((int)CharacterEvents.To_Idle, getWalkTargetState, idleState);

            mStateMachine.AddTransition((int)CharacterEvents.To_GetTarget, spawnInState, getWalkTargetState);

            mStateMachine.AddTransition((int)CharacterEvents.To_SpawnOut, getWalkTargetState, spawnOutState);
            mStateMachine.AddTransition((int)CharacterEvents.To_EatCarrot, getWalkTargetState, eatCarrotState);
            mStateMachine.AddTransition((int)CharacterEvents.To_EatFlower, getWalkTargetState, eatFlowerState);


            mStateMachine.AddTransition((int)CharacterEvents.To_Dead, spawnOutState, deadState);

            mStateMachine.AddTransition((int)CharacterEvents.To_GetTarget, eatCarrotState, getWalkTargetState);

            mStateMachine.AddTransition((int)CharacterEvents.To_GetTarget, eatFlowerState, getWalkTargetState);

            mStateMachine.AddTransition((int)CharacterEvents.To_WinDance, getWalkTargetState, winDanceState);
            mStateMachine.AddTransition((int)CharacterEvents.To_WinDance, idleState, winDanceState);
            mStateMachine.AddTransition((int)CharacterEvents.To_WinDance, walkState, winDanceState);
            mStateMachine.AddTransition((int)CharacterEvents.To_WinDance, spawnInState, winDanceState);
            mStateMachine.AddTransition((int)CharacterEvents.To_WinDance, spawnOutState, winDanceState);
            mStateMachine.AddTransition((int)CharacterEvents.To_WinDance, eatFlowerState, winDanceState);
            mStateMachine.AddTransition((int)CharacterEvents.To_WinDance, eatCarrotState, winDanceState);
            mStateMachine.AddTransition((int)CharacterEvents.To_Lost, idleState, lostState);
            mStateMachine.AddTransition((int)CharacterEvents.To_Lost, walkState, lostState);
            mStateMachine.AddTransition((int)CharacterEvents.To_Lost, getWalkTargetState, lostState);
            mStateMachine.AddTransition((int)CharacterEvents.To_Lost, spawnInState, lostState);
            mStateMachine.AddTransition((int)CharacterEvents.To_Lost, spawnOutState, lostState);
            mStateMachine.AddTransition((int)CharacterEvents.To_Lost, eatFlowerState, lostState);
            mStateMachine.AddTransition((int)CharacterEvents.To_Lost, eatCarrotState, lostState);
#if (WINDOWS_PHONE && !SILVERLIGHT)
 mStateMachine.AddTransition((int)CharacterEvents.To_SpawnIn, deadState, spawnInState);
#endif
            //mStateMachine.AddTransition((int)CharacterEvents.To_Run, walkState, runState);
            //mStateMachine.AddTransition((int)CharacterEvents.TimerElapsed, runState, walkState);

            // character starts in a spawn in state
#if (WINDOWS_PHONE && !SILVERLIGHT)
            mStateMachine.StartState = deadState;
#else
            mStateMachine.StartState = spawnInState;
#endif
        }

        /// <summary>
        /// Sets the navigation path for a character
        /// </summary>
        /// <param name="path"></param>
        public void SetNavigationPath(Queue<Vec2> path)
        {
            mTargets.Clear();
            mReverseTargets.Clear();
            while (path.Count > 0)
            {
                Vec2 pathNode=path.Dequeue();
                mTargets.Enqueue(pathNode);
                mReverseTargets.Add(pathNode);
            }
        }

    
        public void Activate()
        {
            mStateMachine.Activate(null);
        }

        public void Activity(float dt, BoardController boardController)
        {
            mStateMachine.Activity(dt);
        }

        public void HandleEvent(int controllerEvent, object eventData)
        {
            mStateMachine.QueueEvent(controllerEvent, eventData);
        }

    }
#if false
        /// <summary>
        /// 
        /// </summary>
        private void InitializeNavigationPath()
        {
            //TODO - given my position, get a navigation path from the AI mgr.

            Vec2 position = new Vec2(Parent.X, Parent.Y);


            Debug.WriteLine("initial posn "+ position.X+ " "+ position.Y);

            // for now, random points
            for (int i = 0; i < 5; i++)
            {

                float length = Convert.ToSingle(Math.Round(20.0f + mRandom.NextDouble()* 50.0f));

                float direction = Convert.ToSingle(Math.Round(mRandom.NextDouble() * 4.0f));

                Vec2 target = null;

                if (direction < 2.0f)
                {

                    if (direction < 1.0f)
                    {
                        target = new Vec2(length, 0);
                    }
                    else
                    {
                        target = new Vec2(0, length);
                    }

                }
                else
                {
                    if (direction < 3.0f)
                    {
                        target = new Vec2(-length, 0);
                    }
                    else
                    {
                        target = new Vec2(0, -length);
                    }
                }

                target += position;

                position = target;

                Debug.WriteLine("Direction "+ direction);

                Debug.WriteLine("Target "+ target.X+" " +target.Y);

                mTargets.Enqueue(target);
            }
        }

    public class FirstCharacterController
    {
        const float kFlowSpeed = 27;
        float mSpeed = kFlowSpeed;
        int mRow;
        int mColumn;
        int mBoardSize;

        bool mReachHalfSquare = false;
        
        int mOriginRow;
        int mOriginColumn;
        float mOriginPosition;
        //float mRefX;
        //float mRefY;
        CardinalPoint mOriginRoadDirection;

        bool mArrivedAtDestination = false;

        Random mRandom = new Random();

        CardinalPoint mRoadDirection;

        float mSquareLocation = 0;

        public IRoadCharacter Parent { get; set; }

        
        public FirstCharacterController(int row, int column,
            CardinalPoint roadDirection,
            float squarePosition,
            int boardSize,
            bool reachHalfSquare)
        {
            mRow = row;
            mBoardSize = boardSize;
            mOriginRow = row;
            mOriginColumn = column;
            mOriginPosition = squarePosition;
            mOriginRoadDirection = roadDirection;
            mReachHalfSquare = reachHalfSquare;
            mRow = mOriginRow;
            mColumn = mOriginColumn;
            mSquareLocation = mOriginPosition;
            mRoadDirection = mOriginRoadDirection;
            
        }

       

        public void Activate()
        {
            
        }

        public void Activity(float dt, BoardController boardController)
        {
            UpdateBehavior(dt, boardController);
            StepPosition(dt);
        }

        public void HandleEvent(int controllerEvent, object eventData)
        {
            
        }

        /// <summary>
        /// Move gopher
        /// </summary>
        /// <param name="dt"></param>
        private void StepPosition(float dt)
        {
            float dPosition;
#if DEBUG 
            if (mSpeed > 0)
            {
                dPosition = 1f;
            }
            else 
            {
                dPosition = 0;
            }
#else
            dPosition = (float) (dt * mSpeed);
#endif
            mSquareLocation = mSquareLocation + dPosition;





            // some really funky integration scheme
            switch (mRoadDirection)
            {
                case CardinalPoint.N:
                    Parent.Y += dPosition;
                    break;
                case CardinalPoint.S:

                    Parent.Y -= dPosition;
                    break;
                case CardinalPoint.W:
                    Parent.X -= dPosition;

                    break;
                case CardinalPoint.E:
                    Parent.X += dPosition;
                    break;
            }
        }

        /// <summary>
        /// Perform any transitioning
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="boardController"></param>
        private void UpdateBehavior(float dt, BoardController boardController)
        {

            Parent.AnimController.PlayAnimation(RoadAnimationNames.kIdle, MirrorMode.None);

            if (!mArrivedAtDestination)
            {
                if (mSquareLocation > 0.5f * Square.kPixelSize &&
                    !(mReachHalfSquare))
                {
                    //      Debug.WriteLine("Handle Half Square " + " Position " + roadCharacter.Position + " Row " + roadCharacter.Row + " Column " + roadCharacter.Column + "Direction " + roadCharacter.RoadDirection); 
                    HandleMiddleSquareCourse(boardController);
                    mReachHalfSquare = true;
                }
                else if (mSquareLocation > Square.kPixelSize)
                {
                    //        Debug.WriteLine("Handle End Square " + " Position " + roadCharacter.Position + " Row " + roadCharacter.Row + " Column " + roadCharacter.Column + "Direction " + roadCharacter.RoadDirection); 
                    HandleEndSquareCourse(boardController);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roadCharacter"></param>
        private void HandleEndSquareCourse( BoardController boardController)
        {
            int nextRow = mRow;
            int nextColumn = mColumn;
            CardinalPoint roadDirection = mRoadDirection;
            CardinalPoint oppositeCardinalPoint = (CardinalPoint)Enum.ToObject(typeof(CardinalPoint), (((int)(mRoadDirection) + 2) % 4));
            CardinalPoint targetDirection = mOriginRoadDirection;
            switch (roadDirection)
            {
                case CardinalPoint.N:
                    nextRow += 1;
                    break;
                case CardinalPoint.S:
                    nextRow -= 1;
                    break;
                case CardinalPoint.E:
                    nextColumn += 1;
                    break;
                case CardinalPoint.W:
                    nextColumn -= 1;
                    break;
            }

            if ((nextRow < 0) || (nextRow > (boardController.Size - 1)) ||
                (nextColumn > (boardController.Size - 1)) || nextColumn < 0)
            {
                if ((nextRow < 0 && targetDirection == CardinalPoint.S) ||
                    (nextRow > (boardController.Size - 1) &&
                    targetDirection == CardinalPoint.N) ||
                    (nextColumn > (boardController.Size - 1) &&
                    targetDirection == CardinalPoint.E) ||
                    nextColumn < 0 &&
                    targetDirection == CardinalPoint.W)
                {
                    mSpeed = 0f;
                    mArrivedAtDestination = true;
                  }
                else
                {
                    mRoadDirection = oppositeCardinalPoint;
                }
            }
            else
            {
                mReachHalfSquare = false;
                mSquareLocation = 0;
                if (!boardController.HasSquareExit(nextRow, nextColumn, oppositeCardinalPoint))
                {
                    mRoadDirection = oppositeCardinalPoint;
                }
                else
                {
                    mRow = nextRow;
                    mColumn = nextColumn;
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="character"></param>
        private void HandleMiddleSquareCourse( BoardController boardController)
        {
            int row = mRow;
            int column = mColumn;
            bool[] squareExitStatus = new bool[4];
            bool noExit = true;
            int randomCardinalPoint = mRandom.Next(0, 4) - 1;
            CardinalPoint targetDirection = mOriginRoadDirection;
            CardinalPoint currentDirection = mRoadDirection;

            // Cardinal Point{ N, W, S, E }
            for (int cardinalPoint = 0; cardinalPoint < 4; cardinalPoint++)
            {
                squareExitStatus[cardinalPoint] = boardController.HasSquareExit(row, column, (CardinalPoint)Enum.ToObject(typeof(CardinalPoint), cardinalPoint));
            }

            if (squareExitStatus[(int)targetDirection] && ((int)currentDirection != ((int)targetDirection + 2) % 4))
            {
                mRoadDirection = targetDirection;
                noExit = false;
            }
            else if (squareExitStatus[(int)mRoadDirection] && ((int)currentDirection != ((int)targetDirection + 2) % 4))
            {
                noExit = false;
            }
            else
            {
                for (int cardinalPoint = 0; cardinalPoint < 4; cardinalPoint++)
                {
                    randomCardinalPoint += 1;
                    if (randomCardinalPoint == 4)
                    {
                        randomCardinalPoint = 0;
                    }

                    if (randomCardinalPoint != (((int)(currentDirection) + 2) % 4) && (randomCardinalPoint != ((int)targetDirection + 2) % 4))
                    {
                        if (squareExitStatus[randomCardinalPoint])
                        {
                            mRoadDirection = (CardinalPoint)Enum.ToObject(typeof(CardinalPoint), randomCardinalPoint);
                            noExit = false;
                            cardinalPoint = 3;
                        }
                    }
                }
            }

            if (noExit)
            {
                if (squareExitStatus[(((int)targetDirection + 2) % 4)])
                {
                    mRoadDirection = (CardinalPoint)Enum.ToObject(typeof(CardinalPoint), (((int)(targetDirection) + 2) % 4));
                }
                else
                {
                    mRoadDirection = (CardinalPoint)Enum.ToObject(typeof(CardinalPoint), (((int)(currentDirection) + 2) % 4));
                }
            }
        }
    }

# region DebugCharacter
    /// <summary>
    /// Debug Char controller
    /// stays put, transitions between anims
    /// has no states
    /// </summary>
    public class DebugCharacterController : ICharacterController
    {

        float mTime;
        enum DebugState { Idle, Walk_Left, Walk_Right, Run_Left, Run_Right, Run_Forward, Run_Back }
        DebugState mState = DebugState.Idle;

        public IRoadCharacter Parent { get; set; }

        public DebugCharacterController()
        {

        }

        public void Activate() { }

        bool mDead;
        public bool Dead { get { return mDead; } set { mDead = value; } }

        public void Activity(float dt, BoardController boardController)
        {
            if (mTime >= 1.0f)
            {
                mTime = 0;

                mState = (DebugState) (((int) mState + 1) % 7);

                switch (mState)
                {
                    case DebugState.Idle:
                        Parent.AnimController.PlayAnimation(RoadAnimationNames.kIdle, MirrorMode.None);
                        break;
                    case DebugState.Walk_Left:
                        Parent.AnimController.PlayAnimation(RoadAnimationNames.kWalkRight, MirrorMode.None);
                        break;
                    case DebugState.Walk_Right:
                        Parent.AnimController.PlayAnimation(RoadAnimationNames.kWalkRight, MirrorMode.Horizontal);
                        break;
                    case DebugState.Run_Left:
                        Parent.AnimController.PlayAnimation(RoadAnimationNames.kRunRight, MirrorMode.None);
                        break;
                    case DebugState.Run_Right:
                        Parent.AnimController.PlayAnimation(RoadAnimationNames.kRunRight, MirrorMode.Horizontal);
                        break;
                    case DebugState.Run_Forward:
                        Parent.AnimController.PlayAnimation(RoadAnimationNames.kRunForward, MirrorMode.None);
                        break;
                    case DebugState.Run_Back:
                        Parent.AnimController.PlayAnimation(RoadAnimationNames.kRunBack, MirrorMode.None);
                        break;

                }

            }
            else
            {
                 mTime += dt;
            }

        }

        public void SetNavigationPath(Queue<Vec2> path) { }

        public void HandleEvent(int controllerEvent, object eventData)
        { }

    }
#endregion
#endif
    }
