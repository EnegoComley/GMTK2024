using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapController : MonoBehaviour
{
    public GameObject startingGrid;
    public Vector3 fractalScale;
    public Vector3 fractalTranslation;
    public BoundsInt mapGapBounds;
    public CorrosionGridController startingCorrosionGrid;
    public CorrosionGridController[] LargerCorrosionGrids;
    public CorrosionGridController[] SmallerCorrosionGrids;
    public LinkedDepthListNode startingNode;
    public RectTransform rawImage;
    public Camera screenCamera;

    public List<ConnectedBlocks> connectedBlocks;
    
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
        mapGapBounds = mapGap.cellBounds;
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
        connectedBlocks = new List<ConnectedBlocks>();
        foreach (Block block in GetComponentsInChildren<Block>())
        {
            connectedBlocks.Add(new ConnectedBlocks(block, startingNode.currentFractalTranslation, block.transform.localScale));
        }
        
        startingNode.SetMapGapBounds();
        LinkedDepthListNode generatorNode = startingNode;
        for (int i = 0; i < SmallerCorrosionGrids.Length + 2; i++)
        {
            generatorNode = generatorNode.GetNext();
        }

        LinkedDepthListNode.tail = generatorNode;
        generatorNode = startingNode;
        for (int i = 0; i < LargerCorrosionGrids.Length + 2; i++)
        {
            generatorNode = generatorNode.GetPrev();
        }
        LinkedDepthListNode.head = generatorNode;
        generatorNode.GetPrev().GetPrev().GetPrev();
        PlayerController.currentNode = startingNode;
        
        screenCamera.orthographicSize = screenCamera.orthographicSize * LinkedDepthListNode.head.currentFractalScale.y;
        screenCamera.transform.position = LinkedDepthListNode.head.currentFractalTranslation +
                                          Vector3.Scale(LinkedDepthListNode.head.currentFractalScale,
                                              screenCamera.transform.position);
        rawImage.position = new Vector3(rawImage.position.x * LinkedDepthListNode.tail.currentFractalScale.x + LinkedDepthListNode.tail.currentFractalTranslation.x, rawImage.position.y * LinkedDepthListNode.tail.currentFractalScale.y + LinkedDepthListNode.tail.currentFractalTranslation.y, rawImage.position.z);
        rawImage.sizeDelta = new Vector2(rawImage.sizeDelta.x * LinkedDepthListNode.tail.currentFractalScale.x, rawImage.sizeDelta.y * LinkedDepthListNode.tail.currentFractalScale.y);
        
    }
    
    public GameObject InstantiateSomething(GameObject prefab, Transform parent)
    {
        return Instantiate(prefab, parent);
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
    public Vector3 mapGapMin;
    public Vector3 mapGapMax;
    public static TilemapController myTilemapController;
    public GameObject myGrid;
    CorrosionGridController myCorrosionGrid;
    public static LinkedDepthListNode head;
    public static LinkedDepthListNode tail;
    
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
            next.SetMapGapBounds();
            
            if (depth < myTilemapController.SmallerCorrosionGrids.Length)
            {
                next.myCorrosionGrid = myTilemapController.SmallerCorrosionGrids[depth];
                if (next.myCorrosionGrid != null)
                {
                    next.myCorrosionGrid.gameObject.transform.localScale = next.currentFractalScale;
                    next.myCorrosionGrid.gameObject.transform.localPosition = next.currentFractalTranslation;
                }
            }

            foreach (ConnectedBlocks connectedBlock in myTilemapController.connectedBlocks)
            {
                Block newBlock = myTilemapController.InstantiateSomething(connectedBlock.blocks[0].Item1.gameObject, myTilemapController.transform).GetComponent<Block>();
                newBlock.transform.position = Vector3.Scale(connectedBlock.blocks[0].Item1.transform.position, next.currentFractalScale) + next.currentFractalTranslation;
                newBlock.transform.localScale = Vector3.Scale(next.currentFractalScale, connectedBlock.startScale);                
                newBlock.myController = connectedBlock;
                newBlock.myBlockID = connectedBlock.blocks.Count;
                connectedBlock.blocks.Add(new Tuple<Block, Vector3, Vector3>(newBlock, next.currentFractalTranslation, next.currentFractalScale));
            }

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
            prev.SetMapGapBounds();
            if (-depth < myTilemapController.LargerCorrosionGrids.Length)
            {
                prev.myCorrosionGrid = myTilemapController.LargerCorrosionGrids[-depth];
                if (next.myCorrosionGrid != null)
                {
                    prev.myCorrosionGrid.gameObject.transform.localScale = prev.currentFractalScale;
                    prev.myCorrosionGrid.gameObject.transform.localPosition = prev.currentFractalTranslation;
                }
            }
            
            foreach (ConnectedBlocks connectedBlock in myTilemapController.connectedBlocks)
            {
                Block newBlock = myTilemapController.InstantiateSomething(connectedBlock.blocks[0].Item1.gameObject, myTilemapController.transform).GetComponent<Block>();
                newBlock.transform.position = Vector3.Scale(connectedBlock.blocks[0].Item1.transform.position, prev.currentFractalScale) + prev.currentFractalTranslation;
                newBlock.transform.localScale = Vector3.Scale(prev.currentFractalScale, connectedBlock.startScale);
                newBlock.myController = connectedBlock;
                newBlock.myBlockID = connectedBlock.blocks.Count;
                connectedBlock.blocks.Add(new Tuple<Block, Vector3, Vector3>(newBlock, prev.currentFractalTranslation, prev.currentFractalScale));
            }
        }
        return prev;
    }
    
    

    public void SetMapGapBounds()
    {
        mapGapMin = currentFractalTranslation + Vector3.Scale(myTilemapController.mapGapBounds.min, currentFractalScale);
        mapGapMax = currentFractalTranslation + Vector3.Scale(myTilemapController.mapGapBounds.max, currentFractalScale);
    }
}

public class ConnectedBlocks
{
    
    public List<Tuple<Block, Vector3, Vector3>> blocks; //Block, position, scale
    public Vector3 startScale;
    public Vector2 targetVelocity;
    public int velocityVersion;
    
    public ConnectedBlocks(Block firstBlock, Vector3 fistPosition, Vector3 startingScale)
    {
        blocks = new List<Tuple<Block, Vector3, Vector3>>();
        blocks.Add(new Tuple<Block, Vector3, Vector3>(firstBlock, fistPosition, Vector3.one));
        startScale = startingScale;
        firstBlock.myController = this;
        firstBlock.myBlockID = 0;
    }

    public void SyncBlocks(int blockId)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            if (i == blockId)
            {
                continue;
            }

            /*blocks[i].Item1.transform.position = new Vector3(
                ((blocks[blockId].Item1.transform.position.x - blocks[blockId].Item2.x) / blocks[i].Item3.x) *
                blocks[blockId].Item3.x + blocks[blockId].Item2.x,
                ((blocks[blockId].Item1.transform.position.y - blocks[blockId].Item2.y) / blocks[i].Item3.y) *
                blocks[blockId].Item3.y + blocks[blockId].Item2.y, 0);*/
            //blocks[i].Item1.myRigidbody2D.linearVelocity = blocks[blockId].Item1.myRigidbody2D.linearVelocity * blocks[i].Item3;
            blocks[i].Item1.CheckTargetVelocity();
        }
    }
    
    public bool CheckNewVelocity(Vector3 newVelocity)
    {
        foreach (Tuple<Block, Vector3, Vector3> block in blocks)
        {
            if (!block.Item1.CheckNewVelocity(newVelocity))
            {
                return false;
            }
        }

        return true;
    }
}
