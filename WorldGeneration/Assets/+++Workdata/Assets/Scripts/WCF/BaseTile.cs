using System.Collections.Generic;
using UnityEngine;

public class BaseTile : MonoBehaviour
{
	public Vector2Int gridPosition;
	public SO_TileData tileData;

	public List<SO_TileData> possibleTiles;

	public void InitTile(SO_TileData newTileData)
	{
		tileData = newTileData;
		GetComponent<SpriteRenderer>().sprite = tileData.tileSprite;
	}
}
