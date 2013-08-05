#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;

using DeltaCommon.Managers;
using DeltaCommon.Component;


#endregion

namespace DeltaCommon.Entities
{
    interface IAIBot
    {
        bool GetNextMove(BoardController board, List<IDomino> myDominoes, List<IDomino> enemyDominoes,  bool computerIsPlayer1, out IDomino playedDomino);
    }

    // performs random moves
    class RandomBot : IAIBot
    {
#if DEBUG
        Random mRandom = new Random(0);
#else
        Random mRandom = new Random();
#endif

        public bool GetNextMove(BoardController board, List<IDomino> myDominoes, List<IDomino> enemyDominoes,
            bool computerIsPlayer1, out IDomino playedDomino)
        {
            DominoController domino=null;
            playedDomino = null;
            int minRow = Math.Max(0, board.BoundingBox.RowMin - 2);
            int maxRow = Math.Min(board.Size, board.BoundingBox.RowMax + 3);
            int minColumn = Math.Max(0, board.BoundingBox.ColumnMin - 2);
            int maxColumn = Math.Min(board.Size, board.BoundingBox.ColumnMax + 3);

            if (!( GamePlayManager.Instance.FirstDominoSet))
            {
                minRow = board.StartPosition;
                maxRow = board.StartPosition + 1;
                minColumn = board.StartPosition;
                maxColumn = board.StartPosition + 1;
            }
        
            //Pick Random Domino in the Bag
            int dominoIndex = mRandom.Next(0, myDominoes.Count)-1;
            for (int index = 0; index < myDominoes.Count; index++)
            {
                dominoIndex++;
                if (dominoIndex == myDominoes.Count) 
                {
                    dominoIndex = 0;
                }
                domino = myDominoes[dominoIndex].Controller;
                playedDomino = domino.Parent;
        //        Debug.WriteLine("index of Domino in Bag " + dominoIndex);
                
                //Save Position in case No Position Available for this Domino
                int rowInBag=domino.Row;
                int columnInBag=domino.Column;
                int rotationInBag = domino.RotationState;

                // start in a random location
                domino.Row = mRandom.Next(minRow, maxRow) - 1;
                domino.Column = mRandom.Next(minColumn, maxColumn) - 1;
                domino.RotationState = mRandom.Next(0, 4) - 1;

                // brute force search
                for (int row = 0; row < (maxRow - minRow); row++)
                {
                    domino.Row++;
                    if (domino.Row == maxRow)
                    {
                        domino.Row = minRow;
                    }

                    for (int column = 0; column < (maxColumn - minColumn); column++)
                    {
                        domino.Column++;
                        if (domino.Column == maxColumn)
                        {
                            domino.Column = minColumn;
                        }

                        for (int rotationState = 0; rotationState < 4; rotationState++)
                        {
                            domino.RotationState++;
                            if (domino.RotationState == 4)
                            {
                                domino.RotationState = 0;
                            }
                            if (GamePlayManager.Instance.IsLegalMove(domino))
                            {
                                return true;
                            }
                        }
                    }
                }
                //Put Domino Back in the Bag if No Position Available
                domino.Row=rowInBag;
                domino.Column=columnInBag;
                domino.RotationState = rotationInBag;

            }
            return false;
        }
    }



    /// <summary>
    ///  Look Ahead
    /// </summary>
    class LookAheadBot : IAIBot
    {
        int mComputerDepth;

#if DEBUG
        Random mRandom = new Random(0);
#else
        Random mRandom = new Random();
#endif

        /// <summary>
        /// Alpha beta  bot
        /// </summary>
        /// <param name="searchDepth"></param>
        public LookAheadBot(int searchDepth )
        {
            mComputerDepth = searchDepth;
        }


        /// <summary>
        /// Gets the next move
        /// </summary>
        /// <param name="board"></param>
        /// <param name="myDominoes"></param>
        /// <param name="enemyDominoes"></param>
        /// <param name="computerIsPlayer1"></param>
        /// <param name="playedDomino"></param>
        /// <returns> true on success</returns>
        public bool GetNextMove(BoardController board, List<IDomino> myDominoes, List<IDomino> enemyDominoes, 
            bool computerIsPlayer1, out IDomino playedDomino)
        {
            int optimalRow = 0;
            int optimalColumn = 0;
            int optimalRotation = 0;
            AlphaBeta(board, int.MinValue, int.MaxValue, mComputerDepth, true, computerIsPlayer1,
                ref optimalRow, ref optimalColumn, ref optimalRotation, myDominoes, enemyDominoes,out playedDomino);
            playedDomino.Controller.Row = optimalRow;
            playedDomino.Controller.Column = optimalColumn;
            playedDomino.Controller.RotationState = optimalRotation;
            return true;
        }



        /// <summary>
        /// Alpha Beta search for optimal move
        /// </summary>
        /// <param name="board"></param>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="depth"></param>
        /// <param name="maximizeScore"></param>
        /// <param name="player1Playing"></param>
        /// <param name="myDominoes"></param>
        /// <param name="enemyDominoes"></param>
        /// <returns></returns>
        public int AlphaBeta(BoardController board, int alpha, int beta, int depth,
            bool maximizeScore, bool player1Playing, List<IDomino> myDominoes, List<IDomino> enemyDominoes) 
        {
            int optimalRow=0;
            int optimalColumn = 0;
            int optimalRotation = 0;
            IDomino playedDomino=null;
            return AlphaBeta(board, alpha, beta, depth, maximizeScore, player1Playing, ref optimalRow, ref optimalColumn, ref optimalRotation, myDominoes, enemyDominoes, out playedDomino);
        }


        /// <summary>
        ///  Alpha Beta search for optimal move
        ///  
        /// </summary>
        /// <param name="board"></param>
        /// <param name="domino"></param>
        /// <param name="mAlpha"></param>
        /// <param name="beta"></param>
        /// <param name="depth">depth to search - 0 is base case</param>
        /// <param name="maxDepth">maximize vs. minimize</param>
        /// <returns></returns>
       public int AlphaBeta(BoardController board, int alpha, int beta, int depth, 
            bool maximizeScore, bool player1Playing,
            ref int optimalRow, ref int optimalCol, ref int optimalRotation, List<IDomino> myDominoes, List<IDomino> enemyDominoes,
           out IDomino playedDomino)
        {

           playedDomino = null;

           // if we're at depth zero , just return board value
           if (depth == 0)
           {
#if TESTALPHABETA
               Debug.WriteLine("alphaBeta depth 0 Calculate Board Value");
#endif
               return player1Playing ? -board.GetBoardValue() : board.GetBoardValue();
           }
 
           // search bounds
            int minRow,maxRow,minColumn,maxColumn;

           //For the First Move only Domino Rotation is allowed;
            if (!(GamePlayManager.Instance.FirstDominoSet))
            {
                minRow = board.StartPosition;
                maxRow = board.StartPosition + 1;
                minColumn = board.StartPosition;
                maxColumn = board.StartPosition + 1;
            }
            else 
            {
                minRow = Math.Max(0, board.BoundingBox.RowMin - 2);
                maxRow = Math.Min(board.Size, board.BoundingBox.RowMax + 3);
                minColumn = Math.Max(0, board.BoundingBox.ColumnMin - 2);
                maxColumn = Math.Min(board.Size, board.BoundingBox.ColumnMax + 3);
            }

            // first time this is called, we'll use this to restore
            int boardRowMin = board.BoundingBox.RowMin;
            int boardRowMax = board.BoundingBox.RowMax;
            int boardColumnMin = board.BoundingBox.ColumnMin;
            int boardColumnMax = board.BoundingBox.ColumnMax;
      
            
            // counts number of follow on moves (if zero, we're at a leaf)
            int positionsAvailable = 0;


#if TESTALPHABETA
            if (depth==mComputerDepth)
            {
                Debug.WriteLine("Start Main Alpha Beta : depth "+ mComputerDepth);
            }
            else if (depth>0)
            {
                Debug.WriteLine("");
                Debug.WriteLine("Start recursive alpha beta depth " + depth);
                Debug.WriteLine("myDominoes");
                foreach (IDomino idomino in myDominoes)
                {
                    if (!(idomino == null))
                    {
                        Debug.WriteLine("  Domino row " + idomino.Controller.Row + " Column " + idomino.Controller.Column + " label " + idomino.Controller.Label);
                    }
                }
                Debug.WriteLine("Ennemy Dominoes");
                foreach (IDomino idomino in enemyDominoes)
                {
                    if (!(idomino == null))
                    {
                        Debug.WriteLine("  Domino row " + idomino.Controller.Row + " Column " + idomino.Controller.Column + " label " + idomino.Controller.Label);
                    }
                }
                if (maximizeScore)
                {
                    Debug.WriteLine("Max Node " + "beta " + beta + " alpha" + alpha);
                }
                else
                {
                    Debug.WriteLine("Min Node " + "beta " + beta + " alpha" + alpha);
                }
            }
           
#endif
           //state of bag

           
           //Pick Random Domino in the Bag
            // start in a random location
 
           int dominoIndex = mRandom.Next(0, myDominoes.Count)-1;
           int randomStartRow = mRandom.Next(minRow, maxRow) - 1;
           int randomStartColumn = mRandom.Next(minColumn, maxColumn) - 1;
           int randomStartRotationState = mRandom.Next(0, 4) - 1;
           DominoController domino;
            
           for (int index = 0; index < myDominoes.Count; index++)
            {
               dominoIndex++;
                if (dominoIndex == myDominoes.Count)
                {
                    dominoIndex = 0;
                }
               domino = myDominoes[dominoIndex].Controller;
#if TESTALPHABETA
               Debug.WriteLine("");
               Debug.WriteLine("Check Domino Label "+domino.Label+ " depth "+depth+  " computer depth " +mComputerDepth);
#endif
               domino.Row = randomStartRow;
               domino.Column = randomStartColumn;
               domino.RotationState = randomStartRotationState;

                // perform a grid search (row, col, rotation)
                for (int row = 0; row < (maxRow - minRow); row++)
                {
                    domino.Row++;
                    if (domino.Row == maxRow)
                    {
                        domino.Row = minRow;
                    }
                    for (int column = 0; column < (maxColumn - minColumn); column++)
                    {
                        domino.Column++;
                        if (domino.Column == maxColumn)
                        {
                            domino.Column = minColumn;
                        }
                        for (int rotationState = 0; rotationState < 4; rotationState++)
                        {
                            domino.RotationState++;
                            if (domino.RotationState == 4)
                            {
                                domino.RotationState = 0;
                            }
//                            Debug.WriteLine("Domino Index"+dominoIndex+" Label"+domino.Label+" Row "+ domino.Row+ " Column "+domino.Column+ " RotationState"+ domino.RotationState);

                            // if we can place a domino here, then we'll
                            // begin the recursive search
                            if (GamePlayManager.Instance.IsLegalMove(domino))
                            {
 #if TESTALPHABETA
                                Debug.WriteLine("AlphaBetaDepth"+depth +"("+mComputerDepth+")");
#endif
                                // marks this as a possible move
                                positionsAvailable++;
#if TESTALPHABETA
                                if (positionsAvailable ==2)
                                {
                                    rotationState = 4;
                                    row = maxRow - minRow;
                                    column = maxColumn - minColumn;
                                    index = myDominoes.Count;
                                }
#endif
                                // place the domino on the board
                                board.AddDomino(domino);
                                myDominoes.Remove(domino.Parent);

                                if (maximizeScore)
                                {
                                    // this is the recursive call 
                                    int score = AlphaBeta(board,  alpha, beta, (depth - 1), !maximizeScore, player1Playing,enemyDominoes,myDominoes);
#if TESTALPHABETA
                                    Debug.WriteLine("Score" + score);
#endif
                                    // prune
                                    if (score >= beta)
                                    {  
#if TESTALPHABETA
                                        Debug.WriteLine("mScore >= beta: End AlphaBeta with Depth" + depth);
                                        Debug.WriteLine("CUTOFF");
                                        Debug.WriteLine("");
#endif
                                        // undoes the move on the board and bag
                                        myDominoes.Insert(dominoIndex, domino.Parent);
                                        board.PopUndo(domino); // 
                                        return beta;
                                    }

                                    if (score > alpha)
                                    {
                                        alpha = score;
#if TESTALPHABETA
                                        Debug.WriteLine("New alpha " + alpha);
#endif
                                        if (depth == mComputerDepth)
                                        {
#if TESTALPHABETA
                                            Debug.WriteLine("New Optimal Domino: Label "+domino.Label+" Row "+domino.Row+" Column "+domino.Column+" RotationState "+ domino.RotationState);
#endif                                  
                                            optimalRow = domino.Row;
                                            optimalCol = domino.Column;
                                            optimalRotation = domino.RotationState;
                                            playedDomino = domino.Parent;
                                        }
                                    }
                                }
                                else
                                {
                                    // this is the recursive call 
                                    int score = AlphaBeta(board,  alpha, beta, (depth - 1), !maximizeScore, player1Playing,enemyDominoes,myDominoes);
#if TESTALPHABETA
                                    Debug.WriteLine("Score" + score);
#endif
                                    // prune
                                    if (score <= alpha)
                                    {
#if TESTALPHABETA
                                        Debug.WriteLine("mScore<=alpha End AlphaBeta with Depth" + depth);
                                        Debug.WriteLine("CUTOFF");
                                        Debug.WriteLine("");
#endif                                                                //// undoes the move on the board
                                        myDominoes.Insert(dominoIndex, domino.Parent);
                                        board.PopUndo(domino);
                                        return alpha;
                                    }
                                    if (score < beta)
                                    {
                                        beta = score;
#if TESTALPHABETA          

                                        Debug.WriteLine("New beta " + beta);
#endif
                                        if (depth == mComputerDepth)
                                        {
                                            optimalRow = domino.Row;
                                            optimalCol = domino.Column;
                                            optimalRotation = domino.RotationState;
                                            playedDomino =domino.Parent;
                                        }
                                    }                                    
                                }
                                // restore board state
                                myDominoes.Insert(dominoIndex,domino.Parent);
                                board.PopUndo(domino);
                            }
                        }
                    }
                }
#if TESTALPHABETA
                Debug.WriteLine(positionsAvailable + " Positions Available: try another Domino in the Bag "); 
#endif
           }
       
            // restore the board's row min/max state
            board.BoundingBox.RowMin = boardRowMin;
            board.BoundingBox.RowMax = boardRowMax;
            board.BoundingBox.ColumnMin = boardColumnMin;
            board.BoundingBox.ColumnMax = boardColumnMax;
   
            
            // leaf case
            if (positionsAvailable == 0)
            {
#if TESTALPHABETA

                Debug.WriteLine("No Position Available (Leaf Case)" );
                Debug.WriteLine("Board Value" + board.GetBoardValue());
                Debug.WriteLine("End AlphaBeta with Depth" + depth);
                //TODO                domino.UpdateDominoLocation();
                Debug.WriteLine("");
                Debug.WriteLine("");
#endif
                
                return player1Playing ? -board.GetBoardValue() : board.GetBoardValue();
            }
            else
            {
#if TESTALPHABETA
                Debug.WriteLine(positionsAvailable+" Positions Available ");
                if (mComputerDepth == depth)
                {
                    Debug.WriteLine("End Main AlphaBeta with Depth" + depth);
                }
                else
                {
                    Debug.WriteLine("End Recursive AlphaBeta with Depth" + depth);
                }
                if (maximizeScore)
                {
                    Debug.WriteLine("return Max alpha "+alpha);
                }
                else 
                {
                    Debug.WriteLine("return Min Beta "+beta);
                }
#endif
                // return alpha value
                return maximizeScore ? alpha : beta;
            }
        }
    }
    class LookAheadBotWithRandom : IAIBot 
    {
        RandomBot mRandomBot = new RandomBot();
        LookAheadBot mLookAheadBot;
        int mComputerDepth;
        int mRandomIndice;
#if DEBUG
        Random mRandom = new Random(0);
#else
        Random mRandom = new Random();
#endif
        public LookAheadBotWithRandom(int searchDepth,int randomIndice )
        {
            mComputerDepth = searchDepth;
            mRandomIndice = randomIndice;
            mLookAheadBot = new LookAheadBot(searchDepth);
        }
        public bool GetNextMove(BoardController board, List<IDomino> myDominoes, List<IDomino> enemyDominoes,
         bool computerIsPlayer1, out IDomino playedDomino)
        {
            int mBot = mRandom.Next(0, mRandomIndice);
            if (mBot == 0)
            {
                Debug.WriteLine("Random Play");
                return mRandomBot.GetNextMove(board, myDominoes, enemyDominoes, computerIsPlayer1, out playedDomino);
            }
            else
            {
                Debug.WriteLine("LookAhead Play");
                return mLookAheadBot.GetNextMove(board, myDominoes, enemyDominoes, computerIsPlayer1, out playedDomino);
            }
        }
    }
}
