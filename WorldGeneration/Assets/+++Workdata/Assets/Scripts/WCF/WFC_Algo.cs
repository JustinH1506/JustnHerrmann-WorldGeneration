using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WFC_Algo : MonoBehaviour
{
    private WorldGrid worldGrid;

    [SerializeField] private SO_Rules rules;

    private int counterSetTiles = 0;
    
    private List<BaseTile> currentPath = new List<BaseTile>();

    [SerializeField] private float waitTime;

    [SerializeField] private bool visualise = true;

    [SerializeField] private float biomePosition;

    private void Start()
    {
        StartGeneration();
    }

    async Awaitable StartGeneration()
    {
        worldGrid = FindAnyObjectByType<WorldGrid>();
        
        // Add loop - check if tile already set - check if all are set

        BaseTile startingTile = worldGrid.GetRandomBaseTile();
        
        while (counterSetTiles < worldGrid.GetTotalTileCount())
        {
            while (startingTile.tileData)
            {
                startingTile = worldGrid.GetRandomBaseTile();
            }
            
            currentPath.Clear();
            currentPath.Add(startingTile);
            LineUpdate();
            
            startingTile.InitTile(startingTile.possibleTiles[Random.Range(0, startingTile.possibleTiles.Count)]);
            
            counterSetTiles++;
            worldGrid.TileWasSet(startingTile);

            List<SO_TileData> currentPossibleTileDatas = new List<SO_TileData>(startingTile.possibleTiles);
            
            for (int i = 0; i < startingTile.possibleTiles.Count; i++)
            {
                if (startingTile.possibleTiles[i] == startingTile.tileData)
                {
                    continue;
                }
                
                currentPossibleTileDatas.Remove(startingTile.possibleTiles[i]);
                
                HashSet<SO_TileData.TileType> possibleNeighbors =  CalculatePossibleNeighbors(currentPossibleTileDatas);
                await CheckNeighbors(startingTile.gridPosition, possibleNeighbors);
            }

            startingTile.possibleTiles = new List<SO_TileData>(currentPossibleTileDatas);
        }
        Destroy(lineRenderer.gameObject);
    }

    async Awaitable CheckNeighbors(Vector2Int currentPos, HashSet<SO_TileData.TileType> possibleNeighbors)
    {
        await CheckNeighborPossibleTiles(currentPos + Vector2Int.up, possibleNeighbors);
        await CheckNeighborPossibleTiles(currentPos + Vector2Int.right, possibleNeighbors);
        await CheckNeighborPossibleTiles(currentPos + Vector2Int.down, possibleNeighbors);
        await CheckNeighborPossibleTiles(currentPos + Vector2Int.left, possibleNeighbors);
    }

    async Awaitable CheckNeighborPossibleTiles(Vector2Int currentPos, HashSet<SO_TileData.TileType> possibleNeighbors)
    {
        //One to the left 
        
        BaseTile neighborTile = worldGrid.GetBaseTileAt(currentPos);

        if (!neighborTile || neighborTile.tileData != null || currentPath.Contains(neighborTile))
        {
            return;
        }
        
        currentPath.Add(neighborTile);
        LineUpdate();

        if (visualise)
        {
            await Awaitable.WaitForSecondsAsync(waitTime);
        }
        
        List<SO_TileData> currentPossibleTileDatas = new List<SO_TileData>(neighborTile.possibleTiles);
        
        foreach (SO_TileData tileData in neighborTile.possibleTiles)
        {
            if (possibleNeighbors.Contains(tileData.type))
            {
                continue;
            }

            currentPossibleTileDatas.Remove(tileData);
            
            await CheckNeighbors(neighborTile.gridPosition, CalculatePossibleNeighbors(currentPossibleTileDatas));

            if (currentPossibleTileDatas.Count == 1)
            {
                neighborTile.InitTile(currentPossibleTileDatas[0]);
                counterSetTiles++;
                worldGrid.TileWasSet(neighborTile);
            }
        }

        neighborTile.possibleTiles = new List<SO_TileData>(currentPossibleTileDatas);

        currentPath.Remove(neighborTile);
    }
    
    HashSet<SO_TileData.TileType> CalculatePossibleNeighbors(List<SO_TileData> listTileData)
    {
        HashSet<SO_TileData.TileType> possibleTileTypes = new HashSet<SO_TileData.TileType>();
        foreach (SO_TileData tileData in listTileData)
        {
            foreach (SO_Rules.Rule rule in rules.Rules)
            {
                int mapPosition = (int)CalculatePosition();

                Vector3 currentPos = currentPath[currentPath.Count - 1].transform.position;

                if (rule.inNewBiomeArea)
                {
                    if (currentPos.x > mapPosition && currentPos.y > mapPosition)
                    {
                        possibleTileTypes.Add(rule.title2);
                        possibleTileTypes.Add(rule.title1);
                    }
                }
                else if (rule.title1 == tileData.type)
                {
                    possibleTileTypes.Add(rule.title2);
                }
                else if (rule.title2 == tileData.type)
                {
                    possibleTileTypes.Add(rule.title1);
                }
                
            }
        }

        return possibleTileTypes;
    }

    [SerializeField] private LineRenderer lineRenderer;

    void LineUpdate()
    {
        if (!visualise)
        {
            return;
        }
        
        Vector3[] points = currentPath.Select(tile => tile.gridPosition.ToVector3()).ToArray();
        
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }

    float CalculatePosition()
    {
        return worldGrid.GetMapSize() * biomePosition;
    }
}
