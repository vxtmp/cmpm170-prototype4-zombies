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
    [SerializeField] private int damage = 2;


    // Inspector Constants.
    private bool PHYSICS_BASED = true; // Switch between physics-based move / not.
    private float PHYSICS_BASE_SPEED = 100.0f;
    private float PHYSICS_MAX_SPEED = 100.0f;
    private float NOT_PHYSICS_BASE_SPEED = 100.0f;
    private float SPEED_MULTIPLIER = 50.0f;


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
            // WIP . Waiting on GridManager.cs
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
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Human") || collision.gameObject.CompareTag("Player"))
        {
            // do damage to collided human/player
        }
    }
}
