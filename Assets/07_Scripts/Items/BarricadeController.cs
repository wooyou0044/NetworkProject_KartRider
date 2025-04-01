using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricadeController : MonoBehaviour
{
    [SerializeField] float DestroyTime;

    public TestCHMKart kartCtrl;
    void Start()
    {
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
        Debug.Log("Destroy Time : " + DestroyTime);
        yield return new WaitForSeconds(DestroyTime);
        kartCtrl.MakeDisableBarricade(gameObject);
        //gameObject.SetActive(false);
    }
}
