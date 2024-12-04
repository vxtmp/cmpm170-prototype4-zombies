using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    private GameObject objectToMove;
    public Item curItem;
    [SerializeField] private GameObject ItemPrefab;

    public int health;
    [SerializeField] private int maxHealth;
    public int hunger;
    [SerializeField] private int maxHunger;

    private bool throwing = false;
    private bool moving = false;
    [SerializeField] private float maxThrowRange = 10.0f;

    public const float PLAYER_SPEED = 5.0f;

    public float GLOBAL_AGGRO_INTERVAL = 5.0f;
    private float global_aggro_timer = 0.0f;

    public int healthDelta = 0;
    public bool healthChanged = false;
    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        hunger = maxHunger;
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
        /*if (Input.GetKey(KeyCode.Space))
        {
            if(objectToMove)
            {
                // move around object
            }
        }*/

        // use/throw item
        if (Input.GetKeyDown(KeyCode.E))
        {
            if(curItem)
            {
                if (curItem.consumable)
                {
                    health += curItem.hungerSatisfaction;
                    if (health > maxHealth)
                    {
                        health = maxHealth;
                    }
                    curItem.RemoveFromInventory();
                }
                else if (curItem.weapon)
                {
                    Shoot();
                }
                else
                {
                    throwing = true;
                }
            }
        }

        if(throwing && Input.GetMouseButtonDown(0))
        {
            Throw();
            throwing = false;
        }

        if (healthChanged)
        {
            health = health + healthDelta;
            healthDelta = 0;
            healthChanged = false;
            Debug.Log("healthChanged: " + health);
        }
    }

    public void Shoot()
    {
        if (curItem.health > 0)
        {
            GameObject bullet = Instantiate(curItem.bullet, transform.position, Quaternion.identity);
            BulletScript script = bullet.GetComponent<BulletScript>();
            bullet.GetComponent<Rigidbody2D>().velocity = transform.up * script.bulletSpeed;
            script.damage = curItem.damage;
            curItem.health--;
        }
        
        if(curItem.health <= 0)
        {
            curItem.RemoveFromInventory();
        }
    }
    private void Throw()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(mouseRay, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            // Otherwise, project a point forward in the direction of the ray
            targetPoint = mouseRay.GetPoint(maxThrowRange);
        }

        // Calculate direction and clamp distance to maxThrowRange
        Vector3 throwDirection = targetPoint - transform.position;
        if (throwDirection.magnitude > maxThrowRange)
        {
            throwDirection = throwDirection.normalized * maxThrowRange;
        }

        // Instantiate the object and apply force
        GameObject thrownObject = Instantiate(ItemPrefab, transform.position, Quaternion.identity);
        thrownObject.GetComponent<ItemInteractable>().SetUp(curItem);
        Rigidbody rb = thrownObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(throwDirection.normalized * 0.5f, ForceMode.Impulse);
        }

        curItem.RemoveFromInventory();
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
        if (collision.gameObject.CompareTag("Item"))
        {
            Debug.Log("pick up item");
            if (Inventory.instance.Add(collision.gameObject.GetComponent<ItemInteractable>().item))
            {
                Destroy(collision.gameObject);
            }
        }//if (collision.gameObject.CompareTag("Obstacle")) {
        //    objectToMove = collision.gameObject;
        //}
    }

        

    private void OnCollisionExit2D(Collision2D collision)
    {
        //if (collision.gameObject.CompareTag("Obstacle"))
        //{
        //    objectToMove = null;
        //}
    }


    public void takeDamage(int damageValue)
    {
        Debug.Log("player takeDmg:" + damageValue);
        healthDelta -= damageValue;
        healthChanged = true;
    }
}
