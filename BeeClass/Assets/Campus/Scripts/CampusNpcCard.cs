using UnityEngine;
public class CampusNpcCard : MonoBehaviour
{
    public CampusCard card;
    public Renderer indicator;
    void OnTriggerEnter(Collider other) { Give(other); }

    void Give(Collider other)
    {
        var inventory=other.GetComponentInParent<CampusInventory>();
        if(inventory != null) inventory.TryAdd(card);
    }
    void Update()
    {
        if(indicator) indicator.transform.localRotation=Quaternion.Euler(0,Time.time*45,0);
    }
}

