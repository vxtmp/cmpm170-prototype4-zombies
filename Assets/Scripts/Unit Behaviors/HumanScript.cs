using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HumanScript : MonoBehaviour
{
    private bool moving = true;
    private Vector2 dest;
    public float speed = 50.0f;
    public float max_speed = 20.0f;
    // human inventory
    public List<Item> items = new List<Item>();
    [SerializeField] private GameObject ItemPrefab;
    [SerializeField] private GameObject humanBulletPrefab;
    [SerializeField] private GameObject acquisitionRange;
    public int health = 10;
    public int healthDelta = 0;
    private bool healthChanged = false;

    Rigidbody2D rb;

    private float attackTimer = 0.0f;
    private float ATTACK_COOLDOWN = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        Debug.Log(this.transform.position);
        StartCoroutine(idleMove());
    }

    // Update is called once per frame
    void Update()
    {
        if (this.moving)
        {
            move();
            this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        }

        if (healthChanged)
        {
            this.health += this.healthDelta;
            this.healthDelta = 0;
            this.healthChanged = false;
            Debug.Log("human health changed: " + this.health);
            if (this.health <= 0)
            {
                DropItems();
            }
        }

        if (getClosestTarget() && attackTimer <= 0.0f)
        {
            Debug.Log("attempting to shoot");
            shoot();
            attackTimer = ATTACK_COOLDOWN;
        }
    }

    private void move()
    {
        Vector3 direction = this.dest - (Vector2)this.transform.position;
        direction.Normalize();

        // norman's movey code
        rb.AddForce(direction * Time.deltaTime * this.speed);
        // Clamp velocity to max speed
        if (rb.velocity.magnitude > this.max_speed)
        {
            rb.velocity = rb.velocity.normalized * this.max_speed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // zombie attack collision or bullet collisiom
        if (collision.gameObject.CompareTag("Attack"))
        {
            health -= collision.gameObject.GetComponent<BulletScript>().damage;  
        }

        if (collision.gameObject.CompareTag("Zombie"))
        {
            health -= collision.gameObject.GetComponent<ZombieScript>().ZOMBIE_ATTACK_POWER;
        }

        if (health <= 0)
        {
            DropItems();
        }
    }

    public void DropItems()
    {
        foreach (Item item in items)
        {
            Debug.Log(item);
            Vector3 randomPos = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
            GameObject itemInstance = Instantiate(ItemPrefab, transform.position + randomPos, new Quaternion(0, 0, 0, 0));
            itemInstance.GetComponent<ItemInteractable>().SetUp(item);
        }
        Destroy(gameObject);
    }

    // schmoovin
    private IEnumerator idleMove()
    {
        Vector2[] posArray;
        for (; ; )
        {
            if (Random.Range(0, 9) % 2 == 0)
            {
                posArray = GridManager.Instance.getViableNeighbors(this.transform.position);
                this.dest = posArray[Random.Range(0, posArray.Length)];
                //Debug.Log(this.transform.position);
               
                //if (this.moving == false)
                //{
                //    posArray = GridManager.Instance.getNeighbors(this.transform.position);
                //    //Vector2[] arr2 = GridManager.Instance.getDiagonalNeighbors(this.transform.position);
                //    //posArray = GridManager.Instance.combineArray(arr1, arr2);
                //    this.dest = posArray[Random.Range(0, posArray.Length)];
                //    Debug.Log(this.dest);
                //    this.moving = true;
                //}
            }
            else
            {
                this.dest = this.transform.position;
            }
            
            yield return new WaitForSeconds(Random.Range(0.5f, 8.0f));
        }
        
    }

    public void takeDamage(int damageValue)
    {
        //Debug.Log("human takedmg zombie: " + damageValue);
        this.healthDelta -= damageValue;
        this.healthChanged = true;
    }

    public void shoot()
    {
        // spawn a prefab of the bullet with slight offset.
        Debug.Log("shoot function called in human");
        Quaternion quaternion = Quaternion.LookRotation(getClosestTarget().transform.position - this.transform.position, Vector3.up);
        GameObject bullet = Instantiate(humanBulletPrefab, this.transform.position, quaternion);
    }

    private GameObject getClosestTarget()
    {
        GameObject closestTarget = null;
        float closestDistance = Mathf.Infinity;
        foreach (GameObject target in acquisitionRange.GetComponent<HumanAcquisitionRange>().getTargetsInRange())
        {
            if (target == null) break;
            float distance = Vector2.Distance(this.transform.position, target.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }
        return closestTarget;
    }
}
