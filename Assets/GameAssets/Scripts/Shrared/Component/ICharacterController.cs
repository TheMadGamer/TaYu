using System;
using System.Collections.Generic;

using DeltaCommon.Entities;

namespace DeltaCommon.Component
{
    /// <summary>
    /// Properties: IRoadCharacter
    /// 
    /// Methods: Activity,HandleEvent, SetNavigationPath,Activate
    /// </summary>
    public interface ICharacterController
    {
        IRoadCharacter Parent { get; set; }

        void Activity(float dt, BoardController boardController);

        void HandleEvent(int characterEvent, object eventData);

        void SetNavigationPath(Queue<Vec2> path);

        // has the character spawned out - can be removed
        bool Dead { get; set; }
#if (WINDOWS_PHONE && !SILVERLIGHT)
        float WalkSpeed { get; set; }
#endif
        void Activate();
    }
}
