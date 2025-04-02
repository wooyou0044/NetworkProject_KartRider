using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricadeController : MonoBehaviour
{
    [SerializeField] float collisionDestroyTime;
    public float destoryTime;

    public TestCHMKart kartCtrl { get; set; }
    public ItemNetController itemCtrl { get; set; }

    Rigidbody rb;
    Collider col;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        
    }

    public void OffBarricade()
    {
        StartCoroutine(PlayerCollisonBarricade());
    }

    IEnumerator PlayerCollisonBarricade()
    {
        Debug.Log("Destroy Time : " + collisionDestroyTime);
        yield return new WaitForSeconds(collisionDestroyTime);
        kartCtrl.MakeDisableBarricade(gameObject);
        //gameObject.SetActive(false);
    }

    public void GoDownBarricade()
    {
        rb.constraints = RigidbodyConstraints.None;
        col.isTrigger = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
        {
            itemCtrl.RequestDisableItem(gameObject);
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Deadzone"))
        {
            itemCtrl.RequestDisableItem(gameObject);
            gameObject.SetActive(false);
        }
    }
}
