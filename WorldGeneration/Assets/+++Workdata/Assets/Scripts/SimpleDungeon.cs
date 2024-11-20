using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleDungeon : MonoBehaviour
{

	enum Himmelsrichtung
	{
		North,
		East,
		South,
		West
	}
	
	//Important parameters for creation.
	/*
	 * Room Size (Vector2 int)
	 * Room exits (Vector2 int)
	 * Corridors curve (bool)
	 * Branching corridors (bool)
	 * Probability (if a corridor has branches - float 0-1)
	 *
	 * Corridor length (Vector2 int)
	 *
	 * Start in room or corridor
	 *
	 * Non-Walkable areas in rooms ( floatt) -> but all exists have to be reachable.
	 *
	 *
	 * Door location
	 */

	[SerializeField] private DungeonTile floorTile;

	[SerializeField, Tooltip("X: From; Y: To")]
	private Vector2Int roomSize;

	private Transform parentDungeon;

	private void Start()
	{
		parentDungeon = new GameObject().transform;
		parentDungeon.gameObject.name = "DungeonPart";

		StartCoroutine(GenerateDungeonRoom(new Vector2Int(0, 0), Random.Range(roomSize.x, roomSize.y), Random.Range(roomSize.x, roomSize.y)));
	}

	IEnumerator GenerateDungeonRoom(Vector2Int startingPosition, int xSize, int ySize)
	{
		for (int x = 0; x < xSize; x++)
		{
			for (int y = 0; y < ySize; y++)
			{
				DungeonTile dungeonTile = Instantiate(floorTile, new Vector3(x, y), Quaternion.identity, parentDungeon);
				dungeonTile.walkable = true;
				yield return null;
			}
		}

		for (int i = 0; i < 100; i++)
		{
			//Find possible exits
			//Corridors are not allowed next to each other
			(Vector2Int, Himmelsrichtung) value =  GetPossibleCorridorPosition(xSize, ySize);
			Vector2Int corridorPosition = startingPosition + value.Item1;
			Himmelsrichtung direction = value.Item2;

			if (Physics2D.Raycast(corridorPosition.ToVector3() + Vector3.back * 0.2f, Vector3.back))
			{
				Debug.Log("Here is already an exit!");
			}
			else
			{
				Vector3 checkDirection = (direction == Himmelsrichtung.North || direction == Himmelsrichtung.South) ? Vector3.right : Vector3.up;
				
				
				if (Physics2D.Raycast(corridorPosition.ToVector3() + checkDirection + Vector3.back * 0.2f, Vector3.forward) || Physics2D.Raycast(corridorPosition.ToVector3() - checkDirection + Vector3.back * 0.2f, Vector3.forward))
				{
					Debug.Log("Here is already an exit next to the position!");
				}
				else
				{
					Instantiate(floorTile, new Vector3(corridorPosition.x, corridorPosition.y), Quaternion.identity, parentDungeon);
					Debug.Log(direction.ToString());
				}
			}
		}
	}

	(Vector2Int, Himmelsrichtung) GetPossibleCorridorPosition(int x, int y)
	{
		Vector2Int[] possiblePosition =
		{
			 new (Random.Range(0, x), y),
			 new (x , Random.Range(0, y)),
			 new (Random.Range(0, x), -1),
			 new (-1, Random.Range(0, y)),
		};
		int ranNumber = Random.Range(0, possiblePosition.Length);

		return (possiblePosition[ranNumber], (Himmelsrichtung)ranNumber);
	}
}

public static class Extensions
{
	public static Vector3 ToVector3(this Vector2Int vector2Int)
	{
		return new Vector3(vector2Int.x, vector2Int.y);
	}
}

