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
	
	GameObject mActiveDomino = null;
	
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

	    mActiveDomino = GamePlayManager.Instance.GetNextDomino();
		Domino activeController = mActiveDomino.GetComponent<Domino>();
		mDominos.Add(activeController);
        activeController.EnableGraphics(mBoard.Controller.Size);
        activeController.SetHighlight(HighLightMode.Active);
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
		Vector2 inputPos = Input.mousePosition;
		inputPos = new Vector2(inputPos.x - Screen.width / 2.0f, inputPos.y - Screen.height / 2.0f);
		if (Input.GetMouseButtonDown(0)) {
			MoveDominoToPoint(mActiveDomino, inputPos);
		} else if (Input.GetMouseButton(0)) {
			MoveDominoToPoint(mActiveDomino, inputPos);			
		}
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
	
	
	/// <summary>
    /// Is a logical row, col, ornt on the board
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="isHorizontal"></param>
    bool LocationIsInBoard(int row, int column, bool isHorizontal)
    {
        return (isHorizontal && (-1 < row && row < mBoard.Controller.Size) && (0 < column && column < (mBoard.Controller.Size - 1)))
                || (!isHorizontal && (0 < row && row < (mBoard.Controller.Size - 1)) && (-1 < column && column < mBoard.Controller.Size));
    }

    /// <summary>
    /// Is a logical row, col, ornt in a bag
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="isHorizontal"></param>
    /// <returns></returns>
    bool LocationIsInUpperBag(int row, int column, bool isHorizontal)
    {
        return (isHorizontal && (mBoard.Controller.Size <= row && row < (mBoard.Controller.Size + 2)) && (0 < column && column < (mBoard.Controller.Size - 1)))
                || (!isHorizontal && ((mBoard.Controller.Size - 1) <= row && row < (mBoard.Controller.Size + 2)) && (-1 < column && column < mBoard.Controller.Size));
    }

    /// <summary>
    /// Is a logical row, col, ornt in a bag
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="isHorizontal"></param>
    /// <returns></returns>
    bool LocationIsInLeftBag(int row, int column, bool isHorizontal)
    {
        return (isHorizontal && (-1 < row && row < mBoard.Controller.Size) && (-5 < column && column < 1))
                || (!isHorizontal && (0 < row && row < (mBoard.Controller.Size - 1)) && (-6 < column && column < 0));
    }

    /// <summary>
    /// Gets bag assoc'd with current player
    /// </summary>
    /// <returns></returns>
    protected Bag GetActiveBag()
    {
        if (GamePlayManager.Instance.Player1Playing)
        {
            return mBags[0];
        }
        else
        {
            return mBags[1];
        }
    }

        ///<summary>
    /// Move the domino to the closest position next to the Mouse Pointer
    ///</summary>
    /// <param name="domino"></param>
    /// <param name="mousePositionX"></param>
    /// <param name="mousePosittionY"></param>    
    void MoveDominoToPoint(GameObject dominoObject, Vector2 mousePoint)
    {
		Debug.Log("Move domino to point " + mousePoint.ToString());
        int row;
        int column;

        //find row column from point
        if (mBoard.Controller.Size % 2 == 1)
        {
            row = (int)Math.Round((mousePoint.y - 0.5f * Square.kPixelSize) / Square.kPixelSize + 0.5f * mBoard.Controller.Size);
            column = (int)Math.Round((mousePoint.x - 0.5f * Square.kPixelSize) / Square.kPixelSize + 0.5f * mBoard.Controller.Size);

        }
        else
        {
            row = (int)Math.Round(mousePoint.y / Square.kPixelSize + 0.5f * mBoard.Controller.Size);
            column = (int)Math.Round(mousePoint.x / Square.kPixelSize + 0.5f * mBoard.Controller.Size);
        }

		// Note ACL there was some layer logic here.
		
		Domino domino = dominoObject.GetComponent<Domino>();
		
        // this will move on the board
        if (LocationIsInBoard(row, column, domino.Controller.IsHorizontal()))
        {
            domino.Controller.Row = row;
            domino.Controller.Column = column;
            domino.UpdateDominoLocation(mBoard.Controller.Size);

        }
        
        // if p1, allow move out top to bag
        else if (GamePlayManager.Instance.Player1Playing &&
            LocationIsInUpperBag(row, column, domino.Controller.IsHorizontal()))
        {
            domino.Controller.Row = row;
            domino.Controller.Column = column;
            domino.UpdateDominoLocation(mBoard.Controller.Size);

        }
        // if p1, allow move out left to bag
        else if ((!GamePlayManager.Instance.Player1Playing) &&
            LocationIsInLeftBag(row, column, domino.Controller.IsHorizontal()))
        {
            domino.Controller.Row = row;
            domino.Controller.Column = column;
            domino.UpdateDominoLocation(mBoard.Controller.Size);
        }
    }
}
