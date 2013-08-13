using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using DeltaCommon.Component;
using DeltaCommon.Entities;
using DeltaCommon.Managers;

public class GameController : MonoBehaviour {
	
	public GameObject mBoardObject;
    Board mBoard;
	public GameObject mDominoObject;
	Domino mDomino;
	List<Domino> mDominos = new List<Domino>();
    List<Bag> mBags;
	
    String mPlayer1Name = "Player 1";
    String mPlayer2Name = "Player 2";
	
	const int kNumSlots = 3;
	const int kBoardSize = 15;

	void Start () {
		
        GameEventManager.Initialize();
        mBags = new List<Bag>(2);

		mBoard = mBoardObject.GetComponent<Board>();
		mBoard.Initialize(kBoardSize);
		
        // Init shared game data. 
		BoardController boardController = mBoard.Controller;
		Debug.Log("board controller " + boardController.ToString());
		
		DominoGenerator generator = new DominoGenerator(boardController.StartPosition, mDominoObject);
		Debug.Log("board controller" + generator.ToString());
		GamePlayManager.Initialize();
		GamePlayManager.Instance.Init(generator, mBoard.Controller, mDominoObject);
	
		// init draw bags
        {
            Bag bag = new Bag(kNumSlots, mBoard.Controller.Size, true);

            for (int i = 0; i < kNumSlots; i++)
            {
                GameObject gameObject = GamePlayManager.Instance.GetNextDomino();
				Domino d = gameObject.GetComponent<Domino>();
                d.EnableGraphics(mBoard.Controller.Size);
                bag.AddDomino(d);
            }
            mBags.Add(bag);
        }

        {
            Bag bag = new Bag(kNumSlots, mBoard.Controller.Size, false);

            for (int i = 0; i < kNumSlots; i++)
            {
                GameObject gameObject = GamePlayManager.Instance.GetNextDomino();
				Domino d = gameObject.GetComponent<Domino>();

                d.EnableGraphics(mBoard.Controller.Size);
                d.UpdateDominoLocation(mBoard.Controller.Size);
                bag.AddDomino(d);
            }
            mBags.Add(bag);
        }


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
