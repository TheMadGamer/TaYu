using System;
using System.Collections.Generic;

using UnityEngine;

using DeltaCommon.Entities;

public class Bag : IBag
{

    public Bag(int nSlots, int boardSize, bool player1Bag)
    :
        base(nSlots, boardSize, player1Bag)
    {
    } 

    public override void Activity()
    {
        for (int i = 0; i < mDominoes.Length; i++)
        {
            if (mDominoes[i] != null)
            {
                (mDominoes[i] as Domino).Activity();
            }
        }
    }

    /// <summary>
    /// xna vector2
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool Contains(Vector2 point)
    {
        foreach (Domino domino in mDominoes)
        {
            if (domino!= null && domino.Contains(point))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// xna vector2
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public Domino GetSelection(Vector2 point)
    {
        foreach (Domino domino in mDominoes)
        {
            if (domino != null && domino.Contains(point))
            {
                return domino;
            }
        }
        return null;
    }
}

