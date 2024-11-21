using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    private const bool PHYSICS_BASED = false;               // Switch between physics-based move / not.

    private List<GameObject> nearbyTargets;                 
    private readonly string[] TARGET_TAGS = {   "Human",    // Targets that zombies will move towards and attack, when 
                                            "Player",       // within a minimum range as defined by trigger volume radius.
                                            "Gate" };

    void Start()                                            // Start
    {
        nearbyTargets = new List<GameObject>();
    }


    void Update()                                           // Update
    {
        // if there is nearby target, move towards closest
        if (nearbyTargets.Count > 0)
        {
            moveSelf(directionToNearestHuman());
        }
        // else move towards sound.
        else {
            // WIP . Waiting on GridManager.cs
        }

    }

    
    // Performance optimization: move trigger volumes to empty child object
    // on different layer that only collides with targets to avoid collision
    // overhead; while allowing zombies to still have collision with each
    // other
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isATarget(collision.gameObject))
        {
            // insert to list of nearby humans
            nearbyTargets.Add(collision.gameObject);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isATarget(collision.gameObject))
        {
            // remove from list of nearby humans
            nearbyTargets.Remove(collision.gameObject);
        }
    }

    private bool isATarget(GameObject target)
    {
        foreach (string targetTag in TARGET_TAGS)
        {
            if (target.tag == targetTag)
            {
                return true;
            }
        }
        return false;
    }

    private void moveSelf(Vector3 direction)
    {
        if (PHYSICS_BASED)
        {
           // WIP
        } else
        {
            this.transform.position += direction * Time.deltaTime;
        }
    }

    // return normal vector direction towards nearest human
    private Vector3 directionToNearestHuman()
    {
        // find the closest human
        GameObject closestHuman = nearbyTargets[0];
        float closestDistance = Vector3.Distance(this.transform.position, closestHuman.transform.position);
        foreach (GameObject human in nearbyTargets)
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
}
