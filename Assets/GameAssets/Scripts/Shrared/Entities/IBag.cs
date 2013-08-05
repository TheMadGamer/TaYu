using System;
using System.Collections.Generic;
namespace DeltaCommon.Entities
{
    public abstract class IBag 
    {
        protected IDomino[] mDominoes;

        protected int mNumSlots;
        protected int mBoardSize;

        protected bool mLayoutHorizontal = true;

        protected int kHorizontalRowOffset = 2;
        protected int kVerticalColumnOffset = -5;

        public IBag(int nSlots, int boardSize, bool player1Bag)
        {
            mBoardSize = boardSize;

            if (player1Bag)
            {
                mLayoutHorizontal = true;
            }
            else
            {
                mLayoutHorizontal = false;
            }

            mNumSlots = nSlots;

            mDominoes = new IDomino[nSlots];
        }

        public void RemoveDomino(IDomino domino)
        {
            for (int i = 0; i < mDominoes.Length; i++)
            {
                if (mDominoes[i] == domino)
                {
                    mDominoes[i] = null;
                    break;
                }
            }
        }

        // temp debug route - for drawing first domino for computer
        public IDomino RemoveDomino()
        {
            for (int i = 0; i < mDominoes.Length; i++)
            {
                if (mDominoes[i] != null)
                {
                    return mDominoes[i];
                }
            }
            return null;
        }

        //for Undo Moves

        public void DestroyDomino(int label)
        {
            for (int i = 0; i < mDominoes.Length; i++)
            {
                if (mDominoes[i].Controller.Label == label)
                {
                    mDominoes[i] = null;
                    break;
                }
            }
            
        }


        public bool HasEmptySlot()
        {
            for (int i = 0; i < mDominoes.Length; i++)
            {
                if (mDominoes[i] == null)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddDomino(IDomino domino)
        {
            AddDomino(domino, false);
        }

        // adds a drawn domino
        public void AddDomino(IDomino domino, bool disablePlacementAnim)
        {
            int emptySlot = 0;

            domino.Controller.RotationState = 0;
            if (!disablePlacementAnim)
            {
                domino.SetHighlight(HighLightMode.FinishedPlacement);
            }

            for (int i = 0; i < mDominoes.Length; i++)
            {
                if (mDominoes[i] == null)
                {
                    emptySlot = i;
                    break;
                }
            }

            if (mLayoutHorizontal)
            {
#if WINDOWS_PHONE
                domino.Controller.Row = mBoardSize + kHorizontalRowOffset;
                domino.Controller.Column = 3 + emptySlot * 4;
                domino.UpdateDominoLocation(mBoardSize);

#else
                domino.Controller.Row = mBoardSize + kHorizontalRowOffset;
                domino.Controller.Column = 4 + emptySlot * 4;
                domino.UpdateDominoLocation(mBoardSize);
#endif
            }
            else
            {
#if WINDOWS_PHONE

                domino.Controller.Row = -1- kHorizontalRowOffset;
                domino.Controller.Column = 3 + emptySlot * 4;
                domino.UpdateDominoLocation(mBoardSize);

#else
                domino.Controller.Column = kVerticalColumnOffset;
                domino.Controller.Row = emptySlot * 4 + 4;
                domino.UpdateDominoLocation(mBoardSize);
#endif
            }
            mDominoes[emptySlot] = domino;
        }


        public bool HasDomino(IDomino findDomino)
        {
            foreach (IDomino domino in mDominoes)
            {
                if (domino == findDomino)
                {
                    return true;
                }
            }
            return false;
        }

        public List<IDomino> GetDominoes()
        {
            List<IDomino> dominoes = new List<IDomino>();

            dominoes.InsertRange(0, mDominoes);

            return dominoes;
        }

        public abstract void Activity();
        
    }
}
