using System;
using System.Collections.Generic;
using System.Diagnostics;

using DeltaCommon.Entities;


namespace DeltaCommon.Component
{
    public class BoardController
    {
        // allows access back to parent container (board)
        public IBoard Parent { get; set; }

        int mSize;
        public int Size { get { return mSize; } }
        public int StartPosition { get { return mSize / 2; } }

        //Relative coordinates of 2 external squares when a domino is rotated 
        int[,] kRotationStateRelativeCoordinates = new int[,]{
            {0,-1},
            {-1,0},
            {0,1},
            {1,0}
        };

        //This variable will allow to decrase the portion of the Board 
        //to Check for availble Domino Position
        BBox mBBox;
        public BBox BoundingBox { get { return mBBox; } }

        #region EXITS STACK
        // The Game Board is composed of squares.
        //
        // this is the private, internal representation of the board
        private Square[,] mGameGrid;

#if DEBUG
        public Square[,] GameGrid 
        {
            get 
            {
                return mGameGrid;
            }
            set 
            {
                mGameGrid = value;
            }
        }
#endif

        /// <summary>
        /// Points on a grid
        /// ...almost as good as snakes on a plane.
        /// </summary>
        public class GridPoint
        {
            static float TransformToWorldSpace(int rowColumnCoordinate, float boardSize)
            {
                return (-(boardSize / 2) + rowColumnCoordinate) * Square.kPixelSize + Square.kPixelSize / 2.0f;
            }

            public int mRow;
            public int mColumn;

            public GridPoint(int r, int c)
            {
                mRow = r;
                mColumn = c;
            }

            public GridPoint(GridPoint p)
            {
                mRow = p.mRow;
                mColumn = p.mColumn;
            }

            public Vec2 TransformToWorldSpace(float boardSize)
            {
                return new Vec2(TransformToWorldSpace(mColumn, boardSize), TransformToWorldSpace(mRow, boardSize));
            }


            public override int GetHashCode()
            {
                return mRow ^ mColumn;
            }

            public override bool Equals(System.Object obj)
            {    
                // If parameter is null return false.
                if (obj == null)
                {
                    return false;
                }

                // If parameter cannot be cast to Point return false.
                GridPoint p = obj as GridPoint;
                if ((System.Object)p == null)
                {
                    return false;
                }
                return p.mRow == mRow && p.mColumn == mColumn;
            }

            public bool Equals(GridPoint p)
            {
                // If parameter is null return false:
                if ((object)p == null)
                {
                    return false;
                }

                return p.mRow == mRow && p.mColumn == mColumn;
            }

            public static bool operator ==(GridPoint p0, GridPoint p1)
            {
                // If both are null, or both are same instance, return true.
                if (System.Object.ReferenceEquals(p0, p1))
                {
                    return true;
                }

                // If one is null, but not both, return false.
                if (((object)p0 == null) || ((object)p1 == null))
                {
                    return false;
                }

                return Equals(p0, p1);
            }

            public static bool operator !=(GridPoint p0, GridPoint p1)
            {
                // If both are null, or both are same instance, return true.
                if (System.Object.ReferenceEquals(p0, p1))
                {
                    return false;
                }

                // If one is null, but not both, return false.
                if (((object)p0 == null) && ((object)p1 == null))
                {
                    return true;
                }

                return ! Equals(p0, p1);
            }
        }

        // this is a circular undo stack
        // it should remain private
        private OpenExitStack mOpenExitStack;


        /// <summary>
        /// circular undo stack for the open exits
        /// </summary>
        class OpenExitStack
        {
            int mExitsIndex = 0;
            const int kMaxExits = 10;

            private List<int[, ,]> mExitsList = new List<int[, ,]>();

            int mBoardSize = 0;

            public OpenExitStack(int size)
            {
                mBoardSize = size;
                int[, ,] openExits = new int[2, 4, size];

                mExitsList.Add(openExits);

                for (int i = 0; i < size; i++)
                {
                    //for North and East Exit
                    openExits[0, 0, i] = -1;
                    openExits[1, 0, i] = -1;
                    openExits[0, 3, i] = -1;
                    openExits[1, 3, i] = -1;

                    //for West and South Exit
                    openExits[0, 1, i] = size;
                    openExits[1, 1, i] = size;
                    openExits[0, 2, i] = size;
                    openExits[1, 2, i] = size;
                }
            }

            public int[, ,] GetOpenExists()
            {
                return mExitsList[mExitsIndex];
            }

            /// <summary>
            /// Open Exit Stack: pops the top of the stack
            /// </summary>
            public void PopOpenExits()
            {
                mExitsIndex--;
                if (mExitsIndex < 0)
                {
                    mExitsIndex = kMaxExits - 1;
                }
                //Console.WriteLine("Pop open exits {0}", mExitsIndex);
            }

            /// <summary>
            /// Open Exit Stack: copies current open exists, pushes onto stack
            /// </summary>
            public void PushOpenExits()
            {
                if (mExitsIndex + 1 < kMaxExits)
                {
                    // add a new item
                    if (mExitsList.Count == mExitsIndex + 1)
                    {
                        mExitsList.Add(new int[2, 4, mBoardSize]);
                    }
                    mExitsIndex++;
                    CopyOpenExitsArray(mExitsList[mExitsIndex], mExitsList[mExitsIndex - 1]);
                }
                else
                {
                    mExitsIndex = 0;
                    CopyOpenExitsArray(mExitsList[mExitsIndex], mExitsList[kMaxExits - 1]);
                }
            }



            /// <summary>
            /// Open Exit Stack:  copy an Open Exit Array
            /// </summary>
            void CopyOpenExitsArray(int[, ,] mCopyOpenExitsArray, int[, ,] mOpenExitsArray)
            {
                for (int i = 0; i < 2; i++)
                {
                    for (int CardPoint = 0; CardPoint < 4; CardPoint++)
                    {
                        for (int RowCol = 0; RowCol < mBoardSize; RowCol++)
                        {
                            mCopyOpenExitsArray[i, CardPoint, RowCol] = mOpenExitsArray[i, CardPoint, RowCol];
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Creates a new copy of the open exits
        /// </summary>
        public void PushUndo()
        {
            mOpenExitStack.PushOpenExits();
        }

        /// <summary>
        ///  undoes a move
        ///  unmarks squares
        ///  sets the open exists to their previous state
        /// </summary>
        /// <param name="domino"></param>
        public void PopUndo(DominoController domino)
        {
            FreeDominoPosition(domino);
            mOpenExitStack.PopOpenExits();
        }

        #endregion

        // Constructor
        public BoardController(int size)
        {
            mOpenExitStack = new OpenExitStack(size);

            mSize = size;

            // setup bounds
            mBBox = new BBox(mSize);

            // for tracking occupied spaces
            InitializeGameGrid();
        }

        /// <summary>
        /// Once we reserved a Domino to add to the Board we check if the MinMax coordinates of the Domino Positions.
        /// It allows to limit the search area when looking for available position for new Dominos.
        /// </summary>
        public void UpdateMinMaxCoordinates(DominoController domino)
        {
            mBBox.Grow(domino);
        }


        private void InitializeGameGrid()
        {
            // init game grid
            mGameGrid = new Square[mSize, mSize];

            //initialize to an empty board
            for (int row = 0; row < mSize; row++)
            {
                for (int col = 0; col < mSize; col++)
                {
                    mGameGrid[row, col] = new Square();
                }
            }
        }

        /// <summary>
        /// This undoes placement of a domino
        /// </summary>
        /// <param name="domino"></param>
        private void FreeDominoPosition(DominoController domino)
        {
            if (domino.IsHorizontal())
            {
                mGameGrid[domino.Row, domino.Column - 1].Clear();
                mGameGrid[domino.Row, domino.Column].Clear();
                mGameGrid[domino.Row, domino.Column + 1].Clear();
            }
            else
            {
                mGameGrid[domino.Row + 1, domino.Column].Clear();
                mGameGrid[domino.Row, domino.Column].Clear();
                mGameGrid[domino.Row - 1, domino.Column].Clear();
            }
        }

        public int CalculatePlayer1Score()
        {
            int northExit = 0;
            int southExit = 0;

            int[, ,] openExits = mOpenExitStack.GetOpenExists();

            for (int i = 0; i < mSize; i++)
            {
                if (openExits[0, 0, i] == mSize - 1)
                {
                    northExit++;
                }

                if (openExits[0, 2, i] == 0)
                {
                    southExit++;
                }
            }
            return (northExit * southExit);
        }

        public int CalculatePlayer2Score()
        {
            int eastExit = 0;
            int westExit = 0;

            int[, ,] openExits = mOpenExitStack.GetOpenExists();

            for (int i = 0; i < mSize; i++)
            {
                if (openExits[0, 3, i] == mSize - 1)
                {
                    eastExit++;
                }

                if (openExits[0, 1, i] == 0)
                {
                    westExit++;
                }
            }
            return (eastExit * westExit);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="startCoordonnate"></param>
        /// <returns></returns>

        public GridPoint GetOpenExitCoordonnate(int playerIndex, bool spawnLower, int startCoordonnate)
        {
            int[, ,] openExits = mOpenExitStack.GetOpenExists();
            int exitCoord = startCoordonnate-1;
            for (int coord = 0; coord < Size; coord++)
            {
                exitCoord++;
                if (exitCoord == Size) 
                {
                    exitCoord = 0;
                }
                if (playerIndex == 0)
                {
                    if (spawnLower)
                    {
                        if (openExits[0, (int)CardinalPoint.N, exitCoord] > -1 && openExits[0, (int)CardinalPoint.N, exitCoord]<Size)
                        {
                            return new GridPoint(openExits[0, (int)CardinalPoint.N, exitCoord], exitCoord);
                        }
                    }
                    else
                    {
                        if (openExits[0, (int)CardinalPoint.S, exitCoord] > -1 && openExits[0, (int)CardinalPoint.S, exitCoord] < Size )
                        {
                            return new GridPoint(openExits[0, (int)CardinalPoint.S, exitCoord],exitCoord);
                        }
                    }
                }
                else
                {
                    if (spawnLower)
                    {
                        if (openExits[0, (int)CardinalPoint.E, exitCoord] > -1 && openExits[0, (int)CardinalPoint.E, exitCoord] < Size )
                        {
                            return new GridPoint(exitCoord, openExits[0, (int)CardinalPoint.E, exitCoord]);
                        }
                    }
                    else
                    {
                        if (openExits[0, (int)CardinalPoint.W, exitCoord] > -1 && openExits[0, (int)CardinalPoint.W, exitCoord] < Size)
                        {
                            return new GridPoint(exitCoord, openExits[0, (int)CardinalPoint.W, exitCoord]);
                        }
                    }
                }
            }
            Debug.WriteLine("No Open Exit !!!");
            return null;
        }

        /// <summary>
        /// Keep Track of all open Exit for each direction.
        /// </summary>
        /// <param name="domino"></param>
        private void UpdateOpenExitsArray(DominoController domino)
        {
            int[, ,] openExits = mOpenExitStack.GetOpenExists();

            if (domino.IsHorizontal())
            {

                for (int i = -1; i < 2; i++)
                {
                    //North Exit
                    if (openExits[0, 0, domino.Column + i] < domino.Row)
                    {
                        if ((mGameGrid[domino.Row, domino.Column + i].NorthExit)
                            && (openExits[1, 0, domino.Column + i] < domino.Row))
                        {
                            openExits[0, 0, domino.Column + i] = domino.Row;
                        }
                        else
                        {
                            openExits[0, 0, domino.Column + i] = -1;
                            openExits[1, 0, domino.Column + i] = Math.Max(domino.Row, openExits[1, 0, domino.Column + i]);
                        }
                    }

                    //South Exit

                    if (openExits[0, 2, domino.Column + i] > domino.Row)
                    {
                        if (mGameGrid[domino.Row, domino.Column + i].SouthExit
                            && openExits[1, 2, domino.Column + i] > domino.Row)
                        {
                            openExits[0, 2, domino.Column + i] = domino.Row;
                        }
                        else
                        {
                            openExits[0, 2, domino.Column + i] = Size;
                            openExits[1, 2, domino.Column + i] = Math.Min(domino.Row, openExits[1, 2, domino.Column + i]);
                        }
                    }
                }

                //West Exit
                if (openExits[0, 1, domino.Row] > domino.Column)
                {
                    if (mGameGrid[domino.Row, domino.Column - 1].WestExit
                        && openExits[1, 1, domino.Row] > domino.Column)
                    {
                        openExits[0, 1, domino.Row] = domino.Column - 1;
                    }
                    else
                    {
                        openExits[0, 1, domino.Row] = Size;
                        openExits[1, 1, domino.Row] = Math.Min(openExits[1, 1, domino.Row], domino.Column - 1);
                    }
                }


                //East Exit
                if (openExits[0, 3, domino.Row] < domino.Column)
                {
                    if (mGameGrid[domino.Row, domino.Column + 1].EastExit
                        && (openExits[1, 3, domino.Row] < domino.Column))
                    {
                        openExits[0, 3, domino.Row] = domino.Column + 1;
                    }
                    else
                    {
                        openExits[0, 3, domino.Row] = -1;
                        openExits[1, 3, domino.Row] = Math.Max(openExits[1, 3, domino.Row], domino.Column + 1);
                    }
                }
            }
            else //domino vertical
            {
                for (int i = -1; i < 2; i++)
                {
                    //East Exit
                    if (openExits[0, 3, domino.Row + i] < domino.Column)
                    {
                        if (mGameGrid[domino.Row + i, domino.Column].EastExit
                            && (openExits[1, 3, domino.Row + i] < domino.Column))
                        {
                            openExits[0, 3, domino.Row + i] = domino.Column;
                        }
                        else
                        {
                            openExits[0, 3, domino.Row + i] = -1;
                            openExits[1, 3, domino.Row + i] = Math.Max(openExits[1, 3, domino.Row + i], domino.Column);
                        }
                    }

                    //West Exit

                    if (openExits[0, 1, domino.Row + i] > domino.Column)
                    {
                        if (mGameGrid[domino.Row + i, domino.Column].WestExit
                            && openExits[1, 1, domino.Row + i] > domino.Column)
                        {
                            openExits[0, 1, domino.Row + i] = domino.Column;
                        }
                        else
                        {
                            openExits[0, 1, domino.Row + i] = Size;
                            openExits[1, 1, domino.Row + i] = Math.Min(openExits[1, 1, domino.Row + i], domino.Column);
                        }
                    }
                }


                //North Exit
                if (openExits[0, 0, domino.Column] < domino.Row)
                {
                    if (mGameGrid[domino.Row + 1, domino.Column].NorthExit
                        && openExits[1, 0, domino.Column] < domino.Row)
                    {
                        openExits[0, 0, domino.Column] = domino.Row + 1;
                    }
                    else
                    {
                        openExits[0, 0, domino.Column] = -1;
                        openExits[1, 0, domino.Column] = Math.Max(openExits[1, 0, domino.Column], domino.Row + 1);
                    }
                }

                //South Exit
                if (openExits[0, 2, domino.Column] > domino.Row)
                {
                    if (mGameGrid[domino.Row - 1, domino.Column].SouthExit
                        && openExits[1, 2, domino.Column] > domino.Row)
                    {
                        openExits[0, 2, domino.Column] = domino.Row - 1;
                    }
                    else
                    {
                        openExits[0, 2, domino.Column] = Size;
                        openExits[1, 2, domino.Column] = Math.Min(openExits[1, 2, domino.Column], domino.Row - 1);
                    }
                }
            }

#if DEEPTEST
            for (int i = 0; i < 2; i++)
            {
                for (int card = 0; card < 4; card++)
                {
                    for (int mr = 0; mr < Size; mr++)
                    {
                        if ((openExits[i, card, mr] > -1) && (openExits[i, card, mr] < Size))
                        {
                            Console.WriteLine(" OpenExit/(MinMax) " + i + " Card Point " + card + " Row/Column " + mr + " Value " + openExits[i, card, mr]);

                        }
                    }
                }
               }
#endif
        }

        /// <summary>
        /// Compute a board's score based on a heuristic
        /// this is for the AI system
        /// </summary>
        /// <returns></returns>
        public int GetBoardValue()
        {
            int northExit = 0;
            int westExit = 0;
            int southExit = 0;
            int eastExit = 0;

            int[, ,] openExits = mOpenExitStack.GetOpenExists();

            for (int i = 0; i < Size; i++)
            {
                if (openExits[0, 0, i] > -1)
                {
                    northExit += WeightExit(openExits[0, 0, i]);

                }
                if (openExits[0, 1, i] < Size)
                {
                    westExit += WeightExit(Size - openExits[0, 1, i]);
                }
                if (openExits[0, 2, i] < Size)
                {
                    southExit += WeightExit(Size - openExits[0, 2, i]);
                }
                if (openExits[0, 3, i] > -1)
                {
                    eastExit += WeightExit(openExits[0, 3, i]);
                }
            }




#if TESTALPHABETA
            Debug.WriteLine("return (1 + eastExit (" + eastExit + ")) * (1 + westExit (" + westExit + ")) - (1 + northExit(" + northExit + ")) * (1 + southExit(" + southExit + ") ");

            return (1 + eastExit) * (1 + westExit) -
                (1 + northExit) * (1 + southExit);
# else
            return (1 + eastExit) * (1 + westExit) -
                (1 + northExit) * (1 + southExit)
                + 10 * ((mBBox.ColumnMax - mBBox.ColumnMin) - (mBBox.RowMax - mBBox.RowMin));
#endif
        }

        /// <summary>
        /// Used in scoring 
        /// </summary>
        /// <param name="weight"></param>
        /// <returns></returns>
        int WeightExit(int weight)
        {
#if TESTALPHABETA

            return 1;// GameScreenTest.TestWeight(weight);  
#else
           
            if (weight < (Size / 2))
            {
                return weight;
            }
            else if (weight < 0.75 * Size)
            {
                return 2 * weight;
            }
            else if (weight < Size - 1)
            {
                return 3 * weight;
            }
            else
            {
                return 10 * weight;
            }
#endif
        }

        #region CHECK DOMINOES

        public bool IsLegalMove(DominoController domino)
        {
            return PositionFree(domino) && PipesConnect(domino);
        }

        ///<summary> 
        /// A Domino will occupy the space of 3 squares
        /// We have to check that the 3 squares don't belong yet to other dominoes
        /// This Check is not needed for the first Domino
        ///</summary>
        private bool PositionFree(DominoController domino)
        {
			UnityEngine.Debug.DebugBreak();
            bool positionEmpty = false;

            if (domino.IsHorizontal() && domino.Column > 0 && domino.Column < mSize - 1)
            {
                if (((domino.Row > (mBBox.RowMax + 1)) || (domino.Row < (mBBox.RowMin - 1))) ||
                    //North Border search area
                      (domino.Row == (mBBox.RowMax + 1) &&
                            (mGameGrid[mBBox.RowMax, domino.Column - 1].Occupied == false) &&
                           (mGameGrid[mBBox.RowMax, domino.Column].Occupied == false) &&
                               (mGameGrid[mBBox.RowMax, domino.Column + 1].Occupied == false))
                    //South Border search area
                    || (domino.Row == (mBBox.RowMin - 1) &&
                        (mGameGrid[mBBox.RowMin, domino.Column - 1].Occupied == false) &&
                                (mGameGrid[mBBox.RowMin, domino.Column].Occupied == false) &&
                                    (mGameGrid[mBBox.RowMin, domino.Column + 1].Occupied == false))
                        ||
                    //East Border search area      
                   (domino.Column == (mBBox.ColumnMax + 2) &&
                        (mGameGrid[domino.Row, mBBox.ColumnMax].Occupied == false)) ||
                    //West Border search area
                        (domino.Column == (mBBox.ColumnMin - 2) &&
                        (mGameGrid[domino.Row, mBBox.ColumnMin].Occupied == false))
                        )
                {
#if TEST
                        Console.WriteLine("No connections");
#endif
                    return false;
                }

                //Inside search area

                if ((mGameGrid[domino.Row, domino.Column - 1].Occupied == false) &&
                          (mGameGrid[domino.Row, domino.Column].Occupied == false) &&
                              (mGameGrid[domino.Row, domino.Column + 1].Occupied == false))

                    positionEmpty = true;

            }

              //  Domino vertical

            else if (!domino.IsHorizontal() && domino.Row > 0 && domino.Row < Size - 1)
            {
                if (((domino.Column > (mBBox.ColumnMax + 1)) || (domino.Column < (mBBox.ColumnMin - 1))) ||
                    //East Border search area
                    (domino.Column == (mBBox.ColumnMax + 1) &&
                    (mGameGrid[domino.Row - 1, mBBox.ColumnMax].Occupied == false) &&
                    (mGameGrid[domino.Row, mBBox.ColumnMax].Occupied == false) &&
                    (mGameGrid[domino.Row + 1, mBBox.ColumnMax].Occupied == false)) ||
                    //West Border search area
                    (domino.Column == (mBBox.ColumnMin - 1) &&
                    (mGameGrid[domino.Row - 1, mBBox.ColumnMin].Occupied == false) &&
                        (mGameGrid[domino.Row, mBBox.ColumnMin].Occupied == false) &&
                                 (mGameGrid[domino.Row + 1, mBBox.ColumnMin].Occupied == false))
                      ||
                    //North Border search area
                    (domino.Row == (mBBox.RowMax + 2) &&
                    (mGameGrid[mBBox.RowMax, domino.Column].Occupied == false)) ||
                    //South Border search area
                    (domino.Row == (mBBox.RowMin - 2) && (mGameGrid[mBBox.RowMin, domino.Column].Occupied == false)))
                {
#if TEST
                        Console.WriteLine("No connections");
#endif
                    return false;
                }


                //Inside search area



                if ((mGameGrid[domino.Row + 1, domino.Column].Occupied == false) &&
                                     (mGameGrid[domino.Row, domino.Column].Occupied == false) &&
                                         (mGameGrid[domino.Row - 1, domino.Column].Occupied == false))
                {
                    positionEmpty = true;
                }
            }
            else
            {
#if TEST
                Console.WriteLine("No connections");
#endif
                return false;
            }

#if TEST
            if (domino.Row == 6 && domino.Column == 9 && domino.RotationState == 2&& domino.Label==9)
            
            {
                Console.WriteLine("");
            }

            Console.WriteLine("position available " + mPositionEmpty);
#endif

            return positionEmpty;
        }


        /// <summary>
        ///  This checks if a domino's pipe connections match up right
        /// </summary>
        /// <param name="domino"></param>
        /// <returns></returns>
        private bool PipesConnect(DominoController domino)
        {
            bool waterConnect = false;

            bool waterAligns = WaterExitsAlign(domino, -1, ref waterConnect)
                && WaterExitsAlign(domino, 0, ref waterConnect)
                && WaterExitsAlign(domino, 1, ref waterConnect);

            return waterAligns && waterConnect;

        }

        // exitID  0, 1, 2
        private bool WaterExitsAlign(DominoController domino, int squareID, ref bool waterConnect)
        {
            int row = domino.GetRow(squareID);
            int column = domino.GetColumn(squareID);

            //check north
            if (row < (mGameGrid.GetLength(0) - 1) && mGameGrid[row + 1, column].Occupied)
            {
                bool dominoExit = domino.HasWorldSpaceWaterExit(squareID, CardinalPoint.N);
                bool boardExit = mGameGrid[row + 1, column].SouthExit;

                if (dominoExit && boardExit)
                {
                    waterConnect = true;
                }
                else if (dominoExit || boardExit)
                {
                    return false;
                }
            }

            //check east
            if (column < (mGameGrid.GetLength(1) - 1) && mGameGrid[row, column + 1].Occupied)
            {
                bool dominoExit = domino.HasWorldSpaceWaterExit(squareID, CardinalPoint.E);
                bool boardExit = mGameGrid[row, column + 1].WestExit;

                if (dominoExit && boardExit)
                {
                    waterConnect = true;
                }
                else if (dominoExit || boardExit)
                {
                    return false;
                }
            }
            //check south
            if (row > 0 && mGameGrid[row - 1, column].Occupied)
            {
                bool dominoExit = domino.HasWorldSpaceWaterExit(squareID, CardinalPoint.S);
                bool boardExit = mGameGrid[row - 1, column].NorthExit;

                if (dominoExit && boardExit)
                {
                    waterConnect = true;
                }
                else if (dominoExit || boardExit)
                {
                    return false;
                }
            }
            //check west
            if (column > 0 && mGameGrid[row, column - 1].Occupied)
            {
                bool dominoExit = domino.HasWorldSpaceWaterExit(squareID, CardinalPoint.W);
                bool boardExit = mGameGrid[row, column - 1].EastExit;

                if (dominoExit && boardExit)
                {
                    waterConnect = true;
                }
                else if (dominoExit || boardExit)
                {
                    return false;
                }
            }

            return true;

        }


        /// <summary>
        /// This function finds the right square to mark at the right cardinal point 
        /// for the 3 water exits of the dominos.
        /// Each Domino has 3 Water Exits.
        /// Each square has 4 possible water exits,one in each cardinal point North, West,South,East. 
        /// The right square to mark and the cardinal point vary with the Rotation State of the Domino.
        /// </summary>
        private void MarkDominoExits(DominoController domino)
        {
            int rotationStateRelativeCoordinatesIndex;
            int cardinalPoint;
            int relativeRow = 0;
            int relativeColumn = 0;

            for (int i = 0; i < 3; i++)
            {   
                cardinalPoint = ((int)domino.GetWaterExitCardinalPoint(i) + domino.RotationState) % 4;
                switch (domino.GetWaterExitSquareID(i))
                {
                    case -1:
                        rotationStateRelativeCoordinatesIndex = domino.RotationState;
                        relativeRow = kRotationStateRelativeCoordinates[rotationStateRelativeCoordinatesIndex, 0];
                        relativeColumn = kRotationStateRelativeCoordinates[rotationStateRelativeCoordinatesIndex, 1];

                        break;
                    case 0:
                        relativeRow = 0;
                        relativeColumn = 0;

                        break;
                    case 1:
                        rotationStateRelativeCoordinatesIndex = (2 + domino.RotationState) % 4;
                        relativeRow = kRotationStateRelativeCoordinates[rotationStateRelativeCoordinatesIndex, 0];
                        relativeColumn = kRotationStateRelativeCoordinates[rotationStateRelativeCoordinatesIndex, 1];
                        break;

                }
                MarkSquareExit(domino.Row + relativeRow, domino.Column + relativeColumn, (CardinalPoint)Enum.ToObject(typeof(CardinalPoint), cardinalPoint));

            }
            MarkInsideJunction(domino);
        }

        /// <summary>
        /// This Function marks a  Cardinal Point from a specified Square as a Water Exit 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="cardinalPoint"></param>
        private void MarkSquareExit(int row, int column, CardinalPoint cardinalPoint)
        {

#if TEST
            Console.Write(" Mark Square Row" + mRow + " Column" + mColumn + " CardinalPoint" + mCardinalPoint);
#endif

            if (cardinalPoint == CardinalPoint.N)
            {
                mGameGrid[row, column].NorthExit = true;
            }
            else if (cardinalPoint == CardinalPoint.W)
            {
                mGameGrid[row, column].WestExit = true;
            }
            else if (cardinalPoint == CardinalPoint.S)
            {
                mGameGrid[row, column].SouthExit = true;
            }
            else if (cardinalPoint == CardinalPoint.E)
            {
                mGameGrid[row, column].EastExit = true;
            }
            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="cardinalpoint"></param>
        /// <returns></returns>
        public bool HasSquareExit(int row, int column, CardinalPoint cardinalpoint) 
        {
            switch (cardinalpoint)
            {
                case CardinalPoint.N:
                    if( mGameGrid[row, column].NorthExit)
                    {
                        return true;
                    }
                    break;
                case CardinalPoint.W:
                    if( mGameGrid[row, column].WestExit)
                    {
                        return true;
                    }
                    break;
                case CardinalPoint.S:
                    if( mGameGrid[row, column].SouthExit)
                    {
                        return true;
                    }
                    break;

                case CardinalPoint.E:
                    if( mGameGrid[row, column].EastExit)
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Square has any N,S,E,W exit
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool HasAnySquareExit(int row, int column)
        {
            return  mGameGrid[row, column].NorthExit ||
                    mGameGrid[row, column].WestExit ||
                    mGameGrid[row, column].SouthExit ||
                    mGameGrid[row, column].EastExit;
                    
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="domino"></param>
        void MarkInsideJunction(DominoController domino)
        {
            int row = domino.Row;
            int column = domino.Column;
            if (
               ((domino.GetWaterExitSquareID(0) == -1) || (domino.GetWaterExitSquareID(1) == -1) || (domino.GetWaterExitSquareID(2) == -1))

               &&
                (domino.GetWaterExitSquareID(0) == 1) || (domino.GetWaterExitSquareID(1) == 1) || (domino.GetWaterExitSquareID(2) == 1))
            {
                if (domino.IsHorizontal())
                {
                    MarkSquareExit(row, column - 1, CardinalPoint.E);
                    MarkSquareExit(row, column, CardinalPoint.W);
                    MarkSquareExit(row, column, CardinalPoint.E);
                    MarkSquareExit(row, column + 1,CardinalPoint.W);
                }
                else
                {
                    MarkSquareExit(row - 1, column,CardinalPoint.N);
                    MarkSquareExit(row, column, CardinalPoint.N);
                    MarkSquareExit(row, column, CardinalPoint.S);
                    MarkSquareExit(row + 1, column,CardinalPoint.S);
                }

            }
            else if ((domino.GetWaterExitSquareID(0) == 0) || (domino.GetWaterExitSquareID(1) == 0) || (domino.GetWaterExitSquareID(2) == 0))
            {
                if ((domino.GetWaterExitSquareID(0) == -1) || (domino.GetWaterExitSquareID(1) == -1) || (domino.GetWaterExitSquareID(2) == -1))
                {
                    switch (domino.RotationState)
                    {
                        case 0:
                            MarkSquareExit(row, column - 1, CardinalPoint.E);
                            MarkSquareExit(row, column, CardinalPoint.W);
                            break;
                        case 1:
                            MarkSquareExit(row - 1, column, CardinalPoint.N);
                            MarkSquareExit(row, column, CardinalPoint.S);
                            break;
                        case 2:
                            MarkSquareExit(row, column + 1, CardinalPoint.W);
                            MarkSquareExit(row, column, CardinalPoint.E);
                            break;
                        case 3:
                            MarkSquareExit(row + 1, column, CardinalPoint.S);
                            MarkSquareExit(row, column, CardinalPoint.N);
                            break;
                    }
                }
                else if ((domino.GetWaterExitSquareID(0) == 1) || (domino.GetWaterExitSquareID(1) == 1) || (domino.GetWaterExitSquareID(2) == 1))
                {
                    switch (domino.RotationState)
                    {
                        case 0:
                            MarkSquareExit(row, column + 1, CardinalPoint.W);
                            MarkSquareExit(row, column, CardinalPoint.E);
                            break;
                        case 1:
                            MarkSquareExit(row + 1, column, CardinalPoint.S);
                            MarkSquareExit(row, column, CardinalPoint.N);
                            break;
                        case 2:
                            MarkSquareExit(row, column - 1, CardinalPoint.E);
                            MarkSquareExit(row, column, CardinalPoint.W);
                            break;
                        case 3:
                            MarkSquareExit(row - 1, column, CardinalPoint.N);
                            MarkSquareExit(row, column, CardinalPoint.S);
                            break;
                    }
                }
            }
        }

        //Check if the Domino is at a valid Position.
        //The Spot must be empty.
        //At Least one Pipe connection with another Domino and no Pipe connection should be cut.

        public bool HasALegalMove(DominoController domino)
        {
            int initialRow = domino.Row;
            int initialColumn = domino.Column;
            int initialRotationState = domino.RotationState;
            // search for a legal move
            int minRow = Math.Max(0, mBBox.RowMin - 2);
            int maxRow = Math.Min(mSize, mBBox.RowMax + 3);
            int minColumn = Math.Max(0, mBBox.ColumnMin - 2);
            int maxColumn = Math.Min(mSize, mBBox.ColumnMax + 3);

            for (int row = minRow; row < maxRow; row++)
            {
                domino.Row = row;

                for (int column = minColumn; column < maxColumn; column++)
                {
                    domino.Column = column;

                    for (int rotationState = 0; rotationState < 4; rotationState++)
                    {
                        domino.RotationState = rotationState;
#if TEST
                        Debug.WriteLine("mRow "+ domino.Row+ " mColumn "+domino.Column+ " mRotationState"+ domino.RotationState);
#endif
                        if (IsLegalMove(domino))
                        {
#if TESTALPHABETA
                        
                            Debug.WriteLine("Legal Move: label " + domino.Label + " r" + domino.Row + " c" + domino.Column + " rt" + domino.RotationState);
#endif
                         
                            domino.Row=initialRow;
                            domino.Column=initialColumn;
                            domino.RotationState=initialRotationState;
                            return true;
                        }
                    }
                }
            }
            domino.Row = initialRow;
            domino.Column = initialColumn;
            domino.RotationState = initialRotationState;
         
            return false;
        }

        /// <summary>
        /// When we found a Domino to be Valid we can add it to the Board. 
        /// We will then reserve the corresponding Squares.
        /// We will check as well if the new picked Domino is changing both Player Scores.
        /// </summary>
        /// <param name="domino"></param>
        public void AddDomino(DominoController domino)
        {
            if (domino.IsHorizontal())
            {
                for (int i = -1; i < 2; i++)
        
                {
                    mGameGrid[domino.Row, domino.Column + i].Occupied = true;
                }
            }
            else
            {
                
                for (int i = -1; i < 2; i++)
                {
                mGameGrid[domino.Row + i, domino.Column].Occupied = true;
                }
            }
        

            // This maintains a structure of outflows (for the AI)
            MarkDominoExits(domino);

            // Add to the undo stack
            mOpenExitStack.PushOpenExits();

            // Update Boards Data 
            UpdateOpenExitsArray(domino);
            UpdateMinMaxCoordinates(domino);

#if TESTALPHABETA
            Debug.WriteLine("Add to the board  Domino with Label "
                        + domino.Label + " RotationState" +
                        domino.RotationState + " Row " + domino.Row + " Column " + domino.Column );
#endif
      
        }

        public bool IsSpaceOccupied(int row, int column)
        {            
            return mGameGrid[row, column].Occupied;
        }

        public bool IsSpaceOccupied(GridPoint p)
        {
            return IsSpaceOccupied(p.mRow, p.mColumn);
        }

        /// <summary>
        /// Points connect or do not both connect
        /// </summary>
        public bool PointsConnect(GridPoint p0, GridPoint p1)
        {
            if (p0.mColumn == p1.mColumn)
            {
                if (p0.mRow + 1 == p1.mRow)
                {
                    return mGameGrid[p0.mRow, p0.mColumn].NorthExit && mGameGrid[p1.mRow, p1.mColumn].SouthExit;
                }
                else
                {
                    return mGameGrid[p0.mRow, p0.mColumn].SouthExit && mGameGrid[p1.mRow, p1.mColumn].NorthExit;
                }
            }
            else
            {
                if (p0.mColumn + 1 == p1.mColumn)
                {
                    return mGameGrid[p0.mRow, p0.mColumn].EastExit && mGameGrid[p1.mRow, p1.mColumn].WestExit;
                }
                else
                {
                    return mGameGrid[p0.mRow, p0.mColumn].WestExit && mGameGrid[p1.mRow, p1.mColumn].EastExit;
                }
            }
        }

        ///  check if there is room for a scoring domino on one of the Board's border 
        public bool IsSpaceScoringDomino()
        {
            for (int column=1;column <mSize-2; column++)
            {
                //Check North Border horizontal Domino
           
                if (!mGameGrid[mSize - 1, column - 1].Occupied
                    &&
                    !mGameGrid[mSize - 1, column].Occupied
                    && !mGameGrid[mSize - 1, column + 1].Occupied) 
                {
                    return true;
                }

                //Check North Border Vertical Domino

                if (!mGameGrid[mSize - 1, column ].Occupied
                    &&
                    !mGameGrid[mSize - 2, column].Occupied
                    && !mGameGrid[mSize - 3, column].Occupied)
                {
                    return true;
                }

                //Check South Border horizontal Domino
           
                if (!mGameGrid[0, column - 1].Occupied
                    &&
                    !mGameGrid[0, column].Occupied
                    && !mGameGrid[0, column + 1].Occupied)
                {
                    return true;
                }

                //Check South Border Vertical Domino

                if (!mGameGrid[0, column].Occupied
                    &&
                    !mGameGrid[1, column].Occupied
                    && !mGameGrid[2, column].Occupied)
                {
                    return true;
                }
            }
            
            for (int row = 1; row < mSize - 2; row++)
            {
                //Check West Border Vertical Domino

                if (!mGameGrid[row+1, 0].Occupied
                    &&
                    !mGameGrid[row, 0].Occupied
                    && !mGameGrid[row - 1, 0].Occupied)
                {
                    return true;
                }

                //Check West Border Horizontal Domino

                if (!mGameGrid[row, 0].Occupied
                    &&
                    !mGameGrid[row, 1].Occupied
                    && !mGameGrid[row, 2].Occupied)
                {
                    return true;
                }

                
                //Check East Border Vertical Domino
                if (!mGameGrid[row + 1, mSize-1].Occupied
                    &&
                    !mGameGrid[row, mSize - 1].Occupied
                    && !mGameGrid[row - 1, mSize - 1].Occupied)
                {
                    return true;
                }

                //Check East Border Horizontal Domino

                if (!mGameGrid[row, mSize-3].Occupied
                    &&
                    !mGameGrid[row, mSize - 2].Occupied
                    && !mGameGrid[row, mSize - 1].Occupied)
                {
                    return true;
                }
            }
            //No Position available for scoring Domino
            return false;

        } 
        #endregion


    }

    /// <summary>
    /// Bounding box class that works with IDomino (and it's weird rotation)
    /// </summary>
    public class BBox
    {
        int mRowMax;
        int mRowMin;
        int mColumnMax;
        int mColumnMin;

        public BBox(int size)
        {
            mRowMax = 1 + (int)size / 2;
            mRowMin = (int)size / 2;
            mColumnMax = 1 + (int)size / 2;
            mColumnMin = (int)size / 2;
        }

        public int RowMax
        {
            get
            {
                return mRowMax;
            }
            set
            {
                mRowMax = value;
            }
        }

        public int RowMin
        {
            get
            {
                return mRowMin;
            }
            set
            {
                mRowMin = value;
            }
        }

        public int ColumnMin
        {
            get
            {
                return mColumnMin;
            }
            set
            {
                mColumnMin = value;
            }
        }

        public int ColumnMax
        {
            get
            {
                return mColumnMax;
            }
            set
            {
                mColumnMax = value;
            }
        }

        /// <summary>
        /// Expands bbox
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        public void Grow(DominoController domino)
        {
            if (domino.IsHorizontal())
            {
                if ((domino.Row) < RowMin)
                {
                    mRowMin = domino.Row;
                }
                if (domino.Row > mRowMax)
                {
                    mRowMax = domino.Row;
                }


                if ((domino.Column - 1) < mColumnMin)
                {
                    mColumnMin = domino.Column - 1;
                }
                if ((domino.Column + 1) > mColumnMax)
                {
                    mColumnMax = domino.Column + 1;
                }
            }
            else
            {
                if ((domino.Row - 1) < RowMin)
                {

                    mRowMin = domino.Row - 1;
                }
                if ((domino.Row + 1) > mRowMax)
                {
                    mRowMax = domino.Row + 1;
                }


                if ((domino.Column) < mColumnMin)
                {
                    mColumnMin = domino.Column;
                }
                if ((domino.Column) > mColumnMax)
                {
                    mColumnMax = domino.Column;
                }
            }
        }
    }

}
