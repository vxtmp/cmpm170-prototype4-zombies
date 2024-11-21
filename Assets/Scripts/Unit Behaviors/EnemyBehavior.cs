using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    private const bool PHYSICS_BASED = false;
    private List<GameObject> nearbyHumans;
    // Start is called before the first frame update
    void Start()
    {
        nearbyHumans = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (nearbyHumans.Count > 0)
        {
            moveSelf(directionToNearestHuman());
        }
        else { 
            
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Human" || collision.gameObject.tag == "Player")
        {
            // insert to list of nearby humans
            nearbyHumans.Add(collision.gameObject);
        }
    }

    private void moveSelf(Vector3 direction)
    {
        if (PHYSICS_BASED)
        {
           
        } else
        {
            this.transform.position += direction * Time.deltaTime * PlayerBehavior.PLAYER_SPEED;
        }
    }

    // return normal vector direction towards nearest human
    private Vector3 directionToNearestHuman()
    {
        // find the closest human
        GameObject closestHuman = nearbyHumans[0];
        float closestDistance = Vector3.Distance(this.transform.position, closestHuman.transform.position);
        foreach (GameObject human in nearbyHumans)
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
