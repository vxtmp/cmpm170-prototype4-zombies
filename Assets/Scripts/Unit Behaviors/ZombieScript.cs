using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieScript : MonoBehaviour
{
    // References to child object (components)
    [SerializeField] private GameObject zombieAcqRangeObj;
    private ZombieAcquisitionRange acquisitionRange;

    Rigidbody2D rb;

    public int health = 5;
    [SerializeField] private int ZOMBIE_ATTACK_POWER = 2;


    // Inspector Constants.
    private bool PHYSICS_BASED = true; // Switch between physics-based move / not.
    private float PHYSICS_BASE_SPEED = 50.0f;
    private float PHYSICS_MAX_SPEED = 20.0f;
    private float NOT_PHYSICS_BASE_SPEED = 50.0f;
    private float SPEED_MULTIPLIER = 1.0f;

    private const float BUMP_COOLDOWN_SECONDS = 2.0f;
    private float bumpTimer = 0.0f;


    void Start()                                            // Start
    {
        rb = this.GetComponent<Rigidbody2D>();
        acquisitionRange = zombieAcqRangeObj.GetComponent<ZombieAcquisitionRange>();
    }


    void Update()                                           // Update
    {
        // if there is nearby target, move towards closest
        if (acquisitionRange.getTargetsInRange().Count > 0)
        {
            moveSelf(directionToNearestHuman());
        }
        // else move towards sound.
        else
        {
            // move to getLowestNeighbor
            Vector2 dest = GridManager.Instance.getDestinationNeighbor(this.transform.position);
            Vector3 direction = dest - (Vector2)this.transform.position;
            direction.Normalize();
            moveSelf(direction);
        }

        if (bumpTimer > 0)
        {
            bumpTimer -= Time.deltaTime;
        }
    }


    private void moveSelf(Vector3 direction)
    {
        if (PHYSICS_BASED)
        {
            rb.AddForce(direction * Time.deltaTime * PHYSICS_BASE_SPEED * SPEED_MULTIPLIER);
            // Clamp velocity to max speed
            if (rb.velocity.magnitude > PHYSICS_MAX_SPEED)
            {
                rb.velocity = rb.velocity.normalized * PHYSICS_MAX_SPEED;
            }
        }
        else
        {
            this.transform.position += direction * Time.deltaTime * NOT_PHYSICS_BASE_SPEED;
        }
    }

    // return normal vector direction towards nearest human
    private Vector3 directionToNearestHuman()
    {
        // find the closest human
        GameObject closestHuman = acquisitionRange.getTargetsInRange()[0];
        float closestDistance = Vector3.Distance(this.transform.position, closestHuman.transform.position);
        foreach (GameObject human in acquisitionRange.getTargetsInRange())
        {
            float distance = Vector3.Distance(this.transform.position, human.transform.position);
            if (distance < closestDistance)
            {
                closestHuman = human;
                closestDistance = distance;
            }
        }
        // move towards the closest human
        Vector3 direction = closestHuman.transform.position - this.transform.position;
        direction.Normalize();

        return direction;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {

        string collTag = collision.gameObject.tag;
        if (bumpTimer > 0)
        {
            Debug.Log("zombie bump on cd, bump timer: " + bumpTimer + "\n");
            return;
        }
        switch (collTag)
        {
            case "Player":
                Debug.Log("Zombie bumped player\n");
                GameManager.Instance.getPlayer().GetComponent<PlayerBehavior>().takeDamage(ZOMBIE_ATTACK_POWER);
                bumpTimer = BUMP_COOLDOWN_SECONDS;
                break;
            case "Human":
                Debug.Log("Zombie bumped human\n");
                bumpTimer = BUMP_COOLDOWN_SECONDS;
                break;
            case "Door":
                Debug.Log("Zombie bumped door\n");
                bumpTimer = BUMP_COOLDOWN_SECONDS;
                break;
            default:
                break;
        }
    }
}
