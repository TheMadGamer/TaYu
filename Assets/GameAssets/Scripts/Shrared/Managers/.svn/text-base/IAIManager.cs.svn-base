using System;
using System.Collections.Generic;
using System.Reflection;

using DeltaCommon.Component;
using DeltaCommon.Events;
using DeltaCommon.Entities;


namespace DeltaCommon.Managers
{
    public abstract class IAIManager : IEventListener
    {
        // singleton
        #region SINGLETON
        static IAIManager sInstance = null;
        // singleton
        public static IAIManager Instance { get { return sInstance; } }

        public static void Initialize(BoardController board, Type managerType) 
        { 
            ConstructorInfo[] ctors = managerType.GetConstructors();
            object[] args = new object[1];
            args[0] = board;

            sInstance = ctors[0].Invoke(args) as IAIManager; 
        }
        public static void Unload() { sInstance = null; }

        #endregion        

        // gets a path
        public abstract void GetPath(Vec2 source, Vec2 target, Queue<Vec2> path);
        public abstract void GetPathWithInsideViaPoint(Vec2 source, Vec2 target, Queue<Vec2> path, int playerIndex, bool spawnLower);

        public virtual bool HandleEvent(GameEvent gameEvent) { return true; }

        // gets a random point on the board
        public virtual Vec2 GetRandomPoint(){ return new Vec2();}
    }
}
