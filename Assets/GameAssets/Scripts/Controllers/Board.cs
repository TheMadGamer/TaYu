using UnityEngine;
using System.Collections;

using DeltaCommon.Component;
using DeltaCommon.Entities;

public class Board : MonoBehaviour, IBoard {

	public BoardController Controller { get; set; }
}
