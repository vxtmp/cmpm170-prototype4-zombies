using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    private GameObject objectToMove;

    public const float PLAYER_SPEED = 5.0f;

    public float GLOBAL_AGGRO_INTERVAL = 5.0f;
    private float global_aggro_timer = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (global_aggro_timer < GLOBAL_AGGRO_INTERVAL)
        {
            global_aggro_timer += Time.deltaTime;
        }
        else
        {
            global_aggro_timer = 0.0f;
            GridManager.Instance.recalcPathing(this.transform.position, 50.0f);
        }
        // move
        parseWASD();

        // move obstacle
        if (Input.GetKey(KeyCode.Space))
        {
            if(objectToMove)
            {
                // move around object
            }
        }
    }

    public void parseWASD()
    {
        float upDirection = 0.0f;
        float rightDirection = 0.0f;

        if (Input.GetKey(KeyCode.W)) upDirection++;
        if (Input.GetKey(KeyCode.S)) upDirection--;
        if (Input.GetKey(KeyCode.A)) rightDirection--;
        if (Input.GetKey(KeyCode.D)) rightDirection++;

        Vector3 direction = new Vector3(rightDirection, upDirection, 0.0f);
        direction.Normalize();
        moveSelf(direction);
        updateRotation(direction);
    }
    private void moveSelf(Vector3 direction)
    {
        this.transform.position += direction * Time.deltaTime * PLAYER_SPEED;
    }
    
    private void updateRotation(Vector3 direction)
    {
        if (direction.x == 0 && direction.y == 0)
        {
            // idle
        }
        else if (direction.x < 0)
        {
            if      (direction.y < 0)   this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 135.0f);
            else if (direction.y == 0)  this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
            else if (direction.y > 0)   this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 45.0f);
        }
        else if (direction.x == 0)
        {
            if      (direction.y < 0)   this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
            else if (direction.y > 0)   this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        }
        else if (direction.x > 0)
        {
            if      (direction.y < 0)   this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -135.0f);
            else if (direction.y == 0)  this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
            else if (direction.y > 0)   this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -45.0f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Item")) {
            //if (items.Count < 6)
            {
                //Inventory.instance.Add(collision.gameObject);
                // remove from map
            }
        }

        if (collision.gameObject.CompareTag("Obstacle")) {
            objectToMove = collision.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            objectToMove = null;
        }
    }

    private void Remove(GameObject item) 
    {
     //   items.Remove(item);
        // use or throw
    }

}
