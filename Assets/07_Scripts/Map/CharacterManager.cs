using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    private PhotonView _photonView;
    void Awake()
    {
        _photonView = GetComponent<PhotonView>();        
    }

    void Start()
    {
        
    }
}
