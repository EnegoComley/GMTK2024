using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Collider2D myCollider2D;
    public static LinkedDepthListNode currentNode;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myCollider2D = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            Vector3 direction = new Vector3(Vector3.left.x * currentNode.currentFractalScale.x, Vector3.left.y * currentNode.currentFractalScale.y, 0);
            if (!WillCollide(direction))
            {
                transform.position += direction;
            }
        } else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            Vector3 direction = new Vector3(Vector3.right.x * currentNode.currentFractalScale.x, Vector3.right.y * currentNode.currentFractalScale.y, 0);
            if (!WillCollide(direction))
            {
                transform.position += direction;
            }
        } else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            Vector3 direction = new Vector3(Vector3.up.x * currentNode.currentFractalScale.x, Vector3.up.y * currentNode.currentFractalScale.y, 0);
            if (!WillCollide(direction))
            {
                transform.position += direction;
            }
        } else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            Vector3 direction = new Vector3(Vector3.down.x * currentNode.currentFractalScale.x, Vector3.down.y * currentNode.currentFractalScale.y, 0);
            if (!WillCollide(direction))
            {
                transform.position += direction;
            }
        }
    }
    
    bool WillCollide(Vector3 direction)
    {
        RaycastHit2D[] hits = new RaycastHit2D[200];
        int collissions = myCollider2D.Cast(direction, hits, 1.0f);
        if (collissions > 0)
        {
            for (int i = 0; i < collissions; i++)
            {
                if (hits[i].collider.gameObject.layer == 6)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
