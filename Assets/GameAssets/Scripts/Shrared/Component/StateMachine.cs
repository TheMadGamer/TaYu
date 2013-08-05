using System;
using System.Collections;
using System.Collections.Generic;
//using System.Collections.Specialized;
using System.Diagnostics;

namespace DeltaCommon.Component
{

    public class StateMachine
    {

        List<State> mStates = new List<State>();

        Dictionary<State, Dictionary<int, State> > mTransitionTable =
            new Dictionary<State, Dictionary<int, State> >();

        State mActiveState = null;

        // basic event structure
        class StateEvent
        {
            public StateEvent(int id, object data)
            {
                mEventID = id;
                mEventData = data;
            }

            public int mEventID;
            public object mEventData;
        }

        Queue<StateEvent> mEventQueue = new Queue<StateEvent>();


        State mStartState = null;
        public State StartState
        {
            get { return mStartState; }
            set { mStartState = value; }
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void AddState(State state)
        {
            mStates.Add(state);

            mTransitionTable.Add(state, new Dictionary<int, State>());
        }

        public String GetState()
        {
            return mActiveState.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="fromState"></param>
        /// <param name="toState"></param>
        public void AddTransition(int eventID, State fromState, State toState)
        {
            mTransitionTable[fromState].Add(eventID, toState);
        }

        
        public void Activate(object eventData)
        {
            if (mStartState != null)
            {
                mActiveState = mStartState;
            }
            else if(mStates.Count > 0)
            {
                mActiveState = mStates[0];
            }
            if (mActiveState != null)
            {
                mActiveState.Activate(eventData);
            }
        }

        public void Activity(float dt)
        {
            if(mActiveState == null)
            {
                return;
            }

            while (mEventQueue.Count > 0)
            {
                StateEvent stateEvent = mEventQueue.Dequeue();

                // can we transition
                if (mTransitionTable[mActiveState].ContainsKey(stateEvent.mEventID))
                {
                    mActiveState.Deactivate();

                    mActiveState = mTransitionTable[mActiveState][stateEvent.mEventID];

                    // dequeue any remaining events, invalid
                    mEventQueue.Clear();

                    mActiveState.Activate(stateEvent.mEventData);

                    // activation may enque new events allowing double transition
                    // do not simply break
                }
                else
                {
                    Debug.WriteLine("WTF, event not handled");
                }
            }


            if (mActiveState != null)
            {
                mActiveState.Update(dt);
            }
        }

        /// <summary>
        /// Something happened, queue for next update
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="eventData"></param>
        public void QueueEvent( int eventID, object eventData)
        {
            mEventQueue.Enqueue(new StateEvent(eventID, eventData));
        }

    }


    /// <summary>
    /// Methods: Activate, Deactivate, Update
    /// </summary>
    public abstract class State
    {
        public object Parent { get; set; }

        public virtual void Activate(object eventData) { }

        public virtual void Deactivate() { }

        public abstract void Update(float dt);
    }

}
