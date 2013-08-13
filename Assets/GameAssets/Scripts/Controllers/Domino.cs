using UnityEngine;

using System;
using System.Collections.Generic;
using System.Diagnostics;

using DeltaCommon.Managers;
using DeltaCommon.Entities;
using DeltaCommon.Component;


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
public class Sprite {
	// ACL todo - change the grpahical object's alpha
	public float Alpha { get; set; }
 	public bool Visible { get; set; }
	public float AlphaRate { get; set; }
    public  float ScaleX {get; set;}
    public  float ScaleY {get; set;}	
}

public class Domino : MonoBehaviour, IDomino
{
    
    public static float sTimeToTake = .25f;

    // graphical sprite
    public Sprite mSprite = new Sprite();
    public Sprite mOverlaySprite = new Sprite();
    public Sprite mOutlineSprite = new Sprite();
    public Sprite mBorderSprite = new Sprite();

    float X { 
	    get { return this.transform.position.x; }
	    set { this.transform.position = new Vector3(value, this.transform.position.y, this.transform.position.z); }
    }
   float Y { 
	    get { return this.transform.position.y; }
	    set { this.transform.position = new Vector3(this.transform.position.x, value, this.transform.position.z); }
    }
    float Z { 
	    get { return this.transform.position.z; }
	    set { this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, value); }
    }	
    float RotationZ {
		get {return this.transform.rotation.eulerAngles.z;}
		set {
			Vector3 rotation = this.transform.eulerAngles;
			rotation.z = value;
			this.transform.eulerAngles = rotation;
		}
	}

    DominoController mController;
    public DominoController Controller { get { return mController; } }

    HighLightMode mHighlight = HighLightMode.None;
    /// <summary>
    /// Graphical highlighting
    /// </summary>
    public void SetHighlight(HighLightMode mode) 
    {
        if (mHighlight == mode)
        {
            return;
        }

        mHighlight = mode;

        switch (mHighlight)
        {
            case HighLightMode.Active:
                
                mOutlineSprite.Visible = true;
                mOutlineSprite.Alpha = 0.5f;
                mOutlineSprite.AlphaRate = 0.5f;                    
                break;

            case HighLightMode.FinishedPlacement:

                if(mOutlineSprite.Visible)
                {
                    mOutlineSprite.AlphaRate = -0.75f;
                }

                mOverlaySprite.Visible = true;
                mOverlaySprite.Alpha = 0.5f;
                mOverlaySprite.AlphaRate = -0.5f;

                break;

            case HighLightMode.None:

                mOverlaySprite.Visible = false;
                mOutlineSprite.Visible = false;
		
		// TODOD ACL
                //mOutlineSprite.ColorOperation = ColorOperation.Modulate;
                //mOutlineSprite.Red = 1;
                //mOutlineSprite.Green = 1;
                //mOutlineSprite.Blue = 1;

                //mSprite.ColorOperation = ColorOperation.Modulate;
                //mSprite.Red = 1;
                //mSprite.Green = 1;
                //mSprite.Blue = 1;
                
                break;
        }

    }

    public void Initialize(int label, WaterExit[] exits, int startPosition)
    {
        mController = new DominoController(label, exits, startPosition);
        mController.Parent = this;
    }


    public void Initialize(Domino domino) 
    {
        mController = new DominoController(domino.Controller);
        mController.Parent = this;
    }

    private void InitializeOverlaySprites()
    {

        mOutlineSprite.ScaleX = mSprite.ScaleX * 1.01f;
        mOutlineSprite.ScaleY = mSprite.ScaleY * 1.11f;
        mOutlineSprite.Visible = true;
        mOutlineSprite.Alpha = 0.5f;
        mOutlineSprite.AlphaRate = 0.5f;

        mOverlaySprite.ScaleX = mSprite.ScaleX * 1.01f;
        mOverlaySprite.ScaleY = mSprite.ScaleY * 1.11f;            

        mBorderSprite.ScaleX = mSprite.ScaleX;
        mBorderSprite.ScaleY = mSprite.ScaleY * 1.1f;

        mBorderSprite.Alpha = 0.75f;

    }

    /// <summary>
    /// This allows delayed initialization of graphics
    /// </summary>
    public void EnableGraphics(int boardSize)
    {
        		
	// TODO ACL
        //String imageName = String.Format("content/Tile{0}", mController.Label); 
        //mSprite = SpriteManager.AddSprite(imageName);

        mSprite.ScaleY = 0.5f * Square.kPixelSize;
        mSprite.ScaleX = 3.0f * mSprite.ScaleY;
        if (boardSize % 2 == 0)
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
        InitializeOverlaySprites();

        SetHighlight(HighLightMode.None);
    }

    public void MoveWest()
    {
        if (Instructions.Count == 0)
        {
            mController.Column = mController.Column - 1;
            InstructionManager.MoveToAccurate(this, X - Square.kPixelSize, Y, Z, sTimeToTake);
        }
    }

    public void MoveEast()
    {
        if (Instructions.Count == 0)
        {
            mController.Column = mController.Column + 1;
            InstructionManager.MoveToAccurate(this, X + Square.kPixelSize, Y, Z, sTimeToTake);
        }
    }

    public void MoveSouth()
    {
        if (Instructions.Count == 0)
        {
            mController.Row = mController.Row - 1;
            InstructionManager.MoveToAccurate(this, X, Y - Square.kPixelSize, Z, sTimeToTake);
        }
    }

    public void MoveNorth()
    {
        if (Instructions.Count == 0)
        {
            mController.Row = mController.Row + 1;
            InstructionManager.MoveToAccurate(this, X, Y + Square.kPixelSize, Z, sTimeToTake);
        }
    }

    public void MoveCounterClockWise()
    {
        if (Instructions.Count == 0)
        {
            mController.RotationState = (mController.RotationState + 1) % 4;
            InstructionManager.RotateToAccurate(this, 0, 0, 0.5f * (float)Math.PI * mController.RotationState, sTimeToTake);
        }
    }


    public void MoveClockWise()
    {
        if (Instructions.Count == 0)
        {
            mController.RotationState = (mController.RotationState - 1) % 4;
            if (mController.RotationState < 0)
            {
                mController.RotationState += 4;
            }
            InstructionManager.RotateToAccurate(this, 0, 0, 0.5f * (float)Math.PI * mController.RotationState, sTimeToTake);
        }
    }

    ///<summary>
    /// Change the Domino location when the Row and the 
    /// Column are known and the coordinates X,Y are not been updated yet
    ///</summary>
    public void UpdateDominoLocation(int boardSize)
    {
        if (boardSize % 2 == 0)
        {
            this.X = 0.5f * Square.kPixelSize + (mController.Column - boardSize / 2) * Square.kPixelSize;
            this.Y = 0.5f * Square.kPixelSize + (mController.Row - boardSize / 2) * Square.kPixelSize;
        }
        else
        {
            this.X = (mController.Column - (int)(boardSize / 2)) * Square.kPixelSize;
            this.Y = (mController.Row - (int)(boardSize / 2)) * Square.kPixelSize;
        }
        this.RotationZ = 0.5f * (float)Math.PI * mController.RotationState;

    }

    public bool Contains(Vector2 point)
    {
        return Contains(point.x, point.y);
    }

    /// <summary>
    /// Check if the Mouse Cursor pointes at the Domino
    /// </summary>
    /// <param name="mouseX"></param>
    /// <param name="mouseY"></param>
    /// <returns></returns>
    public bool Contains(float mouseX, float mouseY)
    {
        if (mController.IsHorizontal()) 
        {
            if ((Math.Abs(mouseX - this.X) < 1.5f*Square.kPixelSize )
                && (Math.Abs(mouseY - this.Y) < 0.5f *Square.kPixelSize))
            {
                return true;
            }  
        }
        else 
        {
            if ((Math.Abs(mouseX - this.X) < 0.5f * Square.kPixelSize)
                && (Math.Abs(mouseY - this.Y) < 1.5f * Square.kPixelSize))
            {
                return true;
            }
        }

        return false;
        
    }


    #region GRAPHICS
    public void SetDominoAlpha(float alpha)
    {
        mSprite.Alpha=alpha;
    }
 
    public void SetDominoAlphaRate(float alphaRate)
    {
        mSprite.AlphaRate = alphaRate;
    }
 
    //when a domino is created, its transparency must be gradually decreased until alpha=0.5    
    public void Activity( )
    {
        if (mHighlight == HighLightMode.Active)
        {
            
//            float highlightOscillation = 0.25f * Convert.ToSingle(Math.Sin( Time.time *2.0f )) + 0.75f;
            
/*            mSprite.ColorOperation = ColorOperation.Modulate;
            mSprite.Red = highlightOscillation;
            mSprite.Green = highlightOscillation;
            mSprite.Blue = highlightOscillation;

            mOutlineSprite.ColorOperation = ColorOperation.Modulate;
            mOutlineSprite.Red = highlightOscillation;
            mOutlineSprite.Green = highlightOscillation;
            mOutlineSprite.Blue = highlightOscillation;
  */          
        }
        else if (mHighlight == HighLightMode.FinishedPlacement)
        {
            if (mOutlineSprite.Alpha < 0.001f)
            {
                //SpriteManager.RemoveSprite(mOutlineSprite);
                mOutlineSprite.Visible = false;
                mOutlineSprite.AlphaRate = 0;
            }
        
            //if overlay is not really visible, remove
            if (mOverlaySprite.Alpha < 0.001f)
            {
                mHighlight = HighLightMode.None;
                mOverlaySprite.Visible = false;
                mOverlaySprite.AlphaRate = 0;
            }

/*  TODO ACL           mOutlineSprite.ColorOperation = ColorOperation.Modulate;
            mOutlineSprite.Red = 1;
            mOutlineSprite.Green = 1;
            mOutlineSprite.Blue = 1;
  */          
        }
        else
        {
			/* TODO ACL
            mSprite.ColorOperation = ColorOperation.Modulate;
            mSprite.Red = 1;
            mSprite.Green = 1;
            mSprite.Blue = 1;*/
        }

    }
    #endregion
}


   