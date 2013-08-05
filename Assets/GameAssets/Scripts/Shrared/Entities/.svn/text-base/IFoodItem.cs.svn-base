using System;
using System.Collections.Generic;

using DeltaCommon.Component;

namespace DeltaCommon.Entities
{
    public interface IFoodItem
    {
        int Index { get; }
#if !( WINDOWS_PHONE && !SILVERLIGHT)
        IAnimationController AnimController { get; }

		#endif
        void Destroy();

        void Hide();

        void Respawn();
        
    }
}