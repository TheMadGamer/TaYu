using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using DeltaCommon.Component;
using DeltaCommon.Entities;
using DeltaCommon.Managers;
using DeltaCommon.Events;

public class GameController : MonoBehaviour {
	
	public GameObject mBoardObject;
    Board mBoard;
	public GameObject mDominoObject;
	Domino mDomino;
	List<Domino> mDominos = new List<Domino>();
    List<Bag> mBags;
	
	Domino mActiveDomino = null;
	
    String mPlayer1Name = "Player 1";
    String mPlayer2Name = "Player 2";
	
	const int kNumSlots = 3;
	const int kBoardSize = 15;
	const double kWaitingTime = 0.5;
	public int FixedScreenHeight = 680; // Camera size must be 340.
	private GamePlayManager mGamePlayManager = GamePlayManager.Instance; // Pointer seems to dealloc'ing?
	float mTimeStamp;
	
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
                Domino d = GamePlayManager.Instance.GetNextDomino();				
                d.EnableGraphics(mBoard.Controller.Size);
                bag.AddDomino(d);
            }
            mBags.Add(bag);
        }

        {
            Bag bag = new Bag(kNumSlots, mBoard.Controller.Size, false);

            for (int i = 0; i < kNumSlots; i++)
            {
				Domino d = GamePlayManager.Instance.GetNextDomino();

                d.EnableGraphics(mBoard.Controller.Size);
                d.UpdateDominoLocation(mBoard.Controller.Size);
                bag.AddDomino(d);
            }
            mBags.Add(bag);
        }

	    mActiveDomino = GamePlayManager.Instance.GetNextDomino().GetComponent<Domino>();
		mDominos.Add(mActiveDomino);
        mActiveDomino.EnableGraphics(mBoard.Controller.Size);
        mActiveDomino.SetHighlight(HighLightMode.Active);
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

		if (GamePlayManager.Instance.GameOver) {
			Debug.Log("Restart game?");
		} 
		
		GameEventManager.Instance.Activity();
		
		if (GamePlayManager.Instance.Player1Playing && !GamePlayManager.Instance.Player1Computer) {
			//Debug.Log("Player 1 move");
			HandleMouseInput();
		} else if ((!GamePlayManager.Instance.Player1Playing) && !GamePlayManager.Instance.Player2Computer) {
			//Debug.Log("Player 2 move");
			HandleMouseInput();
		}
		
		// Either the user or the computer has added a new Domino
        if (GamePlayManager.Instance.PickNewDomino && ((Time.time - mTimeStamp) > kWaitingTime)) {

	        //draw into previous bag
	        // before the change over
	        Bag bag = GetActiveBag();
	        mActiveDomino = null;
	        if (bag.HasEmptySlot())
	        {
				Debug.Log("Add new domino");
	            Domino domino = GamePlayManager.Instance.GetNextDomino();
				
	            domino.EnableGraphics(mBoard.Controller.Size);
	            bag.AddDomino(domino);
	            //check there's a legal move in that bag
	            List<IDomino> cloneDominoes = CloneBag(bag, mDominoObject);
	            GamePlayManager.Instance.CheckForGameOver(cloneDominoes);
				DestroyBag(cloneDominoes);
	        }
	        else
	        {
	            Debug.Log("First move, no draw");
	        }
	
	        GamePlayManager.Instance.DoneWithDraw();
			ChangePlayer();
	         
	        // check if the game is over
	        // highlight for visibility
	        //mDomino.SetHighlight(Domino.HighLightMode.Active);
	        GamePlayManager.Instance.GameOnHold = false;
	
	        if (GamePlayManager.Instance.GameOver)
	        {
				Debug.Log("Game over");
	            // move this back to the game screen - graphics belong in the screen
	            // not the game play logic
//	            mDisplayStatus.Alpha = 1;
//	            mDisplayStatus.DisplayText = "No legal position to play any tiles.  Game Over.";
//	            mDisplayStatus.AlphaRate = -0.5f;
	        }
	
	        mTimeStamp = Time.time;
	    }

	    //Computer's Turn
	    if (!GamePlayManager.Instance.GameOnHold
	        && !GamePlayManager.Instance.HumanPlayer
	        && !GamePlayManager.Instance.GameOver
	        && ((Time.time - mTimeStamp) > kWaitingTime))
	    {
	        ComputerMove();
	    }
	}
		
	void HandleMouseInput() {
		// Center input position vector.
		Vector2 inputPos = Input.mousePosition;
		// Center
		inputPos = new Vector2(inputPos.x - Screen.width / 2.0f, inputPos.y - Screen.height / 2.0f);
		
		// Rescale
		inputPos *= (float) FixedScreenHeight / Screen.height;
		
		if (Input.GetMouseButtonDown(0)) {
			// Selection.
			HandleDown(inputPos);
		} else if (Input.GetMouseButton(0)) {
			HandleMove(inputPos);
		} else if (Input.GetMouseButtonUp(0)) {
			HandleUp(inputPos);
		}
		
		// TODO: Handle rotation;
	}
	
	void HandleDown(Vector2 mousePoint){
        // Clicked inside board and there is an active domino.
        //Debug.Log("Handle down" + mousePoint.ToString());

		
		if (mBags[0].Contains(mousePoint)) {
			Debug.Log("bag 0 contains");
		}

		if (mBags[1].Contains(mousePoint)) {
			Debug.Log("bag 1 contains");
		}
		
		if (mBoard.Contains(mousePoint) && (mActiveDomino != null))
        {
            Debug.Log("Inside Board Limits and Active Domino - begin dragging");
            if (mActiveDomino.Contains(mousePoint))
            {
                Debug.Log("Board contains mouse point");
//                mIsDragging = true;
            }
        }
        else if (mActiveDomino == null && GamePlayManager.Instance.Player1Playing && mBags[0].Contains(mousePoint))
        {
            Debug.Log("Inside Bag Limits and Active Domino - begin dragging");
            mActiveDomino = mBags[0].GetSelection(mousePoint);
            mBags[0].RemoveDomino(mActiveDomino);
            mDominos.Add(mActiveDomino);
//            mIsDragging = true;
            mActiveDomino.SetHighlight(HighLightMode.Active);
        }
        else if (mActiveDomino == null && (!GamePlayManager.Instance.Player1Playing) && mBags[1].Contains(mousePoint))
        {
            Debug.Log("Inside Bag Limits and Active Domino - begin dragging");
            mActiveDomino = mBags[1].GetSelection(mousePoint);
            mBags[1].RemoveDomino(mActiveDomino);
            mDominos.Add(mActiveDomino);
//            mIsDragging = true;
            mActiveDomino.SetHighlight(HighLightMode.Active);
		}
	}
	
	void HandleMove(Vector2 mousePosition) {
        if (mActiveDomino != null)
        {
            MoveDominoToPoint(mActiveDomino, mousePosition);
        }	
	}
	
	void HandleUp(Vector2 mousePosition){
		if (mActiveDomino != null) {
	        if (!LocationIsInBoard(mActiveDomino.Controller.Row, mActiveDomino.Controller.Column,
	            mActiveDomino.Controller.IsHorizontal())) {
	            Bag activeBag = GetActiveBag();
	            activeBag.AddDomino(mActiveDomino);
	
	            // No longer active on board, set to null.
	            mActiveDomino = null;
	        }
	    }
	}
	
	public void CommitMove() {
		Debug.Log("Commit move");
		if (mActiveDomino != null && TryToPlaceDomino()) {
			Debug.Log("Committed move");
			mActiveDomino = null;
		} else {
            if (GamePlayManager.Instance.Player1Playing)
            {
                Debug.Log("Player 1 needs to make a move.");
            }
            else
            {
                Debug.Log("Player 2 needs to make a move.");
            }
		}
	}
	
	public void Rotate() {
		Debug.Log("Rotate");
		if (mActiveDomino != null) {
			mActiveDomino.GetComponent<Domino>().MoveClockWise();
		}		
	}
	
	List<Domino> getDominoList(List<GameObject> objectList) {
		List<Domino> dominoList = new List<Domino>();
		foreach(GameObject gameObject in objectList) {
			dominoList.Add(gameObject.GetComponent<Domino>());
		}
		return dominoList;
	}
	
    void ComputerMove()
    { 		
		Debug.Log("Computer move");
		if (!GamePlayManager.Instance.FirstDominoSet)           
        {
            Bag ennemyBag = (GamePlayManager.Instance.Player1Playing) ? mBags[1] : mBags[0];
            List<IDomino> reducecomputerPlayingBag = new List<IDomino>();
            reducecomputerPlayingBag.Add(mActiveDomino);
            mActiveDomino = GamePlayManager.Instance.ComputersPlay(reducecomputerPlayingBag, ennemyBag.GetDominoes()) as Domino;
        }
        else
        {
            Bag ennemyBag = (GamePlayManager.Instance.Player1Playing) ? mBags[1] : mBags[0];
            Bag computerPlayingBag = (GamePlayManager.Instance.Player1Playing) ? mBags[0] : mBags[1];
            mActiveDomino = GamePlayManager.Instance.ComputersPlay(computerPlayingBag.GetDominoes(), ennemyBag.GetDominoes()) as Domino;
            computerPlayingBag.RemoveDomino(mActiveDomino);
        }

        
        mTimeStamp = Time.time;

        // updates the graphical position of the domino
        mActiveDomino.UpdateDominoLocation(mBoard.Controller.Size);

        // transfer control of domino to screen 
        mDominos.Add(mActiveDomino);

        RaiseFXEventsAfterPlacedDomino(mActiveDomino);

        mActiveDomino = null;

        DimBagDominoes(GamePlayManager.Instance.Player1Computer ? 0 : 1);

    }
	
	private void DimBagDominoes(int index) {
		// TODO
	}
 
	private void RaiseFXEventsAfterPlacedDomino(Domino domino)
    {
		// TODO
	}	
	
    /// <summary>
    /// This tries to place a domino.  If not legal, a meassage is thrown up.
    /// </summary>
    /// <returns></returns>
    protected bool TryToPlaceDomino()
    {
        bool isLegal = GamePlayManager.Instance.IsLegalMove(mActiveDomino.Controller);
		Debug.Log("Legal move: " + isLegal.ToString());
        if (isLegal)
        {
            GamePlayManager.Instance.PlaceDomino(mActiveDomino.Controller);
            RaiseFXEventsAfterPlacedDomino(mActiveDomino);
            mActiveDomino = null;
        }
        else
        {
			// TODO
//            mDisplayStatus.Alpha = 1;
//            mDisplayStatus.AlphaRate = -0.5f;
//            mDisplayStatus.DisplayText = "Not a legal move.";
            GameEventManager.Instance.RaiseEvent(new GameEvent(
                GameEvent.GameEventType.FAIL_PLACE,
                mActiveDomino,
                GamePlayManager.Instance.Player1Playing ? 0 : 1));
        }

        // then update this
        mTimeStamp = Time.time;
        return isLegal;
    }
	
    private static List<IDomino> CloneBag(Bag bag, GameObject dominoPrefab)
    {
		Debug.Log("Cloning bag");
        List<IDomino> cloneDominoes = new List<IDomino>();
        List<IDomino> bagDominoes = bag.GetDominoes();
        foreach (Domino cloneDomino in bagDominoes)
        {
            if (cloneDomino != null)
            {
				Debug.Log("Cloning domino");
				// Generate a game object, add component. 
				// TODO: This bag needs to be properly removed.
				GameObject dominoObject = GameObject.Instantiate(dominoPrefab) as GameObject;
				Domino domino = dominoObject.GetComponent<Domino>();
				domino.Initialize(cloneDomino);
				domino.DisableGraphics();
				
                cloneDominoes.Add(dominoObject.GetComponent<Domino>());
            }
        }
        return cloneDominoes;
    }
	
	private void DestroyBag(List<IDomino> bagDominoes)
    {
		Debug.Log("Destroying bag");
        foreach (Domino cloneDomino in bagDominoes)
        {
            if (cloneDomino != null)
            {
				Debug.Log("Destroying domino");
				GameObject.Destroy(cloneDomino.gameObject);
            }
        }
    }
	
///<summary>
    /// Change player when one player's turn is over.
    ///</summary>
    void ChangePlayer()
    {
		Debug.Log("Change player");
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
    void MoveDominoToPoint(Domino domino, Vector2 mousePoint)
    {
		//Debug.Log("Move domino to point " + mousePoint.ToString());
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
