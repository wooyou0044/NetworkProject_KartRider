using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemController : MonoBehaviour
{
    public ItemManager item;

    Rigidbody rb;
    Collider col;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if(item.itemType == ItemType.banana)
            {
                rb.isKinematic = true;
                col.isTrigger = true;
            }
            if(item.itemType == ItemType.barricade)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
        }
    }
}
