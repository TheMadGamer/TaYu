using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using DeltaCommon.Managers;

public class GameController : MonoBehaviour {

    Board mBoard = new Board();
	Domino mDomino;
	List<Domino> mDominos = new List<Domino>();
	
	void Start () {
        // Init shared game data. 
        Domino.InitializeGamePlayData();
 
		DominoGenerator generator = new DominoGenerator(mBoardController.StartPosition, typeof(Domino));
		GamePlayManager.Instance.Init(generator, mBoard.Controller);
	
	    mDomino = Domino.GetNextDomino();
	    mDominos.Add(mDomino);
	    mDomino.EnableGraphics();
	    mDomino.SetHighlight(Domino.HighLightMode.Active);
			
	    InitializeDisplay();

        // sets up a first player
        ChangePlayer();

	}
	
	void InitializeDisplay() {
		/*
		// display text
        mDisplayPlayerOneScore.X = 0;
        mDisplayPlayerOneScore.Y = (Board.Size * 0.5f + 1) * Square.kPixelSize;
        mDisplayPlayerOneScore.Scale = kTextSize;
        mDisplayPlayerOneScore.Spacing = kTextSize;
        mDisplayPlayerOneScore.HorizontalAlignment = HorizontalAlignment.Center;

        // display text
        mDisplayPlayerTwoScore.X = -(Board.Size * 0.5f + 1) * Square.kPixelSize - 80;
        mDisplayPlayerTwoScore.Y = 0;
        mDisplayPlayerTwoScore.Scale = kTextSize;
        mDisplayPlayerTwoScore.Spacing = kTextSize;
        mDisplayPlayerTwoScore.HorizontalAlignment = HorizontalAlignment.Center;

        
        // display status text
        mDisplayStatus.Y = -(Board.Size * 0.5f + 1) * Square.kPixelSize;
        mDisplayStatus.Scale = kTextSize;
        mDisplayStatus.Spacing = kTextSize;
        mDisplayStatus.HorizontalAlignment = HorizontalAlignment.Center;
		 */
	}

	void Update () {

	}
	 
	///<summary>
    /// Change player when one player's turn is over.
    ///</summary>
    void ChangePlayer()
    {
        GamePlayManager.Instance.ChangePlayer();

        // this fades the active player's text alpha (dims it)
        if (GamePlayManager.Instance.Player1Playing)
        {
			/*
            mDisplayPlayerOneScore.AlphaRate = 0.75f;
            mDisplayPlayerOneScore.BlendOperation = BlendOperation.Regular;
            mDisplayPlayerTwoScore.Alpha = 0.75f;
            mDisplayPlayerTwoScore.BlendOperation = BlendOperation.Regular;
			 */
           // show hightlight sprites
           ShowPlayer1Highlights();
        }
        else
        {          
			/*
            mDisplayPlayerOneScore.Alpha = 0.75f;  
            mDisplayPlayerOneScore.BlendOperation = BlendOperation.Regular;
            mDisplayPlayerTwoScore.AlphaRate = 0.75f;
            mDisplayPlayerTwoScore.BlendOperation = BlendOperation.Regular;
			 */
            // show hightlight sprites
            ShowPlayer2Highlights();
        }
    }
	
    void ShowPlayer1Highlights()
    {
		// TODO
	}

	
	void ShowPlayer2Highlights()
    {
		// TODO
	}

}
