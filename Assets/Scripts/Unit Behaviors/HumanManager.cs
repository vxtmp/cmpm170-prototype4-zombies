using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanManager : MonoBehaviour
{
    [SerializeField] private GameObject humanPrefab;
    public List<GameObject> instHumans = new List<GameObject>();

    [SerializeField] private List<Item> items;
    [SerializeField] private GameObject ItemPrefab;

    [SerializeField] private float chanceNoItem = 0.10f;
    [SerializeField] private float chanceTwoItems = 0.90f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // manage human behavior

    public void InstantiateHuman(float x, float y)
    {
        Quaternion rotation = new Quaternion(0,0,0,0);
        GameObject humanObj = Instantiate(humanPrefab, new Vector2(x,y), rotation, transform);
        instHumans.Add(humanObj);
        ChooseItem(humanObj);
        instHumans.Remove(humanObj);
        //humanObj.GetComponent<HumanScript>().DropItems();
    }

    private void ChooseItem(GameObject human)
    {
        float chance = Random.Range(0f, 1f);
        if (chance > chanceNoItem)
        {
            int rand = Random.Range(0, items.Count);
            human.GetComponent<HumanScript>().items.Add(items[rand]);
        }

        if (chance > chanceTwoItems)
        {
            int rand = Random.Range(0, items.Count);
            human.GetComponent<HumanScript>().items.Add(items[rand]);
        }
    }
}
