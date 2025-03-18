using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{    
    [SerializeField] public LobbyUIManager lobbyUiMgr;

    private void Start()
    {
        PhotonNetwork.JoinLobby();
    }

    public void CreateRoomBtnOnClick()
    {

    }


}
