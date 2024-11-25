using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonTile : MonoBehaviour
{
	[SerializeField] private Sprite[] possibleTileSprite;
	[SerializeField, Range(0, 1)] private float probabilityDifference;
	public bool walkable;

	private void Start()
	{
		if (possibleTileSprite.Length > 0)
		{
			if (Random.value < probabilityDifference)
			{
				GetComponent<SpriteRenderer>().sprite = possibleTileSprite[Random.Range(0, possibleTileSprite.Length)];
			}
			else
			{
				GetComponent<SpriteRenderer>().sprite = possibleTileSprite[0];
			}
		}
	}
}
