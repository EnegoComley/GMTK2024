using UnityEngine;

public class Block : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Rigidbody2D myRigidbody2D;
    public ConnectedBlocks myController;
    public int myBlockID;
    public int myVelocityVersion;

    void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        CheckTargetVelocity();
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Vector2 collisionPoint = collision.contacts[0].point;
            Vector2 movementDirection = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y) -
                                        collisionPoint;
            movementDirection = movementDirection.normalized;
            if (Mathf.Abs(movementDirection.x) < 0.5f)
            {
                movementDirection.x = 0;
            }
            else
            {
                movementDirection.y = 0;
            }

            movementDirection = movementDirection.normalized;
            SetLinearVelocitySafely(movementDirection * 5 * myController.blocks[myBlockID].Item3);
        }

        CheckTargetVelocity();

    }
    
    public void SetLinearVelocitySafely(Vector2 newVelocity)
    {

        if (myController.CheckNewVelocity(newVelocity))
        {
            myRigidbody2D.linearVelocity = newVelocity;
        }
       
    }

    public bool CheckNewVelocity(Vector2 newVelocity)
    {
        RaycastHit2D[] hits = new RaycastHit2D[100];
        int numHits = myRigidbody2D.Cast(newVelocity, hits, newVelocity.magnitude * Time.fixedDeltaTime);
        for (int i = 0; i < numHits; i++)
        {
            if (hits[i].collider.gameObject.CompareTag("Wall") || hits[i].collider.gameObject.CompareTag("Corrosion"))
            {
                return false;
            }
        }

        return true;
    }
    
    public void CheckTargetVelocity()
    {
        if (myController.velocityVersion < myVelocityVersion)
        {
            Debug.LogError("Velocity version is too low!");
        }
        if (myController.velocityVersion == myVelocityVersion && myRigidbody2D.linearVelocity != myTargetVelocity())
        {
            myController.velocityVersion++;
            myVelocityVersion = myController.velocityVersion;
            myController.targetVelocity = new Vector2(myRigidbody2D.linearVelocity.x/ myController.blocks[myBlockID].Item3.x, myRigidbody2D.linearVelocity.y/ myController.blocks[myBlockID].Item3.x);
            myController.SyncBlocks(myBlockID);
        }
        else if (myController.velocityVersion > myVelocityVersion)
        {
            myRigidbody2D.linearVelocity = myTargetVelocity();
            myVelocityVersion = myController.velocityVersion;
        }
    }

    public Vector2 myTargetVelocity()
    {
        return Vector3.Scale(myController.targetVelocity, myController.blocks[myBlockID].Item3);
    }
    
    

}
