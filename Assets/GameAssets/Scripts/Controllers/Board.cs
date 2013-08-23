using UnityEngine;
using System.Collections;

using DeltaCommon.Component;
using DeltaCommon.Entities;

public class Board : MonoBehaviour, IBoard {

	public BoardController Controller { get; set; }
	
	public void Initialize(int size) {
		Controller = new BoardController(size);
	}
	
    /// <summary>
    /// World space inside outside test.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool Contains(Vector2 point)
    {
		float x = gameObject.transform.position.x;
		float y = gameObject.transform.position.y;
        float dimension = (Controller.Size/2 + 1) * Square.kPixelSize;
        bool xBounds = (x - dimension) <= point.x && point.x <= (x + dimension);
        bool yBounds = (y - dimension) <= point.y && point.y <= (y + dimension);
        return xBounds && yBounds;
    }
}
