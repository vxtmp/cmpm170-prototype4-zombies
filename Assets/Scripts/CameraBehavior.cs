using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // set camera position to player position get from gamemanager
        Vector2 playerPosition = GameManager.Instance.getPlayerPosition();
        this.gameObject.transform.position = new Vector3(playerPosition.x, playerPosition.y, -10);
    }
}
