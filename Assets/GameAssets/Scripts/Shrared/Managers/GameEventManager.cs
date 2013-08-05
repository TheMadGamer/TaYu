using System;
using System.Collections.Generic;

using DeltaCommon.Events;

namespace DeltaCommon.Managers
{
    // basic interfact for system event listener
    public interface IEventListener
    {
        bool HandleEvent(GameEvent gameEvent);
    }

    public class GameEventManager
    {
        #region SINGLETON
        private static GameEventManager sInstance;
        public static GameEventManager Instance { get { return sInstance;}}
        public static void Initialize() { sInstance = new GameEventManager();}
        public static void Unload()
        {
            if (sInstance != null)
            {
                sInstance.Destroy();
                sInstance = null;
            }
        }
        #endregion

        Queue<GameEvent> mEventQueue = new Queue<GameEvent>();
        List<IEventListener> mEventListeners = new List<IEventListener>();

        public void RaiseEvent(GameEvent gameEvent)
        {
            mEventQueue.Enqueue(gameEvent);
        }

        /// <summary>
        /// Add an event listener
        /// </summary>
        /// <param name="listener"></param>
        public void AddListener(IEventListener listener)
        {
            mEventListeners.Add(listener);
        }

        public void Activity()
        {
            foreach (GameEvent gameEvent in mEventQueue)
            {
                foreach (IEventListener listener in mEventListeners)
                {
                    if (listener.HandleEvent(gameEvent))
                    {
                        // lister consumed event
                        break;
                    }
                }         
            }
            mEventQueue.Clear();
        }

        public void Destroy() 
        {
            mEventQueue.Clear();
            mEventListeners.Clear();
        }
    }

}
