using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAcquisitionRange : MonoBehaviour
{
    private List<GameObject> targetsInRange = new List<GameObject>();
    public List<GameObject> getTargetsInRange()
    {
        return targetsInRange;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;
        GameObject collisionObj = collision.gameObject;
        if (collisionObj == null) return;
        if (isATarget(collisionObj))
        {
            targetsInRange.Add(collisionObj);
        }
    }

    private bool isATarget(GameObject obj)
    {
        List<string> tags = HumanManager.Instance.getHumanTargets();
        foreach (string tag in tags)
        {
            if (obj.tag == tag)
                return true;
        }
        return false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject collisionObj = collision.gameObject;
        if (targetsInRange.Contains(collisionObj))
            targetsInRange.Remove(collisionObj);
    }

}
