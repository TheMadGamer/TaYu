using UnityEngine;

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using DeltaCommon.Entities;
using DeltaCommon.Managers;

 public class DominoGenerator {
	// Domino prefab.
	public GameObject mDomino;

    // this should be hidden, accessible via functions
    int[] mRandomLabelArray;

    // this should be hidden, accessible via functions
    int mRandomLabelArrayIndex = 0;

    public const int kNumberDominoSet = 4;

    private const int kNumDominoTypes = 28;

    int mStartPosition = 0;

    // private
    static System.Random sRandomGenerator;
	
    /// <summary>
    /// Generates a game object with a domino
    /// </summary>
    public Domino GetNextDomino()
    {
        int label = GetNextLabel();
		GameObject d = (GameObject)GameObject.Instantiate(mDomino);
		Domino domino = d.GetComponent<Domino>();
		domino.Initialize(label, kWaterExitDominoDatas[label], mStartPosition);
		tk2dSprite sprite = domino.mSpriteObject.GetComponent<tk2dSprite>();
		sprite.SetSprite("Tile" + label.ToString());
		return domino;
    }

    /// <summary>
    /// Generates game object with a domino
    /// </summary>
    public Domino CreateDomino(int labelId)
    {
		GameObject d = (GameObject)GameObject.Instantiate(mDomino);
		Domino domino = d.GetComponent<Domino>();
		domino.Initialize(labelId, kWaterExitDominoDatas[labelId], mStartPosition);
		tk2dSprite sprite = domino.mSpriteObject.GetComponent<tk2dSprite>();
		sprite.SetSprite("Tile" + labelId.ToString());
		return domino;
	}

    public int CountDominoPlayed
    {
        get
        {
            return (mRandomLabelArrayIndex);
        }
    }

    #region WATER EXIT INITIALIZATION

    static readonly WaterExit[][] kWaterExitDominoDatas = new WaterExit[kNumDominoTypes][];

    /// <summary>
    /// Initializes common sprite data
    /// </summary>
    public DominoGenerator(int startPosition, GameObject domino)
    {
		mDomino = domino;
        mStartPosition = startPosition;

        InitializeLabelArray();

        //domino 0
        kWaterExitDominoDatas[0] = new WaterExit[3]{
             new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
             new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
             new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S}
            };

        kWaterExitDominoDatas[1] = new WaterExit[3]{
            //domino 1
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N}
        };

        kWaterExitDominoDatas[2] = new WaterExit[3]{
            //domino 2
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S}
        };

        kWaterExitDominoDatas[3] = new WaterExit[3]{
            //domino 3
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N}
        };

        kWaterExitDominoDatas[4] = new WaterExit[3]{
            //domino 4
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.S}
        };

        kWaterExitDominoDatas[5] = new WaterExit[3]{
            //domino 5
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.E}
        };

        kWaterExitDominoDatas[6] = new WaterExit[3]{
            //domino 6
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N}
        };

        kWaterExitDominoDatas[7] = new WaterExit[3]{
            //domino 7
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S}
        };

        kWaterExitDominoDatas[8] = new WaterExit[3]{
            //domino 8
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N}
        };

        kWaterExitDominoDatas[9] = new WaterExit[3]{
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

        kWaterExitDominoDatas[11] = new WaterExit[3]{
            //domino 11
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S}
        };

        kWaterExitDominoDatas[12] = new WaterExit[3]{
            //domino 12
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N}
        };

        kWaterExitDominoDatas[13] = new WaterExit[3]{
            //domino 13
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.S}
        };

        kWaterExitDominoDatas[14] = new WaterExit[3]{
            //domino 14
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.E}
        };

        kWaterExitDominoDatas[15] = new WaterExit[3]{
            //domino 15
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N}
        };

        kWaterExitDominoDatas[16] = new WaterExit[3]{
            //domino 16
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.E}
        };

        kWaterExitDominoDatas[17] = new WaterExit[3]{
            //domino 17
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.E}
        };

        kWaterExitDominoDatas[18] = new WaterExit[3]{
            //domino 18
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N}
        };

        kWaterExitDominoDatas[19] = new WaterExit[3]{
            //domino 19
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S}
        };

        kWaterExitDominoDatas[20] = new WaterExit[3]{
            //domino 20
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N}
        };

        kWaterExitDominoDatas[21] = new WaterExit[3]{
            //domino 21
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.E}
        };

        kWaterExitDominoDatas[22] = new WaterExit[3]{
            //domino 22
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S}
        };

        kWaterExitDominoDatas[23] = new WaterExit[3]{
            //domino 23
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N}
        };

        kWaterExitDominoDatas[24] = new WaterExit[3]{
            //domino 24
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.E}
        };

        kWaterExitDominoDatas[25] = new WaterExit[3]{
            //domino 25
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.W},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N}
        };

        kWaterExitDominoDatas[26] = new WaterExit[3]{
            //domino 26
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.S}
        };

        kWaterExitDominoDatas[27] = new WaterExit[3]{
            //domino 27
            new WaterExit{mSquareID=-1,mCardinalPoint=CardinalPoint.S},
            new WaterExit{mSquareID=0,mCardinalPoint=CardinalPoint.N},
            new WaterExit{mSquareID=1,mCardinalPoint=CardinalPoint.N}
        };

    }

    // end of array declaration
    #endregion


    #region LABEL INDEXING

    /// <summary>
    /// The label will be randomly picked from an Array.
    /// The array is initialize putting all the dominos label in it and then shuffle
    /// everything.
    /// </summary>
    void InitializeLabelArray()
    {
        int mLabel = 0;
#if DEBUG
        sRandomGenerator = new System.Random(0);
#else
        sRandomGenerator = new System.Random();
#endif
        mRandomLabelArray = new int[kNumDominoTypes * kNumberDominoSet];
        mRandomLabelArrayIndex = 0;
        for (int i = 0; i < kNumDominoTypes * kNumberDominoSet; i++)
        {
            if ((i % kNumDominoTypes) == 0)
            {
                mLabel = 0;
            }
            mRandomLabelArray[i] = mLabel;
            mLabel++;
        }
        for (int i = 0; i < kNumDominoTypes * kNumberDominoSet; i++)
        {
            int mRandomIndex = sRandomGenerator.Next(i, kNumDominoTypes * kNumberDominoSet);
            mLabel = mRandomLabelArray[mRandomIndex];
            mRandomLabelArray[mRandomIndex] = mRandomLabelArray[i];
            mRandomLabelArray[i] = mLabel;
        }
    }
#if DEBUG
    public void SetExplicitLabel(int index, int value)
    {
        mRandomLabelArray[index] = value;
    }
    public void ResetLabelIndex()
    {
        mRandomLabelArrayIndex = 0;
    }
#endif


    public int GetPreviousLabel()
    {
        if (mRandomLabelArrayIndex > 0)
        {
            mRandomLabelArrayIndex--;
        }
        return mRandomLabelArray[mRandomLabelArrayIndex];
    }

    public int GetNextLabel()
    {
        int label;

        if (mRandomLabelArrayIndex < kNumberDominoSet * kNumDominoTypes)
        {
            label = mRandomLabelArray[mRandomLabelArrayIndex];
            mRandomLabelArrayIndex++;
        }
        else
        {
            label = 0;
        }
        return label;
    }

    #endregion
}

