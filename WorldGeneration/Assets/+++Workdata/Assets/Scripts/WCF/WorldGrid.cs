using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldGrid : MonoBehaviour
{
	[SerializeField] private Vector2Int size;
	private BaseTile[,] worldGrid;

	private List<BaseTile> allNotSetTiles = new List<BaseTile>();

	[SerializeField] private BaseTile prefabBaseTile;
	[SerializeField] private Transform baseTileParent;

	private void Awake()
	{
		worldGrid = new BaseTile[size.x, size.y];
		for (int x = 0; x < size.x; x++)
		{
			for (int y = 0; y < size.x; y++)
			{
				worldGrid[x, y] = Instantiate(prefabBaseTile, new Vector3(x, y), Quaternion.identity, baseTileParent);
				worldGrid[x, y].gridPosition = new Vector2Int(x, y);
				allNotSetTiles.Add(worldGrid[x,y]);
			}
		}
	}

	public void TileWasSet(BaseTile setBaseTile)
	{
		allNotSetTiles.Remove(setBaseTile);
	}

	public BaseTile GetBaseTileAt(Vector2Int pos)
	{
		if (pos.x < 0 || pos.y < 0 || pos.x >= size.x || pos.y >= size.y)
		{
			return null;
		}
		
		return worldGrid[pos.x,pos.y];
	}

	public BaseTile GetRandomBaseTile()
	{
		return allNotSetTiles[Random.Range(0, allNotSetTiles.Count)];
	}

	public int GetTotalTileCount()
	{
		return size.x * size.y;
	}

	public float GetMapSize()
	{
		return size.x;
	}
}
