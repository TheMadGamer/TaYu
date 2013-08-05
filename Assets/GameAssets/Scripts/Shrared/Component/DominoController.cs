using System;
using System.Collections.Generic;

using DeltaCommon.Entities;

namespace DeltaCommon.Component
{
    /// <summary>
    /// Domino logic controller
    /// </summary>
    public class DominoController
    {
        public IDomino Parent { get; set; }

        // domino's water exists
        // keep this hidden 
        WaterExit[] mWaterExits;

        // keep this around to do delayed graphics initialization
        int mLabel;

        /// <summary>
        /// Defines the index into list of possible dominoes
        /// </summary>
        /// <returns></returns>
        public int Label
        {
            get { return mLabel; }

        }


        int mRotationState;
        public int RotationState
        {
            get { return mRotationState; }
            set { mRotationState = value; }
        }

        /// <summary>
        /// Row position on board
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Column position on board
        /// </summary>
        public int Column { get; set; }

        public DominoController(int label, WaterExit[] exits, int startPosition)            
        {
            mWaterExits = exits;
            mLabel = label;
            Column = startPosition;
            Row = startPosition;
            mRotationState = 0;
        }

        public DominoController(DominoController controler) 
        {
            mWaterExits = controler.mWaterExits;
            mLabel = controler.Label;
            Column = controler.Column;
            Row = controler.Row;
            mRotationState = controler.RotationState;
        }

        public bool IsHorizontal()
        {
            return mRotationState == 0 || mRotationState == 2;
        }

        public bool IsVertical()
        {
            return mRotationState == 1 || mRotationState == 3;
        }

        public int GetRow(int squareID)
        {
            int row = Row;

            // translate to row, column positions
            switch (RotationState)
            {
                case 1:
                    row += squareID;
                    break;

                case 3:
                    row -= squareID;
                    break;

            }
            return row;
        }

        public int GetColumn(int squareID)
        {
            int column = Column;

            // translate to row, column positions
            switch (RotationState)
            {
                case 0:
                    column += squareID;
                    break;

                case 2:
                    column -= squareID;
                    break;
            }
            return column;
        }




        ///<summary>
        /// this allows the current domino to index into the precomputed 
        /// water exits 
        ///</summary>
        public int GetWaterExitSquareID(int exitNumber)
        {
            return mWaterExits[exitNumber].mSquareID;
        }

        public CardinalPoint GetWaterExitCardinalPoint(int exitNumber)
        {
            return mWaterExits[exitNumber].mCardinalPoint;
        }

        /// <summary>
        /// Given a square id and a direction, is there a water exit present.
        /// (ie square -1, direction north)
        /// </summary>
        /// <param name="squareID"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private bool HasWaterExit(int squareID, CardinalPoint direction)
        {


            return (mWaterExits[0].mCardinalPoint == direction
                    && mWaterExits[0].mSquareID == squareID)
                || (mWaterExits[1].mCardinalPoint == direction
                    && mWaterExits[1].mSquareID == squareID)
                || (mWaterExits[2].mCardinalPoint == direction
                    && mWaterExits[2].mSquareID == squareID);
        }

        /// <summary>
        /// Given a domino in world space (rotated), is there an exit in a given direction
        /// </summary>
        /// <param name="squareID"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool HasWorldSpaceWaterExit(int squareID, CardinalPoint direction)
        {
            int worldDirection = (int)direction;
            worldDirection -= mRotationState;
            worldDirection = worldDirection % 4;
            if (worldDirection < 0)
            {
                worldDirection += 4;
            }

            return HasWaterExit(squareID,
                (CardinalPoint)Enum.ToObject(typeof(CardinalPoint), worldDirection));

        }


    }
}
