using System;
using TreeEditor;
using Unity.Cinemachine;
using UnityEngine;

public class SimpleWorld : MonoBehaviour
{
	[SerializeField] private GameObject prefabCharacter;
	
	[SerializeField] private DungeonTile prefabFloor;
	[SerializeField] private DungeonTile prefabWall;
	[SerializeField] private DungeonTile prefabWater;
	[SerializeField] private DungeonTile prefabTree;

	[SerializeField] private Vector2Int sizeWorld;

	[SerializeField] private float scaleNoise;
	[SerializeField] private float plusNoise;

	private void Start()
	{
		GenerateWorld();
	}

	void GenerateWorld()
	{
		for (int x = 0; x < sizeWorld.x; x++)
		{
			for (int y = 0; y < sizeWorld.y; y++)
			{
				float value = Mathf.PerlinNoise(x * scaleNoise, y * scaleNoise);
				Debug.Log(value);
				if (value > 0.3f && value < 0.7f)
				{
					Instantiate(prefabFloor, new Vector3(x, y), Quaternion.identity);
				}
				else if (value > 0.7f)
				{
					Instantiate(prefabWall, new Vector3(x, y), Quaternion.identity);
				}
				else if(value < 0.3f)
				{
					Instantiate(prefabWater, new Vector3(x, y), Quaternion.identity);
				}
			}
		}
		FindAnyObjectByType<CinemachineCamera>().Follow = Instantiate(prefabCharacter).transform;
		GenerateTrees();
	}

	void GenerateTrees()
	{
		for (int x = 0; x < sizeWorld.x; x++)
		{
			for (int y = 0; y < sizeWorld.y; y++)
			{
				float value = Mathf.PerlinNoise( plusNoise * scaleNoise,  plusNoise * scaleNoise);
				plusNoise++;
				Debug.Log(value);
				DungeonTile dungeonTile = TileCheck(new Vector2Int(x,y));
				
				if (value < 0.3f && dungeonTile.tileType == DungeonTile.TileType.Grass)
				{
					Instantiate(prefabTree, new Vector3(x, y), Quaternion.identity);
				}
			}
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
}
