using System;
using System.Collections.Generic;

using DeltaCommon.Component;

namespace DeltaCommon.Entities
{
    
    public enum CardinalPoint { N, W, S, E }

    /* There are 28 different types of dominoes in the game
     * A domino is made of 3 Squares which ID is -1,0 or 1 (From left to right) 
     * Each Domino contains 3 Water exits that we define by the Square ID they belong to and
     * their Position: North, West, South or East.
     */

    public struct WaterExit
    {
        public int mSquareID;
        public CardinalPoint mCardinalPoint;
    }

    /// <summary>
    /// graphical highlight state
    /// Active - current active dominio
    /// FinishedPlacement - quick flash after placement 
    /// </summary>
    public enum HighLightMode { None, Active, FinishedPlacement };

    public interface IDomino
    {
        DominoController Controller { get; }

        void Destroy();

        void MoveNorth();
        void MoveEast();
        void MoveSouth();
        void MoveWest();

        void MoveCounterClockWise();
        void MoveClockWise();

        void UpdateDominoLocation(int size);

        void SetHighlight(HighLightMode mode);

    }
}
