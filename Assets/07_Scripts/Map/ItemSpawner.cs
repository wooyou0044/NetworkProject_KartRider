using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Range(1f, 5f)]
    public float itemRespawnTime = 3f;
    
    public GameObject[] itemList;
    public PhotonView pv;

    
    private void Awake()
    {
        pv = gameObject.GetPhotonView();
    }
    
    private void OnEnable()
    {
        foreach (var itemBox in itemList)
        {
            ItemBoxController ibc = itemBox.GetComponent<ItemBoxController>();
            ibc.onTouchItemBox.AddListener(OnTouchItemBox);
        }
    }
    
    private void OnDisable()
    {
        foreach (var itemBox in itemList)
        {
            ItemBoxController ibc = itemBox.GetComponent<ItemBoxController>();
            ibc.onTouchItemBox.RemoveListener(OnTouchItemBox);
        }
    }

    public int FindBoxIndex(GameObject box)
    {
        for (int i = 0; i < itemList.Length; i++)
        {
            if (box.Equals(itemList[i]))
            {
                return i;
            }
        }

        Debug.LogError("없는 박스입니다!?");
        return 0;
    }

    public void OnTouchItemBox(GameObject box)
    {
        int boxIndex = FindBoxIndex(box);
        pv = gameObject.GetPhotonView();
        pv.RPC("RemoveItemBox", RpcTarget.All, boxIndex);
    }

    [PunRPC]
    public void RemoveItemBox(int boxIndex)
    {
        ItemBoxController ibc = itemList[boxIndex].GetComponent<ItemBoxController>();
        ibc.InactiveItemBox();
        StartCoroutine(ibc.RespawnItemBox(itemRespawnTime));
    }
}
