using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Collider2D myCollider2D;
    Rigidbody2D myRigidbody2D;
    public static LinkedDepthListNode currentNode;
    public GameObject gun;
    public GameObject bulletPrefab;
    float targetCameraSize;
    private Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myCollider2D = GetComponent<Collider2D>();
        myRigidbody2D = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        targetCameraSize = mainCamera.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 movementDirection = Vector2.zero;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            movementDirection += Vector2.left;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            movementDirection += Vector2.right;
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            movementDirection += Vector2.up;
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            movementDirection += Vector2.down;
        }
        movementDirection = movementDirection.normalized;
        movementDirection = new Vector3(movementDirection.x * currentNode.currentFractalScale.x, movementDirection.y * currentNode.currentFractalScale.y);
        myRigidbody2D.linearVelocity = movementDirection * 5;
        
        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        float gunAngle = Vector3.SignedAngle(Vector3.down, mousePos - gun.transform.position, Vector3.forward);
        gun.transform.rotation = Quaternion.Euler(0, 0, gunAngle);
        
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 bulletDirection =  Vector2.Scale(Quaternion.Euler(0, 0, gunAngle) * Vector2.down, currentNode.currentFractalScale); 
            GameObject bullet = Instantiate(bulletPrefab, transform.position + currentNode.currentFractalScale * 0.5f + bulletDirection, new Quaternion());
            bullet.GetComponent<Bullet>().currentNode = currentNode;
            bullet.transform.localScale = currentNode.currentFractalScale * 0.2f;
            bullet.GetComponent<Rigidbody2D>().linearVelocity = (bulletDirection * 10);
            
        }
        
        
    }

    private void FixedUpdate()
    {   
        Vector3 playerCenter = transform.position + new Vector3(0.5f * currentNode.currentFractalScale.x, 0.5f * currentNode.currentFractalScale.y, 0);
        if (playerCenter.x >= currentNode.mapGapMin.x && playerCenter.x < currentNode.mapGapMax.x && playerCenter.y >= currentNode.mapGapMin.y && playerCenter.y < currentNode.mapGapMax.y)
        {
            LinkedDepthListNode lastNode = currentNode;
            currentNode = currentNode.GetNext();
            ScaleToNewLayer(lastNode, Vector3.Scale(myRigidbody2D.linearVelocity.normalized, lastNode.currentFractalScale));
        }
        if (playerCenter.x < currentNode.GetPrev().mapGapMin.x || playerCenter.x >= currentNode.GetPrev().mapGapMax.x || playerCenter.y < currentNode.GetPrev().mapGapMin.y || playerCenter.y >= currentNode.GetPrev().mapGapMax.y)
        {
            LinkedDepthListNode lastNode = currentNode;
            currentNode = currentNode.GetPrev();
            ScaleToNewLayer(lastNode, Vector3.Scale(myRigidbody2D.linearVelocity.normalized, lastNode.currentFractalScale));
        }
        if (Mathf.Abs(mainCamera.orthographicSize - targetCameraSize) > currentNode.currentFractalScale.y * 0.4)
        {
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetCameraSize, 0.02f);
        }
        else
        {
            mainCamera.orthographicSize = targetCameraSize;
        }
        if ((mainCamera.transform.position - new Vector3(playerCenter.x, playerCenter.y, -10)).magnitude > 0.05f * currentNode.currentFractalScale.magnitude)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(playerCenter.x, playerCenter.y, -10), 0.02f);
        } else
        {
            mainCamera.transform.position = new Vector3(playerCenter.x, playerCenter.y, -10);
        }
        
    }

    bool WillCollide(Vector3 direction)
    {
        Vector3 newPosition = transform.position + direction + new Vector3(0.5f * currentNode.currentFractalScale.x,
            0.5f * currentNode.currentFractalScale.y, 0);
        if (newPosition.x >= currentNode.mapGapMin.x && newPosition.x < currentNode.mapGapMax.x && newPosition.y >= currentNode.mapGapMin.y && newPosition.y < currentNode.mapGapMax.y)
        {
            LinkedDepthListNode lastNode = currentNode;
            currentNode = currentNode.GetNext();
            ScaleToNewLayer(lastNode, direction);
            return true;
        }
        if (newPosition.x < currentNode.GetPrev().mapGapMin.x || newPosition.x >= currentNode.GetPrev().mapGapMax.x || newPosition.y < currentNode.GetPrev().mapGapMin.y || newPosition.y >= currentNode.GetPrev().mapGapMax.y)
        {
            LinkedDepthListNode lastNode = currentNode;
            currentNode = currentNode.GetPrev();
            ScaleToNewLayer(lastNode, direction);
            return true;
        }
        RaycastHit2D[] hits = new RaycastHit2D[200];
        int collissions = myCollider2D.Cast(direction, hits, direction.magnitude);
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

    void ScaleToNewLayer(LinkedDepthListNode lastNode, Vector3 direction)
    {
        //Vector3 playerCenter = transform.position + new Vector3(0.5f * lastNode.currentFractalScale.x, 0.5f * lastNode.currentFractalScale.y, 0);
        //Vector3 newPlayerCenter = playerCenter + 0.5f * direction + 0.5f * new Vector3(currentNode.currentFractalScale.x * (direction.x/ lastNode.currentFractalScale.x), currentNode.currentFractalScale.y * (direction.y/ lastNode.currentFractalScale.y), 0);
        //transform.position = newPlayerCenter - new Vector3(0.5f * currentNode.currentFractalScale.x, 0.5f * currentNode.currentFractalScale.y, 0);
        gameObject.transform.localScale = currentNode.currentFractalScale;

        Vector3 gridCenterPosition = currentNode.myGrid.GetComponent<Grid>().CellToWorld(new Vector3Int(0, 0, 0));
        Vector3 gridToPlayer = transform.position - gridCenterPosition;
        Vector3 currentOffset = new Vector3(gridToPlayer.x % currentNode.currentFractalScale.x, gridToPlayer.y % currentNode.currentFractalScale.y, 0);
        transform.position -= currentOffset;
        transform.position += new Vector3(currentNode.currentFractalScale.x * Mathf.Round(currentOffset.x/currentNode.currentFractalScale.x), currentNode.currentFractalScale.y * Mathf.Round(currentOffset.y/currentNode.currentFractalScale.y), 0);
        targetCameraSize = (currentNode.currentFractalScale.y/lastNode.currentFractalScale.y) * targetCameraSize;
    }

}
