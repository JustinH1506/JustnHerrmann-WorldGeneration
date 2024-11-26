using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
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
     * Room count (int)
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
	[SerializeField] private DungeonTile wallTile;
	[SerializeField] private DungeonTile backgroundTile;
	
	[SerializeField] bool useSeed;
	[SerializeField] private int seed;
	[SerializeField, Tooltip("X: From; Y: To")] private Vector2Int roomCount;
	[SerializeField, Tooltip("X: From; Y: To")] private Vector2Int roomSize;
	[SerializeField, Tooltip("X: From; Y: To")] private Vector2Int possibleExitsPerRoom;
	[SerializeField, Tooltip("X: From; Y: To")] private Vector2Int corridorLenght;
	[SerializeField,  Range(0,1)] private float corridorCurviness;
	[SerializeField, Range(0, 1)] private float corridorBranchingProbability;

	private Transform parentDungeon;
	[SerializeField] private int totalRoomCount;
	[SerializeField] private int roomCounter;

	private Vector2Int upperRightCorner;
	private Vector2Int lowerLeftCorner;

	private int runningCoroutine = 0;
	private bool isGenerationFinished = false;


	struct RoomInfo
	{
		public Vector2Int startingPosition;
		public Vector2Int size;
		public int exitCount;
	}
	
	struct CorridorInfo
	{
		public Vector2Int startingPosition;
		public int lenght;
		public Himmelsrichtung direction;

		public CorridorInfo(Vector2Int newStartingPoint, Himmelsrichtung newHimmelsrichtung)
		{
			startingPosition = newStartingPoint;
			direction = newHimmelsrichtung;
			lenght = 0;
		}
	}

	private void Start()
	{
		GenerateDungeon();
	}
	
	void GenerateDungeon()
	{
		if (useSeed)
		{
			Random.InitState(seed);
		}
        
		parentDungeon = new GameObject().transform;
		parentDungeon.gameObject.name = "DungeonParent";

		totalRoomCount = Random.Range(roomCount.x, roomCount.y + 1);
        
		StartCoroutine(GenerateDungeonRoom(RandomRoomInfo(new Vector2Int(0,0))));
	}
	
	void Reset()
	{
		upperRightCorner = new Vector2Int();
		lowerLeftCorner = new Vector2Int();
		StopAllCoroutines();
		Destroy(parentDungeon.gameObject);
		roomCounter = 0;
		isGenerationFinished = false;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.G))
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
		
		if (!isGenerationFinished && runningCoroutine == 0)
		{
			if (totalRoomCount <= roomCounter)
			{
				isGenerationFinished = true;
				Debug.Log("generateBackground");
				StartCoroutine(GenerateBackground());
			}
			else
			{
				Reset();
				GenerateDungeon();
			}
		}
	}

	IEnumerator GenerateDungeonRoom(RoomInfo roomInfo)
	{
		runningCoroutine++;
		for (int x = 0; x < roomInfo.size.x; x++)
		{
			for (int y = 0; y < roomInfo.size.y; y++)
			{
				Vector2Int newTilePos = new Vector2Int(x, y) + roomInfo.startingPosition;
				if (TileCheck(newTilePos))
				{
					continue;
				}
				Instantiate(floorTile, newTilePos.ToVector3(), Quaternion.identity, parentDungeon);
				AdjustMapBoundaries(newTilePos);
			}
		}
		yield return null;

		roomCounter++;
		if (roomCounter > totalRoomCount)
		{
			StopAllCoroutines();
			runningCoroutine = 0;
			yield break;
		}

		int maxAttempts = 100; // if time -> add function that calculates possible exits.
		for (int i = 0; i < roomInfo.exitCount;)
		{
			CorridorInfo? newCorridorInfo = GetCorridorStartingPoint(roomInfo);

			// add loop
			
			if (newCorridorInfo != null)
			{
				CorridorInfo ci = (CorridorInfo)newCorridorInfo;
				ci.lenght = Random.Range(corridorLenght.x, corridorLenght.y);
				StartCoroutine(GenerateCorridor(ci));
				i++;
			}
			
			maxAttempts--;
			if (maxAttempts <= 0)
			{
				break;
			}
		}

		runningCoroutine--;
	}

	CorridorInfo? GetCorridorStartingPoint(RoomInfo roomInfo)
	{
		//Find possible exits
		//Corridors are not allowed next to each other
		(Vector2Int, Himmelsrichtung) value =  GetPossibleCorridorPosition(roomInfo.size.x, roomInfo.size.y);
		Vector2Int corridorPosition = roomInfo.startingPosition + value.Item1;
		Himmelsrichtung direction = value.Item2;

		CorridorInfo? corridorInfo = new CorridorInfo(corridorPosition, direction);

		if (TileCheck(corridorPosition))
		{
			//Debug.Log("Here is already an exit!");
		}
		else
		{
			Vector2Int checkDirection = (direction == Himmelsrichtung.North || direction == Himmelsrichtung.South) ? Vector2Int.right : Vector2Int.up;
			
			if (TileCheck(corridorPosition + checkDirection) || TileCheck(corridorPosition - checkDirection))
			{
				//Debug.Log("Here is already an exit next to the position!");
			}
			else
			{
				return corridorInfo;
			}
		}

		return null;
	}

	IEnumerator GenerateCorridor(CorridorInfo corridorInfo)
	{
		runningCoroutine++;
		Vector2Int currentPos = corridorInfo.startingPosition;
		Himmelsrichtung currentDirection = corridorInfo.direction;
		
		for (int i = 0; i < corridorInfo.lenght; i++)
		{
			if (TileCheck(currentPos))
			{
				yield break;
			}
			
			Instantiate(floorTile, currentPos.ToVector3(), Quaternion.identity, parentDungeon);
			
			AdjustMapBoundaries(currentPos);

			if (Random.value < corridorCurviness)
			{
				if (Random.value > 0.5f)
				{
					//Curve nach rechts 
					currentDirection++;
				}
				else
				{
					//curve nach links
					currentDirection--;
				}

				if ((int)currentDirection == 4)
				{
					currentDirection = Himmelsrichtung.North;
				}
				else if((int)currentDirection == -1)
				{
					currentDirection = Himmelsrichtung.West;
				}
			}
			
			currentPos += HimmelsrichtungToVector(currentDirection);
		}
		yield return null;

		StartCoroutine(GenerateDungeonRoom(RandomRoomInfo(currentPos)));
		runningCoroutine--;
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

	Vector2Int HimmelsrichtungToVector(Himmelsrichtung himmelsrichtung)
	{
		switch (himmelsrichtung)
		{
			case Himmelsrichtung.North:
				return Vector2Int.up;
			case Himmelsrichtung.East:
				return Vector2Int.left;
			case Himmelsrichtung.South:
				return Vector2Int.down;
			case Himmelsrichtung.West:
				return Vector2Int.left;
			default:
				return Vector2Int.zero;
		}
	}

	DungeonTile TileCheck(Vector2Int checkPosition)
	{
		RaycastHit2D hit = Physics2D.Raycast(checkPosition.ToVector3() + Vector3.back * 0.2f, Vector3.forward);

		if (hit)
		{
			return Physics2D.Raycast(checkPosition.ToVector3() + Vector3.back * 0.2f, Vector3.forward).transform.GetComponent<DungeonTile>();
		}
		else
		{
			return null;
		}
	}
	
	RoomInfo RandomRoomInfo(Vector2Int startingPoint)
	{
		return new RoomInfo()
		{
			startingPosition = startingPoint,
			size = new Vector2Int(Random.Range(roomSize.x, roomSize.y), Random.Range(roomSize.x, roomSize.y)),
			exitCount = Random.Range(possibleExitsPerRoom.x, possibleExitsPerRoom.y + 1)
		};
	}

	void AdjustMapBoundaries(Vector2Int currentPoint)
	{
		upperRightCorner.x = Mathf.Max(upperRightCorner.x, currentPoint.x);
		lowerLeftCorner.x = Mathf.Min(lowerLeftCorner.x, currentPoint.x);

		upperRightCorner.y = Mathf.Max(upperRightCorner.y, currentPoint.y);
		lowerLeftCorner.y = Mathf.Min(lowerLeftCorner.y, currentPoint.y);
	}

	IEnumerator GenerateBackground()
	{
		for (int x = lowerLeftCorner.x; x < upperRightCorner.x + 2; x++)
		{
			for (int y = lowerLeftCorner.y; y < upperRightCorner.y + 2; y++)
			{
				Vector2Int currentPos = new Vector2Int(x, y);
				DungeonTile dt = TileCheck(currentPos);
				
				if (dt && dt.walkable)
				{
					if (!TileCheck(currentPos + Vector2Int.up))
					{
						Instantiate(wallTile, (currentPos + Vector2Int.up).ToVector3(), Quaternion.identity, parentDungeon);
					}
				}
				else if(!dt)
				{
					Instantiate(backgroundTile, currentPos.ToVector3(), Quaternion.identity, parentDungeon);
				}
			}
			yield return null;
		}
	}
}

public static class Extensions
{
	public static Vector3 ToVector3(this Vector2Int vector2Int)
	{
		return new Vector3(vector2Int.x, vector2Int.y);
	}
}