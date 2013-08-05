#undef DEBUG
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

using DeltaCommon.Component;
using DeltaCommon.Entities;
using DeltaCommon.Events;
using DeltaCommon.Managers;

namespace DeltaCommon.Managers
{
    public class FoodRespawn
    {
        public IFoodItem mFoodItem = null;
        public float mRespawnTime = 0;
    }

    public class AnimationManager : IEventListener
    {
        #region SINGLETON
        static AnimationManager sInstance = null;
        public static AnimationManager Instance { get { return sInstance; } }

        Type mControllerType;
        Type mFoodItemType;
        object mCanvas = null;

        public static void Initialize(Type controllerType, Type foodItemType,BoardController boardController)
        {
            sInstance = new AnimationManager(controllerType, foodItemType, boardController);
        }

        /// <summary>
        /// override for silverlight
        /// </summary>
        /// <param name="controllerType"></param>
        /// <param name="boardController"></param>
        public static void Initialize(Type controllerType, Type foodItemType, BoardController boardController, object canvas)
        {
            sInstance = new AnimationManager(controllerType,foodItemType, boardController, canvas);
        }

        public static void Unload()
        {
            if (sInstance != null)
            {
                sInstance.Destroy();
                sInstance = null;
            }
        }
        #endregion

        private BoardController mBoardController;

        /// <summary>
        /// This defines how a player's swarm behaves
        /// Whether characters spawn in and walk on boundary, 
        /// Walk onto grid
        /// or Walk Across Grid
        /// </summary>
        enum PathBehavior { NoConnections, HasSingleConnections, HasFullConnections }

        /// <summary>
        /// Defines if the player's character path behavior
        /// </summary>
        PathBehavior[] mPathBehaviors = { PathBehavior.NoConnections, PathBehavior.NoConnections };

        int [] mNumberCharactersExp;
        const int kMaxCharactersNullScore = 5;
        const int kMaxCharacters = 10;
        const int kPoolCharactersSize = 15;
        int mPoolCaracterCount = 0;
        bool mAlternate=false;
        float mSpawnTimer=0.25f;
        bool mSpawnPlayer1 = false;
        /// <summary>
        /// A list of active characters for a player
        /// </summary>
        List<IRoadCharacter> mPlayer1Characters = new List<IRoadCharacter>();
        IFoodItem[] mFoodItemPlayer1;
        IFoodItem[] mFoodItemPlayer2;
        /// <summary>
        /// A list of active characters for a player
        /// </summary>
        List<IRoadCharacter> mPlayer2Characters = new List<IRoadCharacter>();

        bool mPlayer1LowerConnections = false;
        bool mPlayer1UpperConnections = false;

        bool mPlayer2LowerConnections = false;
        bool mPlayer2UpperConnections = false;

        object mGopherLayer;
        public object GopherLayer { set { mGopherLayer = value; } }

        bool mIsGameOver = false;
#if DEBUG
        Random mRandom = new Random(0);
#else
        Random mRandom = new Random();
#endif

        List<FoodRespawn> mRespawns = new List<FoodRespawn>();

#if (WINDOWS_PHONE && !SILVERLIGHT)
        int mCountAliveCharactersPlayer1=0;
        public int CountAliveCharactersPlayer1
        {
            get
            {
                return mCountAliveCharactersPlayer1;
            }
            set
            {
                mCountAliveCharactersPlayer1 = value;
            }
        }
        int mCountAliveCharactersPlayer2 = 0;
        public int CountAliveCharactersPlayer2
        {
            get
            {
                return mCountAliveCharactersPlayer2;
            }
            set
            {
                mCountAliveCharactersPlayer2 = value;
            }
        }
        string logTime=" ";
        int countTime=10;
#endif
        public AnimationManager(Type controllerType, Type foodItemType, BoardController boardController)
        {
            mControllerType = controllerType;
            mFoodItemType = foodItemType;
            mBoardController = boardController;
            mNumberCharactersExp = new int[2];
            InitializeFoodItem();
        }

        public void InitializeRoadCharacter()
        {
                InitCharacter(0);
                InitCharacter(1);
                mPoolCaracterCount = 1;
         }
        
		
        public AnimationManager(Type controllerType, Type foodItemType, BoardController boardController, object canvas)
        {
            mControllerType = controllerType;
            mFoodItemType = foodItemType;
            mBoardController = boardController;
            mCanvas = canvas;
            mNumberCharactersExp = new int[2];
            InitializeFoodItem();
         }

        /// <summary>
        /// Defines the number of characters to spawn
        /// </summary>
        /// <param name="behavior"></param>
        /// <returns></returns>        
        int GetNumDesiredCharacters(int playerIndex)
        {
            int numDesired = 0;
            switch (mPathBehaviors[playerIndex] )
            {
                case PathBehavior.NoConnections:
                    {
                        numDesired = 1;
                        break;
                    }
                case PathBehavior.HasSingleConnections:
                case PathBehavior.HasFullConnections:
                    {
                        numDesired = mNumberCharactersExp[playerIndex];
                        break;
                    }
                 }
            return numDesired;
        }

        /// <summary>
        /// Builds carrots and flowers along border
        /// </summary>
        public void InitializeFoodItem() 
        {
            //build the carrots and flowers border
            mFoodItemPlayer1 = new IFoodItem[mBoardController.Size];
            mFoodItemPlayer2 = new IFoodItem[mBoardController.Size];

            for (int i = 0; i < mBoardController.Size; i++) 
            { 
                mFoodItemPlayer1[i]=BuildFoodItem(0, i,  mBoardController.Size );
                mFoodItemPlayer2[i]=BuildFoodItem(1, i, mBoardController.Size);
            }
        }

        public void Activity(float dt)
        {
            // this handles spawn in of new characters
            // new characters are spawned on boundaries (for now)
            //For now we decided to spawn character only from the South Border and West Border were we located the holes
            mSpawnTimer = mSpawnTimer - dt;
            int countCharacterPlayer1=0;
            int countCharacterPlayer2=0;
            if (mPoolCaracterCount > 0 && mPoolCaracterCount < kPoolCharactersSize )
            {
                if (!mAlternate)
                {
                    InitCharacter(0);
                    mAlternate = !mAlternate;
                }
                else
                {
                    InitCharacter(1);
                    mAlternate = !mAlternate;
                    mPoolCaracterCount++;
                }
            }

#if !(WINDOWS_PHONE&& !SILVERLIGHT)
            countCharacterPlayer1 = mPlayer1Characters.Count;
            countCharacterPlayer2 = mPlayer2Characters.Count;
#else
            countCharacterPlayer1 = mCountAliveCharactersPlayer1;
            countCharacterPlayer2 = mCountAliveCharactersPlayer2;
            //countTime --;
            //logTime = logTime + " ActivityTime " + dt;
            //if (countTime == 0) 
            //{
            //    countTime = 10;
            //    Debug.WriteLine(logTime);
            //    logTime = "";
            //}
#endif
           
        if ( !mIsGameOver && mSpawnTimer < 0 && mPoolCaracterCount==kPoolCharactersSize)
            {
                mSpawnTimer = 1f;
                if (mSpawnPlayer1)
                {
                    Debug.WriteLine("Player 1 desired " + GetNumDesiredCharacters(0)+" current "+countCharacterPlayer1 );
                    if (countCharacterPlayer1 < GetNumDesiredCharacters(0))
                    {
                        /*
                        if (mPlayer1LowerConnections && mPlayer1UpperConnections)
                        {
                            SpawnBoundaryCharacter(0, (mRandom.Next(2) % 2 == 0) ); 
                        }
                        else
                        {
                            SpawnBoundaryCharacter(0, mPlayer1LowerConnections);
                        }*/
                        Debug.WriteLine("Spawn player 1");
                        SpawnBoundaryCharacter(0, true);
                    }
                }
                else
                {
                    Debug.WriteLine("Player 2 desired " + GetNumDesiredCharacters(1) + " current " + countCharacterPlayer2);
                    if (countCharacterPlayer2 < GetNumDesiredCharacters(1))
                    {
                        /*
                        if (mPlayer2LowerConnections && mPlayer2UpperConnections)
                        {
                            SpawnBoundaryCharacter(1, (mRandom.Next(2) % 2 == 0));
                        }
                        else
                        {
                            SpawnBoundaryCharacter(1, mPlayer2LowerConnections);
                        }*/
                        Debug.WriteLine("Spawn player 2");
                        SpawnBoundaryCharacter(1, true);
                    }
                }
                mSpawnPlayer1 = !mSpawnPlayer1;
            }

            foreach (IRoadCharacter character in mPlayer1Characters)
            {
                if (character.StartFeeding) 
                {
                    character.StartFeeding = false;
                    
                    // TODO - is this removed from the network? 
                    mFoodItemPlayer1[character.TargetFood].Hide();

                }
                character.Activity(dt, mBoardController);
            }
            foreach (IRoadCharacter character in mPlayer2Characters)
            {
                if (character.StartFeeding)
                {
                    character.StartFeeding = false;

                    mFoodItemPlayer2[character.TargetFood].Hide();

                }
             
                character.Activity(dt, mBoardController);
            }
#if !(WINDOWS_PHONE&&!SILVERLIGHT)
#region REMOVEDEADCHARACTER
            ///Identify all dead characters and remove them from the Screen
            List<IRoadCharacter> deadCharacters1 = new List<IRoadCharacter>();
            List<IRoadCharacter> deadCharacters2 = new List<IRoadCharacter>();
            foreach (IRoadCharacter character in mPlayer1Characters) 
            {
                if (CharacterIsDead(character))
                {
                    deadCharacters1.Add(character);
                }
            }
            foreach (IRoadCharacter character in mPlayer2Characters)
            {
                if (CharacterIsDead(character))
                {
                    deadCharacters2.Add(character);
                }
            }


            foreach (IRoadCharacter character in deadCharacters1)
            {
                mPlayer1Characters.Remove(character);
                character.Destroy();
            }

            foreach (IRoadCharacter character in deadCharacters2)
            {
                mPlayer2Characters.Remove(character);
                character.Destroy();
            }
#endregion
#endif
            List<FoodRespawn> toRemove = new List<FoodRespawn>();

            foreach (FoodRespawn respawn in mRespawns)
            {
                respawn.mRespawnTime -= dt;
                if (respawn.mRespawnTime <= 0)
                {
                    respawn.mFoodItem.Respawn();
                    toRemove.Add(respawn);
                }
            }
            foreach (FoodRespawn respawn in toRemove)
            {
                mRespawns.Remove(respawn);
            }
#if!(WINDOWS_PHONE&&!SILVERLIGHT) 
            foreach (IFoodItem foodItemPlayer in mFoodItemPlayer1) 
            {
                foodItemPlayer.AnimController.Activity(dt);

            };
            foreach (IFoodItem foodItemPlayer in mFoodItemPlayer2)
            {
                foodItemPlayer.AnimController.Activity(dt);
            };
#endif
           
        }

        
 
        // unary predicate
        static bool CharacterIsDead(IRoadCharacter character)
        {
            return character.IsDead();
        }

        public void Destroy()
        {
            foreach (IRoadCharacter character in mPlayer1Characters)
            {
                character.Destroy();
            }

            mPlayer1Characters.Clear();

            foreach (IRoadCharacter character in mPlayer2Characters)
            {
                character.Destroy();
            }

            mPlayer2Characters.Clear();

            foreach (IFoodItem item in mFoodItemPlayer1)
            {
                item.Destroy();
            }
               
            foreach (IFoodItem item in mFoodItemPlayer2)
            {
                item.Destroy();
            }


        }

        public bool HandleEvent(GameEvent gameEvent)
        {
            int playerIndex = gameEvent.PlayerIndex;
            switch(gameEvent.EventType)
            {
                case GameEvent.GameEventType.SCORING_POSITION:
                {
                    if ((int)mPathBehaviors[playerIndex] < (int)PathBehavior.HasSingleConnections)
                    {
                        mNumberCharactersExp[playerIndex]++;
                        // spawn from one side, and loop
                    
                        if (playerIndex == 0)
                        {
                            if (gameEvent.WhichDomino.Controller.Row == 0 || gameEvent.WhichDomino.Controller.Row == 1)
                            {
                                mPlayer1LowerConnections = true;
                                //considered has single connection only if the path reach the holes line
                                mPathBehaviors[playerIndex] = PathBehavior.HasSingleConnections;
                            }
                            else
                            {
                                mPlayer1UpperConnections = true;
                            }
                        }
                        else
                        {
                            if (gameEvent.WhichDomino.Controller.Column == 0 || gameEvent.WhichDomino.Controller.Column == 1)
                            {
                                mPlayer2LowerConnections = true;
                                //considered has single connection only if the path reach the holes line
                                mPathBehaviors[playerIndex] = PathBehavior.HasSingleConnections;
                            }
                            else
                            {
                                mPlayer2UpperConnections = true;
                            }
                        }
                    }
                    else
                    {
                        if (mNumberCharactersExp[playerIndex] < kMaxCharactersNullScore)
                        {
                            mNumberCharactersExp[playerIndex]++;
                        }
                    }
                    break;
                }
                case GameEvent.GameEventType.SCORE:
                {
                    if ((mNumberCharactersExp[0] + mNumberCharactersExp[1]) < kMaxCharacters)
                    {
                        mNumberCharactersExp[playerIndex] = mNumberCharactersExp[playerIndex]+2;
                    }
                    else
                    {
                        int scorePlayer1 = mBoardController.CalculatePlayer1Score();
                        int scorePlayer2 = mBoardController.CalculatePlayer2Score();
                        mNumberCharactersExp[0] = Math.Max((int)(kMaxCharacters * scorePlayer1 / (scorePlayer1 + scorePlayer2)), mNumberCharactersExp[0]);
                        mNumberCharactersExp[1] = Math.Max((int)(kMaxCharacters * scorePlayer2 / (scorePlayer1 + scorePlayer2)), mNumberCharactersExp[1]);
                    }
                
                    if ((int)mPathBehaviors[playerIndex] < (int)PathBehavior.HasFullConnections)
                    {
                        // spawn from side to side
                        mPathBehaviors[playerIndex] = PathBehavior.HasFullConnections;
                        if (playerIndex == 0)
                        {
                            if (gameEvent.WhichDomino.Controller.Row == 0 || gameEvent.WhichDomino.Controller.Row == 1)
                            {
                                mPlayer1LowerConnections = true;
                            }
                            else
                            {
                                mPlayer1UpperConnections = true;
                            }
                        }
                        else
                        {
                            if (gameEvent.WhichDomino.Controller.Column == 0 || gameEvent.WhichDomino.Controller.Column == 1)
                            {
                                mPlayer2LowerConnections = true;
                            }
                            else
                            {
                                mPlayer2UpperConnections = true;
                            }
                        }
                    }
                    break;
                }
                case GameEvent.GameEventType.GAME_OVER_WIN:
                {
                    mIsGameOver = true;
                    List<IRoadCharacter> winChars = (playerIndex == 0) ? mPlayer1Characters : mPlayer2Characters;
                    List<IRoadCharacter> loseChars = (playerIndex == 0) ? mPlayer2Characters : mPlayer1Characters;
                    foreach (IRoadCharacter character in winChars)
                    {
                        character.BehaviorController.HandleEvent((int) CharacterController.CharacterEvents.To_WinDance, null);
                    }
                    foreach (IRoadCharacter character in loseChars)
                    {
                        character.BehaviorController.HandleEvent((int)CharacterController.CharacterEvents.To_Idle, null);
                    }
	                break;
                }
                case GameEvent.GameEventType.GAME_OVER_LOSE:
                {
                    mIsGameOver = true;
                    List<IRoadCharacter> winChars = (playerIndex == 0) ? mPlayer2Characters : mPlayer1Characters;
                    List<IRoadCharacter> loseChars = (playerIndex == 0) ? mPlayer1Characters : mPlayer2Characters;
                    foreach (IRoadCharacter character in winChars)
                    {
                        character.BehaviorController.HandleEvent((int)CharacterController.CharacterEvents.To_WinDance, null);
                    }
                    foreach (IRoadCharacter character in loseChars)
                    {
                        character.BehaviorController.HandleEvent((int)CharacterController.CharacterEvents.To_Lost, null);
                    }
                    break;
                }
            }
#if DEBUG
            if (gameEvent.EventType == GameEvent.GameEventType.TEST_SPAWN)
            {
                int targetFood = -1;
                List<IRoadCharacter> characterList =  gameEvent.PlayerIndex == 0 ? mPlayer1Characters : mPlayer2Characters;
                Queue<Vec2> path = new Queue<Vec2>();
                BuildCharacterPath(path, mPathBehaviors[gameEvent.PlayerIndex], gameEvent.PlayerIndex, true,ref targetFood);
                IRoadCharacter character;
                character=BuildCharacter(
                             4,
                             5,
                             CardinalPoint.E,
                             0,
                             mBoardController.Size,
                             false);
                character.TargetFood = targetFood;
                characterList.Add(character);
                characterList[characterList.Count - 1].BehaviorController.SetNavigationPath(path);
            }
#endif 
            return false;
        }

        /// <summary>
        /// Handles spawn generation of new road chars, given the location of a domino
        /// </summary>
        /// <param name="domino">spawn in on this domino</param>
        protected void SpawnBoundaryCharacter(int playerIndex, bool spawnLower)
        {
            int targetFood = -1;
            CardinalPoint direction;
            Debug.WriteLine("\nSpawn boundary character player index"+playerIndex);
            List<IRoadCharacter> playerList = (playerIndex == 0) ? mPlayer1Characters : mPlayer2Characters;
            Queue<Vec2> path = new Queue<Vec2>();

            if (playerIndex == 0)
            {
                if (spawnLower)
                {
                    direction = CardinalPoint.N;
                }
                else
                {
                    direction = CardinalPoint.S;
                }
#if (WINDOWS_PHONE&&!SILVERLIGHT)
                mCountAliveCharactersPlayer1++;
#endif
            }
            else
            {
                if (spawnLower)
                {
                    direction = CardinalPoint.E;
                }
                else
                {
                    direction = CardinalPoint.W;
                }
#if (WINDOWS_PHONE&&!SILVERLIGHT)
                mCountAliveCharactersPlayer2++;
#endif
            }
         
            // first build a path
            BuildCharacterPath(path, mPathBehaviors[playerIndex], playerIndex, spawnLower,ref targetFood);          
            IRoadCharacter character = null;
#if !(WINDOWS_PHONE&&!SILVERLIGHT)            
            character = BuildCharacter(
                0,
                0,
                direction,
                0,
                mBoardController.Size,
                false);
#else
            foreach (IRoadCharacter poolCharacter in playerList)
            {
                if (poolCharacter.IsDead())
                {
                    character = poolCharacter;
                    break;
                }
            }
              if (mPathBehaviors[playerIndex] > 0) 
            {
                character.BehaviorController.WalkSpeed = 30f;
            }
#endif
            character.TargetFood = targetFood;
            
            if (path.Count!=0)
            {
                character.X = path.Peek().X;
                character.Y = path.Peek().Y;
                character.BehaviorController.SetNavigationPath(path);
#if (WINDOWS_PHONE&&!SILVERLIGHT)
                character.BehaviorController.HandleEvent((int)CharacterController.CharacterEvents.To_SpawnIn, null);
#else
                playerList.Add(character);
#endif
            }
            else 
            {
                Debug.WriteLine("Path should not be null");
            }
        
        }
    
        protected void InitCharacter(int playerIndex)
        {
            Debug.WriteLine("\nSpawn boundary character");
            List<IRoadCharacter> playerList = (playerIndex == 0) ? mPlayer1Characters : mPlayer2Characters;
            CardinalPoint direction;
            if (playerIndex == 0)
            {
                direction = CardinalPoint.N;
            }
            else
            {
                direction = CardinalPoint.E;
            }
            IRoadCharacter character = BuildCharacter(
                    0,
                    0,
                    direction,
                    0,
                    mBoardController.Size,
                    false);
            playerList.Add(character);
        }

        /// <summary>
        /// Builds a generic food item, 
        /// food item type index defines the sprite type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <param name="boardSize"></param>
        /// <returns></returns>
        private IFoodItem BuildFoodItem(int type, int index, int boardSize )
        {
            ConstructorInfo[] info = mFoodItemType.GetConstructors();
            object[] args = (mCanvas == null) ? new object[3] : new object[4];

            args[0] = type;
            args[1] = index;
            args[2] = boardSize;

            if (mCanvas != null)
            {
                args[3] = mCanvas;
            }
            
            object obj = info[0].Invoke(args);
            IFoodItem foodItem = obj as IFoodItem;

            return foodItem;
        }

        public void AddRespawn(FoodRespawn respawn)
        {
            mRespawns.Add(respawn);
        }

        /// <summary>
        /// Builds a road character
        /// Uses a passed in type, ctor + reflection 
        /// to get this working on both XNA and Silver
        ///  This BUILDS a character
        ///  It creates a Road Character with a behavior and animation
        ///  It does NOT create the navigation path
        /// </summary>
        /// <param name="character"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="roadDirection"></param>
        /// <param name="squarePosition"></param>
        /// <param name="boardSize"></param>
        /// <param name="reachHalfSquare"></param>
        /// <returns></returns>
        private IRoadCharacter BuildCharacter(int row, 
            int column, 
            CardinalPoint roadDirection,
            float squarePosition, 
            int boardSize, 
            bool reachHalfSquare)
        {
            object[] args = (mCanvas == null) ? new object[6] : new object[7];

            args[0] = row;
            args[1] = column;
            args[2] = roadDirection;

            // used for FRB layering
            args[3] = mGopherLayer;

            // not used
            args[4] = boardSize;

            // not used
            args[5] = reachHalfSquare;

            if (mCanvas != null)
            {
                args[6] = mCanvas;
            }

            ConstructorInfo[] info = mControllerType.GetConstructors();
            object obj = info[0].Invoke(args);
                    
            IRoadCharacter character = obj as IRoadCharacter;

            return character;
        }

        /// <summary>        
        ///  This creates the navigation path
        /// </summary>
        /// <param name="character"></param>
        private void BuildCharacterPath(Queue<Vec2> path, PathBehavior pathBehavior, int playerIndex, bool spawnLower,ref int targetFood)
        {
            Vec2 source = GetRandomBoundaryPoint(playerIndex, spawnLower).TransformToWorldSpace(mBoardController.Size);
            Vec2 target = new Vec2();
            
            switch (pathBehavior)
            {
                case PathBehavior.NoConnections:
                    {
                        Debug.WriteLine("NoConnections");
                        // random target on same side 
                        while (true)
                        {
                            target = GetRandomBoundaryPoint(playerIndex, spawnLower).TransformToWorldSpace(mBoardController.Size);
                            if (target != source)
                            {
                                break;
                            }
                        }
                        Queue<Vec2> subPath = new Queue<Vec2>();
                        IAIManager.Instance.GetPath(source, target, subPath);

                        // append sub path to path
                        while (subPath.Count > 0)
                        {
                            Vec2 wayPoint = subPath.Dequeue();
                            path.Enqueue(wayPoint);
                        }

                        break;
                    }
                case PathBehavior.HasSingleConnections:
                    {
                        Debug.WriteLine("Single Connections");
                        while (true)
                        {
                            target = GetRandomBoundaryPoint(playerIndex, spawnLower).TransformToWorldSpace(mBoardController.Size);
                            if (target != source)
                            {
                                break;
                            }
                        }
                        
                        Queue<Vec2> subPath = new Queue<Vec2>();
                        IAIManager.Instance.GetPathWithInsideViaPoint(source, target, subPath,playerIndex,spawnLower);
                        // append sub path to path
                        while (subPath.Count > 0)
                        {
                            Vec2 wayPoint = subPath.Dequeue();
                            path.Enqueue(wayPoint);
                        }

                        break;
                    }
                case PathBehavior.HasFullConnections:
                    {
                        Debug.WriteLine("Full Connections");
                        // get a target on the other side
                        BoardController.GridPoint g = GetRandomBoundaryPoint(playerIndex, !spawnLower);
                        if (playerIndex == 0)
                        {
                            targetFood = g.mColumn;
                        }
                        else
                        {
                            targetFood = g.mRow;
                        }
                        target = g.TransformToWorldSpace(mBoardController.Size);
                            
                        Queue<Vec2> subPath = new Queue<Vec2>();
                        IAIManager.Instance.GetPath(source, target, subPath);

                        // append sub path to path
                        while (subPath.Count > 0)
                        {
                            Vec2 wayPoint = subPath.Dequeue();
                            path.Enqueue(wayPoint);
                        }
                        break;
                    }
            }

        }

        // gets a random point on a character's boundaries
        BoardController.GridPoint GetRandomBoundaryPoint(int playerIndex, bool spawnLower)
        {
            // spawn at a row, column
            // defined by spawnLower ? lower{row,column} : upper{row,column}            
            int randomIndex = mRandom.Next(mBoardController.Size);

            int row = (playerIndex == 0) ? (spawnLower ? -1 : mBoardController.Size) : randomIndex;
            int column = (playerIndex == 0) ? randomIndex : (spawnLower ? -1 : mBoardController.Size);

            return new BoardController.GridPoint(row, column);

        }

       

#if false

        /// <summary>
        /// Handles spawn generation of new road chars (in the road)
        /// spwawn given the location of a domino
        /// </summary>
        /// <param name="domino">spawn in on this domino</param>
        protected void SpawnRoadCharacter(DominoController domino, float playerIndex)
        {
            List<IRoadCharacter> playerList = (playerIndex == 1) ? mPlayer1Characters : mPlayer2Characters;

            if (domino.IsHorizontal())
            {
                // spawn on small ends
                if (domino.Column == 1 && 
                    mBoardController.HasSquareExit(domino.Row, 0, CardinalPoint.W))
                {

                    playerList.Add(BuildCharacter(
                            domino.Row, 
                            0, 
                            CardinalPoint.E, 
                            0, 
                            mBoardController.Size, 
                            false));
                }
                else if (domino.Column == (mBoardController.Size - 2) && 
                    mBoardController.HasSquareExit(domino.Row, mBoardController.Size - 1, CardinalPoint.E))
                {
                    playerList.Add(
                        BuildCharacter(domino.Row, 
                        mBoardController.Size - 1, 
                        CardinalPoint.W, 
                        0,
                        mBoardController.Size, 
                        false));
                }

                // try spawing on bottom of board
                if (domino.Row == 0)
                {
                    for (int col = -1; col < 2; col++)
                    {
                        if (mBoardController.HasSquareExit(0, domino.Column + col, CardinalPoint.S))
                        {
                            playerList.Add(
                                BuildCharacter(0, 
                                    domino.Column + col, 
                                    CardinalPoint.N, 
                                    0, 
                                    mBoardController.Size, 
                                    false));
                        }
                    }
                }
                // try spawning at top of board
                else if (domino.Row == mBoardController.Size - 1)
                {
                    for (int col = -1; col < 2; col++)
                    {
                        if (mBoardController.HasSquareExit(domino.Row, domino.Column + col, CardinalPoint.N))
                        {
                            playerList.Add(
                                BuildCharacter(
                                    mBoardController.Size - 1, 
                                    domino.Column + col, 
                                    CardinalPoint.S, 
                                    0, 
                                    mBoardController.Size, 
                                    false));
                        }
                    }
                }
            }
            else //Domino Vertical
            {
                // try spawing on short ends
                if (domino.Row == 1 && 
                    mBoardController.HasSquareExit(0, domino.Column, CardinalPoint.S))
                {
                    playerList.Add(
                        BuildCharacter(
                        0, 
                        domino.Column, 
                        CardinalPoint.N, 
                        0, 
                        mBoardController.Size, 
                        false));
                }
                else if (domino.Row == (mBoardController.Size - 2) && 
                    mBoardController.HasSquareExit(mBoardController.Size - 1, domino.Column, CardinalPoint.N))
                {
                    playerList.Add(
                        BuildCharacter(
                        mBoardController.Size - 1, 
                        domino.Column, 
                        CardinalPoint.S, 
                        0, 
                        mBoardController.Size, 
                        false));
                }

                // try spawning along long length
                if (domino.Column == 0)
                {
                    for (int row = -1; row < 2; row++)
                    {
                        if (mBoardController.HasSquareExit(domino.Row + row, 0, CardinalPoint.W))
                        {
                            playerList.Add(
                                BuildCharacter(
                                domino.Row + row, 
                                0, 
                                CardinalPoint.E, 
                                0,
                                mBoardController.Size, 
                                false));
                        }
                    }
                }
                else if (domino.Column == mBoardController.Size - 1)
                {
                    for (int row = -1; row < 2; row++)
                    {
                        if (mBoardController.HasSquareExit(domino.Row + row, mBoardController.Size - 1, CardinalPoint.E))
                        {
                            playerList.Add(
                                BuildCharacter(
                                domino.Row + row, 
                                mBoardController.Size - 1,
                                CardinalPoint.W, 
                                0, 
                                mBoardController.Size,
                                false));
                        }
                    }
                }

            }
        }
#endif
    }
}
