using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Vector3 = System.Numerics.Vector3;

public class Bullet : MonoBehaviour
{
    Rigidbody2D myRigidbody2D;
    public LinkedDepthListNode currentNode;
    public int electricityType = -1;
    public Sprite[] bulletSprites;
    public String[] electrifierTags;
    public TileBase[] corrosionTiles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (transform.position.x >= currentNode.mapGapMin.x && transform.position.x < currentNode.mapGapMax.x && transform.position.y >= currentNode.mapGapMin.y && transform.position.y < currentNode.mapGapMax.y)
        {
            LinkedDepthListNode lastNode = currentNode;
            currentNode = currentNode.GetNext();
            myRigidbody2D.linearVelocity *= new Vector2(currentNode.currentFractalScale.x/lastNode.currentFractalScale.x, currentNode.currentFractalScale.y/lastNode.currentFractalScale.y);
        }
        if (transform.position.x < currentNode.GetPrev().mapGapMin.x || transform.position.x >= currentNode.GetPrev().mapGapMax.x || transform.position.y < currentNode.GetPrev().mapGapMin.y || transform.position.y >= currentNode.GetPrev().mapGapMax.y)
        {
            LinkedDepthListNode lastNode = currentNode;
            currentNode = currentNode.GetPrev();
            myRigidbody2D.linearVelocity *= new Vector2(currentNode.currentFractalScale.x/lastNode.currentFractalScale.x, currentNode.currentFractalScale.y/lastNode.currentFractalScale.y);
        }
        
        if (new Vector2(myRigidbody2D.linearVelocity.x/ currentNode.currentFractalScale.x, myRigidbody2D.linearVelocity.y/ currentNode.currentFractalScale.y).magnitude < 5)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Bullet"))
        {
            Destroy(gameObject);
        } else if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Block") || collision.gameObject.CompareTag("Corrosion"))
        {
            myRigidbody2D.linearVelocity = Vector2.Reflect(-collision.relativeVelocity, collision.contacts[0].normal).normalized * 10 * currentNode.currentFractalScale;
        }
        
    }
    
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Fire") || collider.gameObject.CompareTag(electrifierTags[0]) || collider.gameObject.CompareTag(electrifierTags[1]))
        {
            for (int i = 0; i < electrifierTags.Length; i++)
            {
                if (collider.gameObject.CompareTag(electrifierTags[i]))
                {
                    electricityType = i;
                    break;
                }
            }
            
            gameObject.GetComponent<SpriteRenderer>().sprite = bulletSprites[electricityType];
        }
    }
}
