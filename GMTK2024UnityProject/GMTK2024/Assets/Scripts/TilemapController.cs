using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapController : MonoBehaviour
{
    public GameObject startingGrid;
    public Vector3 fractalScale;
    public Vector3 fractalTranslation;
    public LinkedDepthListNode startingNode;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Tilemap mapGap = null;
        Tilemap[] startingTilemaps = startingGrid.GetComponentsInChildren<Tilemap>();
        foreach (Tilemap tilemap in startingTilemaps)
        {
            if (tilemap.gameObject.CompareTag("MapGap"))
            {
                mapGap = tilemap;
                break;
            }
        }
        if (mapGap == null)
        {
            Debug.LogError("MapGap not found");
            return;
        }
        
        // Iterate through all the tiles in the tilemap and store the maximum and minimum x and y values that have a tile
        mapGap.CompressBounds();
        BoundsInt mapGapBounds = mapGap.cellBounds;
        BoundsInt[] allTilemapBounds = new BoundsInt[startingTilemaps.Length];
        for (int i = 0; i < startingTilemaps.Length; i++)
        {
            startingTilemaps[i].CompressBounds();
            allTilemapBounds[i] = startingTilemaps[i].cellBounds;
        }
        Vector3Int mapGapSize = mapGapBounds.size;
        Vector3Int allTilesMapMin = mapGapBounds.min;
        Vector3Int allTilesMapMax = mapGapBounds.max;
        // Iterate through the tiles and store the total maximum and minimum x and y values
        foreach (BoundsInt bounds in allTilemapBounds)
        {
            if (bounds.min.x < allTilesMapMin.x)
            {
                allTilesMapMin.x = bounds.min.x;
            }
            if (bounds.min.y < allTilesMapMin.y)
            {
                allTilesMapMin.y = bounds.min.y;
            }
            if (bounds.max.x > allTilesMapMax.x)
            {
                allTilesMapMax.x = bounds.max.x;
            }
            if (bounds.max.y > allTilesMapMax.y)
            {
                allTilesMapMax.y = bounds.max.y;
            }
        }
        Vector3Int allTilesMapSize = allTilesMapMax - allTilesMapMin;
        fractalScale = new Vector3(mapGapSize.x / (float)allTilesMapSize.x, mapGapSize.y / (float)allTilesMapSize.y, 1);
        fractalTranslation = mapGap.localBounds.center - new Vector3((allTilesMapMin.x + allTilesMapSize.x / 2) * fractalScale.x, (allTilesMapMin.y + allTilesMapSize.y / 2) * fractalScale.y, 0);
        mapGap.ClearAllTiles();
        startingNode = new LinkedDepthListNode(0);
        startingNode.currentFractalScale = new Vector3(1, 1, 1);
        startingNode.currentFractalTranslation = new Vector3();
        startingNode.myGrid = startingGrid;
        LinkedDepthListNode.myTilemapController = this;
        startingNode.GetNext().GetNext().GetNext().GetNext();
        startingNode.GetPrev().GetPrev();
        PlayerController.currentNode = startingNode;
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public GameObject CreateNewLayer(Vector3 fractalScale, Vector3 fractalTranslation)
    {
        GameObject newLayer = Instantiate(startingGrid, gameObject.transform);
        newLayer.transform.localScale = fractalScale;
        newLayer.transform.localPosition = fractalTranslation;
        return newLayer;
    }
}

public class LinkedDepthListNode
{   
    public LinkedDepthListNode next;
    public LinkedDepthListNode prev;
    public int depth;
    public Vector3 currentFractalScale;
    public Vector3 currentFractalTranslation;
    public static TilemapController myTilemapController;
    public GameObject myGrid;
    
    public LinkedDepthListNode(int depth)
    {
        this.depth = depth;
    }

    public LinkedDepthListNode GetNext()
    {
        if (next == null)
        {
            next = new LinkedDepthListNode(depth + 1);
            next.prev = this;
            next.currentFractalScale = new Vector3(currentFractalScale.x * myTilemapController.fractalScale.x, currentFractalScale.y * myTilemapController.fractalScale.y, 1);
            next.currentFractalTranslation = new Vector3(currentFractalTranslation.x + myTilemapController.fractalTranslation.x * currentFractalScale.x, currentFractalTranslation.y + myTilemapController.fractalTranslation.y * currentFractalScale.x, 0);
            next.myGrid = myTilemapController.CreateNewLayer(next.currentFractalScale, next.currentFractalTranslation);
        }
        return next;
    }
    
    public LinkedDepthListNode GetPrev()
    {
        if (prev == null)
        {
            prev = new LinkedDepthListNode(depth - 1);
            prev.next = this;
            prev.currentFractalScale = new Vector3(currentFractalScale.x / myTilemapController.fractalScale.x, currentFractalScale.y / myTilemapController.fractalScale.y, 1);
            prev.currentFractalTranslation = new Vector3(currentFractalTranslation.x - (myTilemapController.fractalTranslation.x) * prev.currentFractalScale.x, currentFractalTranslation.y - (myTilemapController.fractalTranslation.y) * prev.currentFractalScale.y, 0);
            prev.myGrid = myTilemapController.CreateNewLayer(prev.currentFractalScale, prev.currentFractalTranslation);
        }
        return prev;
    }
    
    
}
