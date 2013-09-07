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
	public GameObject InfoTextObject;
	
	public GameObject mPlayer1Text;
	public GameObject mPlayer2Text;
	
	public GameObject mHintDominoObject;
	public GameObject mReloadDialog;
	
	bool mShowingModalDialog = false;
	
	public GameObject mHelpView;
	int mHelpIndex = 0;
	
	Domino ActiveDomino {
		get { return mActiveDomino; }
		set { 
			mActiveDomino = value;
		}
	}
	
	String InfoText {
		get { return InfoTextObject.GetComponent<tk2dTextMesh>().text; }
		set { 
			tk2dTextMesh text =  InfoTextObject.GetComponent<tk2dTextMesh>();
			text.text = value;
			text.Commit();
		}
	}
	
	String Player1Text {
		get { return mPlayer1Text.GetComponent<tk2dTextMesh>().text; }
		set { 
			tk2dTextMesh text = mPlayer1Text.GetComponent<tk2dTextMesh>();
			text.text = value; 
			text.Commit();
		}
	}

	String Player2Text {
		get {return mPlayer2Text.GetComponent<tk2dTextMesh>().text; }
		set { 
			Debug.Log("Set Player 2 text " + value);
			tk2dTextMesh text = mPlayer2Text.GetComponent<tk2dTextMesh>();
			text.text = value; 
			text.Commit();
		}
	}
	
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

	    ActiveDomino = GamePlayManager.Instance.GetNextDomino().GetComponent<Domino>();
		if (mActiveDomino != null) {
			mActiveDomino.MakeActive();
		}
		mDominos.Add(ActiveDomino);
        ActiveDomino.EnableGraphics(mBoard.Controller.Size);
        ActiveDomino.SetHighlight(HighLightMode.Active);
	}

	void Update () {		
		GameEventManager.Instance.Activity();
		
		if (GamePlayManager.Instance.Player1Playing && !GamePlayManager.Instance.Player1Computer) {
			//Debug.Log("play");
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
	        ActiveDomino = null;
	        if (bag.HasEmptySlot())
	        {
				Debug.Log("Add new domino");
	            Domino domino = GamePlayManager.Instance.GetNextDomino();
				
	            domino.EnableGraphics(mBoard.Controller.Size);
	            bag.AddDomino(domino);
	            //check there's a legal move in that bag
	            List<IDomino> cloneDominoes = CloneBag(bag);
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
				int score1 = mBoard.Controller.CalculatePlayer1Score();
				int score2 = mBoard.Controller.CalculatePlayer2Score();
				
				if (score1 > score2) {
					this.InfoText = "Game Over: " + mPlayer1Name + " wins!";
				} else if (score2 > score1) {
					this.InfoText = "Game Over: " + mPlayer2Name + " wins!";
				} else {
					this.InfoText = "Game Over: Draw";
				}
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
		if (mShowingModalDialog) {
			return;
		}

		
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
	
	void HandleDown(Vector2 mousePoint) {
		HideHintIfVisible();
        // Clicked inside board and there is an active domino.	
		if (mBoard.Contains(mousePoint) && (ActiveDomino != null))
        {
            Debug.Log("Inside Board Limits and Active Domino - begin dragging");
            if (ActiveDomino.Contains(mousePoint))
            {
                Debug.Log("Board contains mouse point");
//                mIsDragging = true;
            }
        }
        else if (ActiveDomino == null && GamePlayManager.Instance.Player1Playing && mBags[0].Contains(mousePoint))
        {
            Debug.Log("Inside Bag Limits and Active Domino - begin dragging");
            ActiveDomino = mBags[0].GetSelection(mousePoint);
			
            mBags[0].RemoveDomino(ActiveDomino);
            mDominos.Add(ActiveDomino);
//            mIsDragging = true;
            ActiveDomino.SetHighlight(HighLightMode.Active);
        }
        else if (ActiveDomino == null && (!GamePlayManager.Instance.Player1Playing) && mBags[1].Contains(mousePoint))
        {
            Debug.Log("Inside Bag Limits and Active Domino - begin dragging");
            ActiveDomino = mBags[1].GetSelection(mousePoint);
            mBags[1].RemoveDomino(ActiveDomino);
            mDominos.Add(ActiveDomino);
//            mIsDragging = true;
            ActiveDomino.SetHighlight(HighLightMode.Active);
		}
	}
	
	void HandleMove(Vector2 mousePosition) {
        if (ActiveDomino != null)
        {
            MoveDominoToPoint(ActiveDomino, mousePosition);
        }	
	}
	
	void HandleUp(Vector2 mousePosition){
		if (ActiveDomino != null) {
	        if (!LocationIsInBoard(ActiveDomino.Controller.Row, ActiveDomino.Controller.Column,
	            ActiveDomino.Controller.IsHorizontal())) {
	            Bag activeBag = GetActiveBag();
	            activeBag.AddDomino(ActiveDomino);
	
	            // No longer active on board, set to null.
	            ActiveDomino = null;
				// Leave active.
	        }
	    }
	}
	
	public void CommitMove() {
		if (mShowingModalDialog) {
			return;
		}
		Debug.Log("Commit move");
		if (ActiveDomino != null && TryToPlaceDomino()) {
			Debug.Log("Committed move");
			ActiveDomino = null;			
		} else {
            if (GamePlayManager.Instance.Player1Playing)
            {
				InfoText = "Not a legal move.";
                Debug.Log("Player 1 needs to make a move.");
            }
            else
            {
				InfoText = "Not a legal move.";				
                Debug.Log("Player 2 needs to make a move.");
            }
		}
	}
	
	public void Rotate() {
		if (mShowingModalDialog) {
			return;
		}

		Debug.Log("Rotate");
		if (ActiveDomino != null) {
			ActiveDomino.GetComponent<Domino>().MoveClockWise();
		}		
	}
	
	public void ShowReloadDialog() {
		if (mShowingModalDialog) {
			return;
		}

		mReloadDialog.SetActiveRecursively(true);
		mShowingModalDialog = true;
	}
	
	public void ReloadGame() {
		mReloadDialog.SetActiveRecursively(false);
		mShowingModalDialog = false;
		Application.LoadLevel(0);
	}
	
	public void CancelRestart() {
		mReloadDialog.SetActiveRecursively(false);
		mShowingModalDialog = false;
	}
	
	public void ShowHelp() {
		if (mShowingModalDialog) {
			return;
		}
		mShowingModalDialog = true;
		Debug.Log("Show Info");
		mHelpView.SetActiveRecursively(true);
		
		// Set first sprite 
	}
	
	public void HideHelp() {
		mHelpView.SetActiveRecursively(false);
		mShowingModalDialog = false;
	}
	
	public void NextHelp() {
		tk2dSprite sprite = mHelpView.GetComponent<tk2dSprite>();
		if (mHelpIndex == 6) {
			mHelpIndex = 0;		
			HideHelp();
		} else {
			mHelpIndex++;
		}
		sprite.SetSprite("Help" + mHelpIndex.ToString());
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
            reducecomputerPlayingBag.Add(ActiveDomino);
            ActiveDomino = GamePlayManager.Instance.ComputersPlay(reducecomputerPlayingBag, ennemyBag.GetDominoes()) as Domino;
        }
        else
        {
            Bag ennemyBag = (GamePlayManager.Instance.Player1Playing) ? mBags[1] : mBags[0];
            Bag computerPlayingBag = (GamePlayManager.Instance.Player1Playing) ? mBags[0] : mBags[1];
            ActiveDomino = GamePlayManager.Instance.ComputersPlay(computerPlayingBag.GetDominoes(), ennemyBag.GetDominoes()) as Domino;
            computerPlayingBag.RemoveDomino(ActiveDomino);
        }

        
        mTimeStamp = Time.time;

        // updates the graphical position of the domino
        ActiveDomino.UpdateDominoLocation(mBoard.Controller.Size);

        // transfer control of domino to screen 
        mDominos.Add(ActiveDomino);

        RaiseFXEventsAfterPlacedDomino(ActiveDomino);
		ActiveDomino.MakeInactive();
        ActiveDomino = null;

        ActivateBagDominoes(GamePlayManager.Instance.Player1Computer ? 0 : 1);

    }
	
	private void InactivateBagDominoes(int index) {
		// TODO
		Bag bag = mBags[index];
		for (int domIdx = 0; domIdx < 3; domIdx++) {
	        Domino d = bag.GetDominoes()[domIdx] as Domino;
	        d.MakeInactive();
		}
	}

	private void ActivateBagDominoes(int index) {
		// TODO
		Bag bag = mBags[index];
		for (int domIdx = 0; domIdx < 3; domIdx++) {
	        Domino d = bag.GetDominoes()[domIdx] as Domino;
	        d.MakeActive();
		}
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
        bool isLegal = GamePlayManager.Instance.IsLegalMove(ActiveDomino.Controller);
		Debug.Log("Legal move: " + isLegal.ToString());
        if (isLegal)
        {
            GamePlayManager.Instance.PlaceDomino(ActiveDomino.Controller);
            RaiseFXEventsAfterPlacedDomino(ActiveDomino);
			ActiveDomino.MakeInactive();
            ActiveDomino = null;
        }
        else
        {
			// TODO
//            mDisplayStatus.Alpha = 1;
//            mDisplayStatus.AlphaRate = -0.5f;
			InfoText = "Not a legal move.";
            GameEventManager.Instance.RaiseEvent(new GameEvent(
                GameEvent.GameEventType.FAIL_PLACE,
                ActiveDomino,
                GamePlayManager.Instance.Player1Playing ? 0 : 1));
        }

        // then update this
        mTimeStamp = Time.time;
        return isLegal;
    }
	
	public void ShowHint() {
		if (mShowingModalDialog) {
			return;
		}

		Debug.Log("Show Hint");
        Bag bag = GetActiveBag();
        List<IDomino> cloneDominoes = CloneBag(bag);
		int labelId;
		int row;
		int column;
		int rotation;
        GamePlayManager.Instance.CheckForHintDomino(cloneDominoes, out labelId, out row, out column, out rotation);
        Domino hintDomino = GamePlayManager.Instance.CreateDomino(labelId);
        hintDomino.Controller.Row = row;
        hintDomino.Controller.Column = column;
        hintDomino.Controller.RotationState = rotation;
        hintDomino.UpdateDominoLocation(mBoard.Controller.Size);
		hintDomino.MakeHint();
		mHintDominoObject = hintDomino.gameObject;
		DestroyBag(cloneDominoes);
	}
	
	private void HideHintIfVisible() {
		if (mHintDominoObject != null) {
			// TODO add some fancy graphics.
			GameObject.Destroy(mHintDominoObject);
		}
	}
	
	public void PlaceAnyDomino() {
		int bagIdx = GamePlayManager.Instance.Player1Playing ? 0 : 1;
	    if (GamePlayManager.Instance.FirstDominoSet)
        {
			for (int domIdx = 0; domIdx < 3; domIdx++) {
	            Domino d = mBags[bagIdx].GetDominoes()[0] as Domino;
	            mDominos.Add(d);
	
	            for (int r = 0; r < 4; r++)
	            {
	
	                for (int i = 0; i < GamePlayManager.Instance.BoardSize; i++)
	                {
	                    for (int j = 0; j < GamePlayManager.Instance.BoardSize; j++)
	                    {
	                        d.Controller.RotationState = r;
	                        d.Controller.Row = i;
	                        d.Controller.Column = j;
	
	
	                        if (mBoard.Controller.IsLegalMove(d.Controller))
	                        {
	                            ActiveDomino = d;
	                            TryToPlaceDomino();
	                            mBags[bagIdx].RemoveDomino(d);
	                            d.UpdateDominoLocation(mBoard.Controller.Size);
								ActiveDomino = null;
								return;
	                        }
	                    }
	                }
	            }
			}
        }
	}
	
    private List<IDomino> CloneBag(Bag bag)
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
				GameObject dominoObject = GameObject.Instantiate(mDominoObject) as GameObject;
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
            ShowPlayer1Highlights();
			InfoText = "Player 1's Move";
        }
        else
        {   
            ShowPlayer2Highlights();
			InfoText = "Player 2's Move";
		}
		Player1Text = mPlayer1Name + ": " + mBoard.Controller.CalculatePlayer1Score().ToString();
		Player2Text = mPlayer2Name + ": " + mBoard.Controller.CalculatePlayer2Score().ToString();
	}
	
    void ShowPlayer1Highlights()
    {
		ActivateBagDominoes(0);
		InactivateBagDominoes(1);
	}

	
	void ShowPlayer2Highlights()
    {
		ActivateBagDominoes(1);
		InactivateBagDominoes(0);
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
