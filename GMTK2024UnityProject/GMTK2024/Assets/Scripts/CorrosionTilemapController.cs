using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CorrosionTilemapController : MonoBehaviour
{
    public CorrosionGridController myCorrosionGridController;
    Tilemap myTilemap;
    Queue<Tuple<Vector3Int, TileBase>> tilesToDestroy = new Queue<Tuple<Vector3Int, TileBase>>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myTilemap = GetComponent<Tilemap>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet.electricityType < 0)
            {
                return;
            }
            tilesToDestroy.Enqueue(new Tuple<Vector3Int, TileBase>(myTilemap.WorldToCell(collision.contacts[0].point + collision.relativeVelocity * (1.0f/20.0f)), bullet.corrosionTiles[bullet.electricityType]));
            DestroyCorrosion();
        }
    }
    
    void DestroyCorrosion()
    {   
        Tuple<Vector3Int, TileBase> tileToDestroy = tilesToDestroy.Dequeue();
        Vector3Int position = tileToDestroy.Item1;
        if (myTilemap.GetTile(position) == null)
        {
            return;
        }
        if (myTilemap.GetTile(position) != tileToDestroy.Item2)
        {
            return;
        }
        myTilemap.SetTile(position, null);
        
        tilesToDestroy.Enqueue(new Tuple<Vector3Int, TileBase>(position + Vector3Int.left, tileToDestroy.Item2));
        tilesToDestroy.Enqueue(new Tuple<Vector3Int, TileBase>(position + Vector3Int.right, tileToDestroy.Item2));
        tilesToDestroy.Enqueue(new Tuple<Vector3Int, TileBase>(position + Vector3Int.up, tileToDestroy.Item2));
        tilesToDestroy.Enqueue(new Tuple<Vector3Int, TileBase>(position + Vector3Int.down, tileToDestroy.Item2));
        
        Invoke("DestroyCorrosion", 0.5f);
        Invoke("DestroyCorrosion", 0.5f);
        Invoke("DestroyCorrosion", 0.5f);
        Invoke("DestroyCorrosion", 0.5f);
        
        
    }
}
