using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieScript : MonoBehaviour
{
    // References to child object (components)
    [SerializeField] private GameObject zombieAcqRangeObj;
    private ZombieAcquisitionRange acquisitionRange;
    [SerializeField] private GameObject zombieAtkRangeObj;
    private ZombieAttackRange attackRange;

    Rigidbody2D rb;

    public float health = 1;
    public int ZOMBIE_ATTACK_POWER = 2;


    // Zombie Stats
    private bool PHYSICS_BASED = true; // Switch between physics-based move / not.
    private float PHYSICS_BASE_SPEED = 50.0f;
    private float PHYSICS_MAX_SPEED = 20.0f;
    private float SPEED_MULTIPLIER = 15.0f;

    private const float BUMP_COOLDOWN_SECONDS = 2.0f;
    private float bumpTimer = 0.0f;

    // deprecated.
    private float NOT_PHYSICS_BASE_SPEED = 50.0f;

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
            moveSelf(directionToNearestTarget());
        }
        // else move towards sound.
        else
        {
            // move to getLowestNeighbor
            Vector2 dest = GridManager.Instance.getDestinationNeighbor(this.transform.position);
            if (dest != new Vector2(-1, -1))
            {
                Vector3 direction = dest - (Vector2)this.transform.position;
                direction.Normalize();
                moveSelf(direction);
            }
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
    private Vector3 directionToNearestTarget()
    {
        GameObject closestHuman = null;
        float closestHumanDistance = float.MaxValue;
        // find the closest target
        GameObject closestTarget = acquisitionRange.getTargetsInRange()[0];
        float closestDistance = Vector3.Distance(this.transform.position, closestTarget.transform.position);
        // if (closestTarget.tag == "Human"){
        //     closestHuman = closestTarget;
        //     closestHumanDistance = closestDistance;
        // }
        foreach (GameObject target in acquisitionRange.getTargetsInRange())
        {
            // if (target.tag == "Player"){
            //     Vector3 pd = target.transform.position - this.transform.position;
            //     pd.Normalize();
            //     return pd;
            // }
            float distance = Vector3.Distance(this.transform.position, target.transform.position);
            if (closestHuman == null){
                if (distance < closestDistance)
                {
                    closestTarget = target;
                    closestDistance = distance;
                }
            } else {
                if (target.tag == "Human" && distance < closestHumanDistance)
                {
                    closestHuman = target;
                    closestHumanDistance = distance;
                }
            }
        }
        // if (closestHuman){
        //     closestTarget = closestHuman;
        //     closestDistance = closestHumanDistance;
        // }
        // move towards the closest human
        Vector3 direction = closestTarget.transform.position - this.transform.position;
        direction.Normalize();

        return direction;
    }
    private void OnCollisionStay2D(Collision2D collision)
    {

        string collTag = collision.gameObject.tag;
        //Debug.Log(collTag);
        if (bumpTimer > 0)
        {
            return;
        }
        GameObject collObj = collision.gameObject;
        switch (collTag)
        {
            case "Player":
                Debug.Log("Zombie bumped player\n");
                collObj.GetComponent<PlayerBehavior>()!.changeHealth(-ZOMBIE_ATTACK_POWER);
                bumpTimer = BUMP_COOLDOWN_SECONDS;
                break;
            case "Human":
                Debug.Log("Zombie bumped human\n");
                collObj.GetComponent<HumanScript>()!.takeDamage(ZOMBIE_ATTACK_POWER);
                bumpTimer = BUMP_COOLDOWN_SECONDS;
                break;
            case "Door":
                Debug.Log("Zombie bumped door\n");
                collObj.GetComponent<DoorBehavior>()!.takeDamage(ZOMBIE_ATTACK_POWER);
                bumpTimer = BUMP_COOLDOWN_SECONDS;
                break;
            default:
                break;
        }
    }

    public void takeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
