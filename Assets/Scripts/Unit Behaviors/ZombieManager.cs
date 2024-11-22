using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    // Singleton
    public static ZombieManager Instance { get; private set; }
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
