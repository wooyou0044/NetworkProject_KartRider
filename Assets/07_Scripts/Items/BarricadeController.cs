using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricadeController : MonoBehaviour
{
    [SerializeField] float DestroyTime;

    void Start()
    {
    }

    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        //if(collision.gameObject.CompareTag("Player"))
        //{
        //    Debug.Log("ºÎµúÈû");
        //    StartCoroutine(PlayerCollisonBarricade());
        //}
    }

    void OnCollisionStay(Collision collision)
    {
        //if (collision.gameObject.CompareTag("Player"))
        //{
        //    if(gameObject.activeSelf == true)
        //    {
        //        Debug.Log("ºÎµúÈû");
        //        StartCoroutine(PlayerCollisonBarricade());
        //    }
        //}
    }

    public void OffBarricade()
    {
        StartCoroutine(PlayerCollisonBarricade());
    }

    IEnumerator PlayerCollisonBarricade()
    {
        Debug.Log("Destroy Time : " + DestroyTime);
        yield return new WaitForSeconds(DestroyTime);
        gameObject.SetActive(false);
    }
}
