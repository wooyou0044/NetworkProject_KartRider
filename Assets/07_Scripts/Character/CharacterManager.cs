using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    private PhotonView _photonView;
    private GameObject _myKart;
    void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        Player owner = _photonView.Owner;
        TestCHMKart[] karts = FindObjectsOfType<TestCHMKart>();
        
        foreach (var kart in karts)
        {
            if (owner.Equals(kart.gameObject.GetComponent<PhotonView>().Owner))
            {
                Transform characterTr = kart.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
                transform.parent = characterTr;
                transform.position = characterTr.position;
                
                Quaternion characterDeafultRot = Quaternion.Euler(0, 180, 0);
                transform.rotation = characterDeafultRot;
                break;
            }
        }
    }
}
