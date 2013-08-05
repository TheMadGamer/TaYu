using System;
using System.Collections.Generic;

using DeltaCommon.Entities;

namespace DeltaCommon.Events
{
    public class GameEvent
    {
        public enum GameEventType { 
            PLACE_DOMINO, 
            ROTATE_DOMINO, 
            SCORE, 
            SCORING_POSITION,
            FAIL_PLACE, 
            TEST_SPAWN,
            GAME_OVER_WIN,
            GAME_OVER_LOSE,
            BOOM1,
            BOOM2,TOGGLE_MUTE

        }
        
        GameEventType mEventType;
        public GameEventType EventType { get { return mEventType;}}

        IDomino mDomino;
        public IDomino WhichDomino { get { return mDomino; } }

        int mPlayerIndex;
        public int PlayerIndex { get { return mPlayerIndex;}}

        public GameEvent(GameEventType eventType, IDomino whichDomino, int playerIndex)
        {
            mEventType = eventType;
            mDomino = whichDomino;
            mPlayerIndex = playerIndex;
        }

    }
}
