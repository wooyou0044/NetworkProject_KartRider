using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BarricadeController : MonoBehaviour
{
    [SerializeField] float DestroyTime;

    void Start()
    {
    }

    void Update()
    {
        
    }

    [PunRPC]
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
