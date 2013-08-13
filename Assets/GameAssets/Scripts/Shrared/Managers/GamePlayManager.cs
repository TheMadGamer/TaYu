#region Using

using UnityEngine;

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using DeltaCommon.Entities;
using DeltaCommon.Component;

#endregion

namespace DeltaCommon.Managers
{
    public class GamePlayManager
    {

        #region SINGLETON
        // this is for a singleton pattern
        static private GamePlayManager sInstance;
        static public GamePlayManager Instance { get { return sInstance; } }
        static public void Initialize()
        {
            sInstance = new GamePlayManager();


        }
        #endregion
        #region VARIABLES, PROPERTIES
        public int BoardSize
        {
            get
            {
                if (mBoardController != null)
                {
                    return mBoardController.Size;
                }
                else
                {
                    UnityEngine.Debug.Log("Should not get here, board should be initialized by now.");
                    return 15;
                }
            }
        }

        //variable to keep track of the game Status
        // todo - this should be an enum not a bunch of bools
        bool mGameOver;
        public bool GameOver 
        {
            get 
            { 
                return mGameOver;
            }
#if DEBUG
            set {  mGameOver=value; }
#endif
        }

        bool mPlayer1Playing;
        public bool Player1Playing { get { return mPlayer1Playing; } }

        bool mFreeMove;
        public bool FreeMove { get { return mFreeMove; } }

        bool mPickNewDomino;
        public bool PickNewDomino
        {
            get { return mPickNewDomino; }

#if DEBUG
            set
            {
                mPickNewDomino = value;
            }
#endif
        }

        bool mHumanPlayer;
        public bool HumanPlayer
        {
            get { return mHumanPlayer; }
#if true
            set
            {
                mHumanPlayer = value;
            }
#endif

        }

        bool mFirstDominoSet;
        public bool FirstDominoSet
        {
            get
            {
                return mFirstDominoSet;
            }
#if DEBUG
            set
            {
                mFirstDominoSet = value;
            }
#endif
        }

        public bool GameOnHold { get; set; }

        // is the computer bot playing as player1
        bool mPlayer1Computer = false;
        public bool Player1Computer
        {
            get
            {
                return mPlayer1Computer;
            }
            set
            {
                mPlayer1Computer = value;
            }
        }

        int mComputerPlayer1Level = 0;
        public int ComputerPlayer1Level
        {
            get
            {
                return mComputerPlayer1Level;
            }
            set
            {
                mComputerPlayer1Level = value;
            }
        }


        // is the computer bot playing as player2
        bool mPlayer2Computer = false;
        public bool Player2Computer
        {
            get
            {
                return mPlayer2Computer;
            }
            set
            {
                mPlayer2Computer = value;
            }
        }

        int mComputerPlayer2Level = 0;
        public int ComputerPlayer2Level
        {
            get
            {
                return mComputerPlayer2Level;
            }
            set
            {
                mComputerPlayer2Level = value;
            }
        }

        // Interface to our AI Bot
        IAIBot[] mAIBotLevel = new IAIBot[3];

        DominoGenerator mDominoGenerator;
        BoardController mBoardController;
		GameObject mDominoPrefab;

        #endregion
        
        private GamePlayManager(){ }

        public void Init(DominoGenerator generator, BoardController controller, GameObject dominoPrefab)
        {
            mAIBotLevel[0] = new LookAheadBotWithRandom(1,3); 
            mAIBotLevel[1] = new LookAheadBot(1);
            mAIBotLevel[2] = new LookAheadBot(3);            
            mDominoGenerator = generator;
            mFirstDominoSet = false;
            mFreeMove = false;
            mPlayer1Playing = false;
            mPickNewDomino = false;
            mGameOver = false;
            mBoardController = controller;
			mDominoPrefab = dominoPrefab;
        }

        #region Domino Movements Functions

        //Move Domino to the West. Move not possible for the First Domino
        //Have to Check that the Domino will still be in the limits of the Board after the Move

        public void MoveDominoWest(  IDomino domino)
        {
            if ((mFirstDominoSet) && (domino.Controller.IsHorizontal() && (domino.Controller.Column > 1)
                    || (domino.Controller.IsVertical() && (domino.Controller.Column > 0))))
            {
                domino.MoveWest();
            }
        }

        //Move Domino to the East. Move not possible for the First Domino
        //Have to Check that the Domino will still be in the limits of the Board after the Move
        public void MoveDominoEast(  IDomino domino)
        {
            if ((mFirstDominoSet) &&
                ((domino.Controller.IsHorizontal() && domino.Controller.Column < (mBoardController.Size-2))
                  || ((domino.Controller.IsVertical() && domino.Controller.Column < (mBoardController.Size-1)))))
            {
                domino.MoveEast();
            }
        }

        //Move Domino to the South. Move not possible for the First Domino
        //Have to Check that the Domino will still be in the limits of the Board after the Move
        public void MoveDominoSouth(  IDomino domino)
        {
            if ((mFirstDominoSet) &&
                ((domino.Controller.IsHorizontal() && (domino.Controller.Row > 0))
                      || (domino.Controller.IsVertical() && (domino.Controller.Row >1))))
            {
                domino.MoveSouth();
            }
        }
        ///<summary>
        ///Move Domino to the North. Move not possible for the First Domino
        ///Have to Check that the Domino will still be in the limits of the Board after the Move
        ///</summary>
        public void MoveDominoNorth(  IDomino domino )
        {
            if ((mFirstDominoSet) &&
                ((domino.Controller.IsHorizontal() && (domino.Controller.Row < mBoardController.Size-1))
                   || (domino.Controller.IsVertical() && (domino.Controller.Row < mBoardController.Size-2))))
            {
                domino.MoveNorth();
            }
        }

        ///<summary>
        ///Rotate the domino counter clockwise
        ///if domino remains in the board after the move
        ///</summary>
        public void MoveDominoCounterClockWise( IDomino domino )
        {
           // if ((domino.Controller.Row>0)&&(domino.Controller.Row<(mBoardController.Size-1))
             //   &&(domino.Controller.Column>0)&&(domino.Controller.Column<(mBoardController.Size-1)))
            //{
                domino.MoveCounterClockWise();
            //}
        }


        ///<summary>
        ///Rotate the domino counter clockwise
        ///if domino remains in the board after the move
        ///</summary>
        public void MoveDominoClockWise(IDomino domino)
        {
            if ((domino.Controller.Row > 0) && (domino.Controller.Row < (mBoardController.Size - 1))
                && (domino.Controller.Column > 0) && (domino.Controller.Column < (mBoardController.Size - 1)))
            {
                domino.MoveClockWise();
            }
        }
        #endregion


        public bool IsLegalMove(DominoController domino)
        {
            return  (!mFirstDominoSet) || mBoardController.IsLegalMove(domino);
        }

        public void PlaceDomino(DominoController domino)
        {
            if (!mFirstDominoSet)
            {
                mFirstDominoSet = true;
            }

            mPickNewDomino = true;
            GameOnHold = true;
            mBoardController.AddDomino(domino);
        }

        /// <summary>
        /// Is this domino in a scoring position
        /// </summary>
        /// <param name="domino"></param>
        /// <returns></returns>
        public bool DominoScoresPlayer1(DominoController domino)
        {
            if (domino.IsHorizontal())
            {
                if (domino.Row == 0)
                {
                    for (int col = -1; col < 2; col++)
                    {
                        if (mBoardController.HasSquareExit(0, domino.Column + col, CardinalPoint.S))
                        {
                            return true;
                        }
                    }
                }
                else if (domino.Row == mBoardController.Size - 1)
                {
                    for (int col = -1; col < 2; col++)
                    {
                        if (mBoardController.HasSquareExit(domino.Row, domino.Column + col, CardinalPoint.N))
                        {
                            return true;
                        }
                    }
                }
            }
            else //Domino Vertical
            {
                if ((domino.Row == 1 && (mBoardController.HasSquareExit(0, domino.Column, CardinalPoint.S))))
                {
                    return true;
                }
                else if ((domino.Row == (mBoardController.Size - 2) && (mBoardController.HasSquareExit(mBoardController.Size - 1, domino.Column, CardinalPoint.N))))
                {
                    return true;    
                }
            }
            return false;
        }

        /// <summary>
        /// Is this domino in a scoring position
        /// </summary>
        /// <param name="domino"></param>
        /// <returns></returns>
        public bool DominoScoresPlayer2(DominoController domino)
        {
            if (domino.IsHorizontal())
            {
                if ((domino.Column == 1 &&
                    (mBoardController.HasSquareExit(domino.Row, 0, CardinalPoint.W))))
                {
                    return true;
                }
                else if ((domino.Column == (mBoardController.Size - 2) &&
                    (mBoardController.HasSquareExit(domino.Row, mBoardController.Size - 1, CardinalPoint.E))))
                {
                    return true;
                }
            }
            else //Domino Vertical
            {
                if (domino.Column == 0)
                {
                    for (int row = -1; row < 2; row++)
                    {
                        if (mBoardController.HasSquareExit(domino.Row + row, 0, CardinalPoint.W))
                        {
                            return true;
                        }
                    }
                }
                else if (domino.Column == mBoardController.Size - 1)
                {
                    for (int row = -1; row < 2; row++)
                    {
                        if (mBoardController.HasSquareExit(domino.Row + row, mBoardController.Size - 1, CardinalPoint.E))
                        {
                            return true;
                        }
                    }
                }

            }
            return false;
        }


        /// <summary>
        /// When a random Domino has been selected, have to check if there is 
        /// an available fitting Position  on the Board.
        /// If not, the Game will be over.
        /// The idea is to try all possible position on the Board for the domino for all value of Row,Column and Rotation State. 
        /// We limit the search area with the value of min/max dominos coordinates.
        /// We will use the AlphaBeta function of the Alpha Beta algorithm
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="domino"></param>
        public void CheckForGameOver(  List<IDomino> dominoes)
        {
            mFreeMove = true;
            mGameOver = true;
         
            //Check if there is space for scoring Domino

            if (!mBoardController.IsSpaceScoringDomino())
            { 
                return; 
            }
            
            foreach (IDomino domino in dominoes)
            {
                if (mBoardController.HasALegalMove(domino.Controller))
                {
                    // game is not over if a legal move found
                    mFreeMove = false;
                    mGameOver = false;
#if TESTALPHABETA
                    UnityEngine.Debug.Log("Legal move found, game continues");
#endif
                    return;
                }
            }
            
            UnityEngine.Debug.Log("No legal move found, game over");
        }

        
        
        public void CheckForHintDomino(List<IDomino> dominoes, out int labelID, out int row, out int column, out int rotation)
        {
            labelID = 0;
            row = 0;
            column = 0;
            rotation = 0;
            foreach (IDomino domino in dominoes)
            {
                if (domino != null)
                {
                    for (int r = 0; r < mBoardController.Size; r++)
                    {
                        for (int c = 0; c < mBoardController.Size; c++)
                        {
                            for (int rot = 0; rot < 4; rot++)
                            {
                                domino.Controller.Row = r;
                                domino.Controller.Column = c;
                                domino.Controller.RotationState = rot;
                                if (mBoardController.IsLegalMove(domino.Controller))
                                {
                                    labelID = domino.Controller.Label;
                                    row = r;
                                    column = c;
                                    rotation = rot;
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    UnityEngine.Debug.Log("Active Domino should not have empty slot");
                }
            }
            UnityEngine.Debug.Log("No legal move found!! WTH");
        }



        public void DoneWithDraw()
        {
            mPickNewDomino = false;
        }


        #region Computer AI

        /// <summary>
        /// Ask the computer to make a move
        /// </summary>
        /// <param name="board"></param>
        /// <param name="domino"></param>
        public IDomino ComputersPlay(  List<IDomino> computerList, List<IDomino> enemyList)
        {
            IDomino playedDomino = null;
            if (Player1Playing)
            {
#if TESTALPHABETA
                UnityEngine.Debug.Log("Computer Player 1 Level {0}",mComputerPlayer1Level);
#endif
                if (!mAIBotLevel[mComputerPlayer1Level].GetNextMove(mBoardController, computerList, enemyList, Player1Playing, out playedDomino) )
                {
                    UnityEngine.Debug.Log("AI Search failed");
                }
            }
            else
            {
#if TESTALPHABETA
                UnityEngine.Debug.Log("Computer Player 2 Level " + mComputerPlayer2Level);
#endif
                if (!mAIBotLevel[mComputerPlayer2Level].GetNextMove(mBoardController, computerList, enemyList, Player1Playing, out playedDomino) )
                {
                    UnityEngine.Debug.Log("AI Search failed");
                }
            }
            // add the domino to the board
            mBoardController.AddDomino(playedDomino.Controller);
            mPickNewDomino = true;
            GameOnHold = true;
            if (!mFirstDominoSet)
            {
                mFirstDominoSet = true;
            }
            return playedDomino;
        }


        ///<summary>
        /// Change player when one player's turn is over.
        ///</summary>
        public void ChangePlayer()
        {
            if (Player1Playing)
            {                
                mPlayer1Playing = false;
                mHumanPlayer = !mPlayer2Computer;
            }
            else
            {
                mPlayer1Playing = true;
                mHumanPlayer = !mPlayer1Computer;
            }
        }

        /// <summary>
        /// Undoes a move
        /// Restores board to previous state
        /// should only be used in player/computer mode?
        /// </summary>
        /// <param name="board"></param>
        /// <param name="domino"></param>
        public void UndoMove( DominoController domino)
        {
        //    mDominoGenerator.GetPreviousLabel();
        //    mDominoGenerator.GetPreviousLabel();
            mBoardController.PopUndo(domino);
        }

        public GameObject GetNextDomino()
        {
            return mDominoGenerator.GetNextDomino();
        }

        public int GetNextLabel()
        {
            return mDominoGenerator.GetNextLabel();
        }

        public int GetPreviousLabel()
        {
            return mDominoGenerator.GetPreviousLabel();
        }

        public int NumDominoesPlayed()
        {
            return mDominoGenerator.CountDominoPlayed;
        }
        #endregion

        public GameObject CreateDomino(int labelId)
        {
            return mDominoGenerator.CreateDomino(labelId);
        }

#if DEBUG
        
        public void ResetLabelIndex()
        {
            mDominoGenerator.ResetLabelIndex();
        }

        public void SetExplicitLabel(int index, int value)
        {
            mDominoGenerator.SetExplicitLabel(index, value);
        }

#endif
    }
}

