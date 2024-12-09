using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Object references. Set in inspector.
    [SerializeField] private GameObject player;
    [SerializeField] private HumanManager humanManager;
    [SerializeField] private ZombieManager zombieManager;
    //[SerializeField] private TMP_Text gameoverText;
    [SerializeField] private GameObject gameoverText;
    private bool gameover = false;


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
        if(!gameover)
        {
            if ((humanManager.instHumans.Count == 0 && zombieManager.zombieCount <= 0) || player == null)
            {
                gameoverText.SetActive(true);
                gameover = true;
            }
        }
    }

    public void Reset()
    {
        SceneManager.LoadScene("main");
    }
}
