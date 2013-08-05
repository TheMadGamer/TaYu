using UnityEngine;

using System;
using System.Collections.Generic;
using System.Text;

using DeltaCommon.Component;

    public class Domino : MonoBehaviour
    {
		class InstructionManager {
			public static void MoveToAccurate(Domino d, float x, float y, float z, float time) {
				// ACL
			}
			
			public static void RotateToAccurate(Domino d, float rx, float ry, float rz, float time){
				
			}
		}
		
		class Instructions {
			
			public static int Count{
				get;
				set;
			}
		}
		
		// Abstraction around game object so that the FRB/XBox port isnt' that bad.
		class Sprite {
			// ACL todo - change the grpahical object's alpha
			public float Alpha {
				get; set;
			}
		}
		
        public enum CardinalPoint { N, W, S, E }
		
		// ACL  - make properties that change the game object
		public float X {
			get;
			set;
		}
		public float Y {
			get;
			set;
		}
		public float Z {
			get;
			set;
		}
		
		public float RotationZ {
			get; set;
		}		
		
        public const int kNumberDominoSet = 4;

        private const int kNumDominoTypes = 28;

        // this should be hidden, accessible via functions
        static int[] sRandomLabelArray;

        // this should be hidden, accessible via functions
        static int sRandomLabelArrayIndex = 0;

        // private
        static System.Random sRandomGenerator;

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

        #region WATER EXIT INITIALIZATION

        static readonly WaterExit[][] kWaterExitDominoDatas = new WaterExit[kNumDominoTypes][];

        /// <summary>
        /// Initializes common sprite data
        /// </summary>
        static public void InitializeGamePlayData()
        {
             InitializeLabelArray();
            
                //domino 0
             kWaterExitDominoDatas[0]  =  new WaterExit[3]{
                 new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                 new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
                 new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S}
                };

            kWaterExitDominoDatas[1]  =  new WaterExit[3]{
                //domino 1
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N}
            };

            kWaterExitDominoDatas[2]  =  new WaterExit[3]{
                //domino 2
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S}
            };
            
            kWaterExitDominoDatas[3]  =  new WaterExit[3]{
                //domino 3
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N}
            };
            
            kWaterExitDominoDatas[4]  =  new WaterExit[3]{
                //domino 4
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.S}
            };

            kWaterExitDominoDatas[5]  =  new WaterExit[3]{
                //domino 5
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.E}
            };
            
            kWaterExitDominoDatas[6]  =  new WaterExit[3]{
                //domino 6
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N}
            };

            kWaterExitDominoDatas[7]  =  new WaterExit[3]{
                //domino 7
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S}
            };
            
            kWaterExitDominoDatas[8]  =  new WaterExit[3]{
                //domino 8
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N}
            };

            kWaterExitDominoDatas[9]  =  new WaterExit[3]{
                //domino 9
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.S}
            };

            kWaterExitDominoDatas[10] = new WaterExit[3]{
                //domino 10
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.E}
            };

            kWaterExitDominoDatas[11]  =  new WaterExit[3]{
                //domino 11
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S}
            };
            
            kWaterExitDominoDatas[12]  =  new WaterExit[3]{
                //domino 12
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N}
            };

            kWaterExitDominoDatas[13]  =  new WaterExit[3]{
                //domino 13
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.S}
            };
            
            kWaterExitDominoDatas[14]  =  new WaterExit[3]{
                //domino 14
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.E}
            };
            
            kWaterExitDominoDatas[15]  =  new WaterExit[3]{
                //domino 15
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N}
            };
            
            kWaterExitDominoDatas[16]  =  new WaterExit[3]{
                //domino 16
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.E}
            };
            
            kWaterExitDominoDatas[17]  =  new WaterExit[3]{
                //domino 17
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.E}
            };

            kWaterExitDominoDatas[18]  =  new WaterExit[3]{
                //domino 18
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N}
            };

            kWaterExitDominoDatas[19]  =  new WaterExit[3]{
                //domino 19
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S}
            };

            kWaterExitDominoDatas[20]  =  new WaterExit[3]{
                //domino 20
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N}
            };
            
            kWaterExitDominoDatas[21]  =  new WaterExit[3]{
                //domino 21
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.E}
            };
            
            kWaterExitDominoDatas[22]  =  new WaterExit[3]{
                //domino 22
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S}
            };

            kWaterExitDominoDatas[23]  =  new WaterExit[3]{
                //domino 23
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N}
            };

            kWaterExitDominoDatas[24]  =  new WaterExit[3]{
                //domino 24
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.E}
            };

            kWaterExitDominoDatas[25]  =  new WaterExit[3]{
                //domino 25
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N}
            };

            kWaterExitDominoDatas[26]  =  new WaterExit[3]{
                //domino 26
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S}
            };

            kWaterExitDominoDatas[27]  =  new WaterExit[3]{
                //domino 27
                new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
                new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
                new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N}
            };
        
        }

        // end of array declaration
        #endregion

        
        public static float sTimeToTake = .25f;

        // graphical sprite
        Sprite mSprite;
        Sprite mOverlaySprite;
        Sprite mOutlineSprite;

        // domino's water exists
        // keep this hidden 
        WaterExit[] mWaterExits;

        // keep this around to do delayed graphics initialization
        int mLabel;

        /// <summary>
        /// Defines the index into list of possible dominoes
        /// </summary>
        /// <returns></returns>
        public int Label()
        {
            return mLabel;
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

        /// <summary>
        /// graphical highlight state
        /// Active - current active dominio
        /// FinishedPlacement - quick flash after placement 
        /// </summary>
        public enum HighLightMode { None, Active, FinishedPlacement };

        HighLightMode mHighlight = HighLightMode.None;
        /// <summary>
        /// Graphical highlighting
        /// </summary>
        public void SetHighlight(HighLightMode mode) 
        { 
            mHighlight = mode;

            switch (mHighlight)
            {
                case HighLightMode.Active:

/*  ACL                  mOutlineSprite = SpriteManager.AddSprite("Content/TileOutline");
                    mOutlineSprite.X = this.X;
                    mOutlineSprite.Y = this.Y;
                    mOutlineSprite.ScaleX = mSprite.ScaleX;
                    mOutlineSprite.ScaleY = mSprite.ScaleY;

                    mOutlineSprite.Visible = true;
                    mOutlineSprite.RotationZ = RotationZ;
                    mOutlineSprite.Alpha = 0.5f;
                    mOutlineSprite.AlphaRate = 0.5f;
                    
                    mOutlineSprite.AttachTo(this, true);
                    mOutlineSprite.BlendOperation = BlendOperation.Regular;
                    */

                    break;

                case HighLightMode.FinishedPlacement:
/* ACL
                    if (mOutlineSprite != null)
                    {
                       // SpriteManager.RemoveSprite(mOutlineSprite);
                        mOutlineSprite.AlphaRate = -0.75f;
                    }


                    mOverlaySprite = SpriteManager.AddSprite("Content/TileFlash");
                    mOverlaySprite.X = this.X;
                    mOverlaySprite.Y = this.Y;
                    mOverlaySprite.ScaleX = mSprite.ScaleX;
                    mOverlaySprite.ScaleY = mSprite.ScaleY;

                    mOverlaySprite.Visible = true;
                    mOverlaySprite.RotationZ = RotationZ;
                    mOverlaySprite.Alpha = 0.5f;
                    mOverlaySprite.AttachTo(this, true);
                    mOverlaySprite.BlendOperation = BlendOperation.Regular;

                    // add an alpha decay
                    mOverlaySprite.AlphaRate = -0.5f;
*/
                    break;
                case HighLightMode.None:
                    if (mOverlaySprite != null)
                    {
   //ACL                     SpriteManager.RemoveSprite(mOverlaySprite);
                    }

                    if (mOutlineSprite != null)
                    {
   // ACL                     SpriteManager.RemoveSprite(mOutlineSprite);
                    }

                    break;
            }

        }

        public Domino(int label)
            : base()
        {

            mWaterExits = kWaterExitDominoDatas[label];
            mLabel = label;
            InitializeDomino();
        }

        
        public static int CountDominoPlayed 
        {
            get
            {
                return (sRandomLabelArrayIndex);
            }
        }
        
        public static Domino GetNextDomino()
        {
            int label = GetNextLabel();
            return new Domino(label);
        }
        

        private void InitializeDomino()
        {   
            Column = Board.StartPosition;
            Row = Board.StartPosition;
            mRotationState = 0;
        }

        /// <summary>
        /// This allows delayed initialization of graphics
        /// </summary>
        public void EnableGraphics()
        {
/* ACL
            String imageName = String.Format("content/Tile{0}", mLabel); 

            mSprite = SpriteManager.AddSprite(imageName);
            mSprite.AttachTo(this, true);
            SpriteManager.AddPositionedObject(this);

            mSprite.ScaleY = 0.5f * Square.kPixelSize;
            mSprite.ScaleX = 3.0f * mSprite.ScaleY;
            if (Board.Size % 2 == 0)
            {
                X = mSprite.ScaleY;
                Y = mSprite.ScaleY;
            }
            else
            {
                X = 0;
                Y = 0;
            }

            mSprite.AlphaRate = 0.5f;

#if DEBUG
            mSprite.Alpha = 0.5f;
#else
            mSprite.Alpha = 0;
#endif
			 */
        }
        


        public void MoveWest()
        {
            if (Instructions.Count == 0)
            {
                Column = Column - 1;
                InstructionManager.MoveToAccurate(this, X - Square.kPixelSize, Y, Z, sTimeToTake);
            }
        }
        
        public void MoveEast()
        {
            if (Instructions.Count == 0)
            {
                Column = Column + 1;
                InstructionManager.MoveToAccurate(this,X + Square.kPixelSize, Y, Z, sTimeToTake);
            }
        }

        public void MoveSouth()
        {
            if (Instructions.Count == 0)
            {
                Row = Row - 1;
                InstructionManager.MoveToAccurate(this, X, Y - Square.kPixelSize, Z, sTimeToTake);
            }
        }

        public void MoveNorth()
        {
            if (Instructions.Count == 0)
            {
                Row = Row + 1;
                InstructionManager.MoveToAccurate(this, X, Y + Square.kPixelSize, Z, sTimeToTake);
            }
        }

        public void MoveCounterClockWise()
        {
            if (Instructions.Count == 0)
            {
                mRotationState = (RotationState + 1) % 4;
                InstructionManager.RotateToAccurate(this, 0, 0, 0.5f * (float)Math.PI * RotationState, sTimeToTake);
            }
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

        #region LABEL INDEXING

        /// <summary>
        /// The label will be randomly picked from an Array.
        /// The array is initialize putting all the dominos label in it and then shuffle
        /// everything.
        /// </summary>
        static void InitializeLabelArray()
        {
            int mLabel = 0;
            sRandomGenerator = new System.Random();
            sRandomLabelArray = new int[kNumDominoTypes * kNumberDominoSet];
            sRandomLabelArrayIndex = 0;
            for (int i = 0; i < kNumDominoTypes * kNumberDominoSet; i++)
            {
                if ((i % kNumDominoTypes) == 0)
                {
                    mLabel = 0;
                }
                sRandomLabelArray[i] = mLabel;
                mLabel++;
            }
            for (int i = 0; i < kNumDominoTypes * kNumberDominoSet; i++)
            {
                int mRandomIndex = sRandomGenerator.Next(i, kNumDominoTypes * kNumberDominoSet);
                mLabel=sRandomLabelArray[mRandomIndex];
                sRandomLabelArray[mRandomIndex] = sRandomLabelArray[i];
                sRandomLabelArray[i] = mLabel;
                
            }
           
        }

        static public void SetExplicitLabel(int index, int value)
        {
            sRandomLabelArray[index] = value;
        }

        static public void ResetLabelIndex()
        {
            sRandomLabelArrayIndex = 0;
        }

       
        static public int GetPreviousLabel()
        {
            if (sRandomLabelArrayIndex > 0 )
            {
                sRandomLabelArrayIndex--;
            }
            
            return sRandomLabelArray[sRandomLabelArrayIndex];
        
        }

        static public int GetNextLabel()
        {
            int label;

            if (sRandomLabelArrayIndex < kNumberDominoSet * kNumDominoTypes )
            {
                label = sRandomLabelArray[sRandomLabelArrayIndex];
                sRandomLabelArrayIndex++;

            }
            else
            {
                label = 0;
                
            }
            return label;
        }

        #endregion

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
            int worldDirection =  (int) direction;
            worldDirection -= mRotationState;
            worldDirection = worldDirection % 4;
            if (worldDirection < 0)
            {
                worldDirection += 4;
            }

            return HasWaterExit(squareID,
                (Domino.CardinalPoint)Enum.ToObject(typeof(Domino.CardinalPoint), worldDirection));

        }

        /// <summary>
        /// Check if the Mouse Cursor pointes at the Domino
        /// </summary>
        /// <param name="mouseX"></param>
        /// <param name="mouseY"></param>
        /// <returns></returns>
        public bool HasMousePointing(float mouseX, float mouseY)
        {
            bool hasMousePointing=false;
            if (IsHorizontal()) 
            {
                if ((Math.Abs(mouseX - this.X) < 1.5f*Square.kPixelSize )
                    && (Math.Abs(mouseY - this.Y) < 0.5f *Square.kPixelSize))
                
                {
                    hasMousePointing=true;
                }
                    
            }
            else 
            {
                if ((Math.Abs(mouseX - this.X) < 0.5f * Square.kPixelSize)
                    && (Math.Abs(mouseY - this.Y) < 1.5f * Square.kPixelSize))
                {
                    hasMousePointing = true;
                }

            }

            return hasMousePointing;
            
        }




        #region GRAPHICS
        public void SetDominoAlpha(float mAlpha)
        {
        // ACL    mSprite.Alpha=mAlpha;
        }
     
        ///<summary>
        /// Change the Domino location when the Row and the 
        /// Column are known and the coordinates X,Y are not been updated yet
        ///</summary>
        public void UpdateDominoLocation()
        {
            if (Board.Size % 2 == 0)
            {
                this.X = 0.5f * Square.kPixelSize + (this.Column - Board.Size / 2) * Square.kPixelSize;
                this.Y = 0.5f * Square.kPixelSize + (this.Row - Board.Size / 2) * Square.kPixelSize;
            }
            else
            {
                this.X = (this.Column - (int)(Board.Size / 2)) * Square.kPixelSize;
                this.Y = (this.Row - (int)(Board.Size / 2)) * Square.kPixelSize;
            }
            this.RotationZ = 0.5f * (float)Math.PI * this.RotationState;
            
        }

        //when a domino is created, its transparency must be gradually decreased until alpha=0.5    
        public void Activity( )
        {
            /*if (mSprite.Alpha < 0.5f)
            {
                mSprite.Alpha = (float)(mSprite.Alpha + 0.8 * TimeManager.SecondDifference);
            }*/

            if (mHighlight == HighLightMode.Active)
            {
                //Console.WriteLine("Time {0}, Sin {1}", Convert.ToSingle( TimeManager.LastCurrentTime ), Convert.ToSingle(Math.Sin(TimeManager.LastCurrentTime)));

               /* float highlightOscillation = 0.2f * Convert.ToSingle(Math.Sin( TimeManager.LastCurrentTime *2.0f )) + 0.8f;

                mSprite.Red = highlightOscillation;
                mSprite.Green = highlightOscillation;
                mSprite.Blue = highlightOscillation;
                mSprite.ColorOperation = ColorOperation.Modulate;
                */
            }
            else if (mHighlight == HighLightMode.FinishedPlacement)
            {
                if(mOutlineSprite != null)
                {
                    if (mOutlineSprite.Alpha < 0.001f)
                    {
						// ACL - how to handle this?
                        //SpriteManager.RemoveSprite(mOutlineSprite);
                    }
                }

                //if overlay is not really visible, remove
                if (mOverlaySprite.Alpha < 0.001f)
                {
                    mHighlight = HighLightMode.None;
                    // ACL - how to handle this?
					//SpriteManager.RemoveSprite(mOverlaySprite);
                    if (mOutlineSprite != null)
                    {
						// ACL - how to handle this?
                        // SpriteManager.RemoveSprite(mOutlineSprite);
                    }
                }
                

            }
            else
            {
                /*
                mSprite.Red = 0.8f;
                mSprite.Green = 0.8f;
                mSprite.Blue = 0.8f;
                mSprite.ColorOperation = ColorOperation.Modulate;*/
            }
   
        }
        #endregion
    }


   