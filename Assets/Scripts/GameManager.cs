using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Object references. Set in inspector.
    [SerializeField] private GameObject player;
    
    public GameObject getPlayer() { return player; }

    private Vector2 lastPlayerPosition = new Vector2(0, 0);
    public Vector2 getPlayerPosition() {
        if (player != null)
        {
            lastPlayerPosition = player.transform.position;
        }
        return lastPlayerPosition;
    }


    // Singleton
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
