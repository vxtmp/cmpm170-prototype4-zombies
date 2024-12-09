using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    // Singleton
    public static ZombieManager Instance { get; private set; }

    public int zombieCount = 0;
    private const int MAX_ZOMBIES = 10;

    private float spawnTimer = 0.0f;
    private const float SPAWN_INTERVAL = 5.0f;
    private const float MAX_PER_SPAWN = 0.1f; // max zombies per spawn interval. as % of MAX_ZOMBIES
                                              // minimum of a max of 1. random between 0 and this value.

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

    [SerializeField] private List<string> zombieTargetTags = new List<string>();
    private void Start()
    {
        
    }
    private void Update()
    {
        
    }
    public void addZombieTarget(string tag)
    {
        zombieTargetTags.Add(tag);
    }
    public List<string> getZombieTargets()
    {
        return zombieTargetTags;
    }

    public void removeZombieTarget(string tag)
    {
        if (zombieTargetTags.Contains(tag))
            zombieTargetTags.Remove(tag);
    }
}
