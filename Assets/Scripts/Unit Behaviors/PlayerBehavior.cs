using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    public const float PLAYER_SPEED = 5.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // move
        parseWASD();
        updateRotation();
        moveSelf();
    }

    private float upDirection = 0.0f;
    private float rightDirection = 0.0f;
    private Vector3 direction = new Vector3(0.0f, 0.0f, 0.0f);
    public void parseWASD()
    {
        upDirection = 0.0f;
        rightDirection = 0.0f;
        if (Input.GetKey(KeyCode.W))
        {
            upDirection++;
        }
        if (Input.GetKey(KeyCode.S))
        {
            upDirection--;
        }
        if (Input.GetKey(KeyCode.A))
        {
            rightDirection--;
        }
        if (Input.GetKey(KeyCode.D))
        {
            rightDirection++;
        }
        direction = new Vector3(rightDirection, upDirection, 0.0f);
        direction.Normalize();
    }
    // WASD move translates transform.position
    public void moveSelf()
    {
        this.transform.position += direction * Time.deltaTime * PLAYER_SPEED;
    }
    // set player rotation in direction
    public void updateRotation()
    {
        // set player rotation to look in direction vector direction, rotating only z rotation
        if (direction.x == 0 && direction.y == 0)
        {
            // do nothing
        }
        else if (direction.x < 0)
        {
            if (direction.y < 0)
            {
                this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 135.0f);
            }
            else if (direction.y == 0)
            {
                this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
            }
            else if (direction.y > 0)
            {
                this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 45.0f);
            }
        }
        else if (direction.x == 0)
        {
            if (direction.y < 0)
            {
                this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
            }
            else if (direction.y > 0)
            {
                this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            }
        }
        else if (direction.x > 0)
        {
            if (direction.y < 0)
            {
                this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -135.0f);
            }
            else if (direction.y == 0)
            {
                this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
            }
            else if (direction.y > 0)
            {
                this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -45.0f);
            }
        }
    }

}
