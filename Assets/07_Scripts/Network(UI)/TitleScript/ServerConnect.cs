using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ServerConnect : MonoBehaviourPunCallbacks
{    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();//유저 세팅값으로 서버연결
    }
    
    public bool Connect()
    {
        return PhotonNetwork.IsConnected;
    }


    public override void OnConnectedToMaster()
    {
        //파이어베이스의 유저네임과 포톤의 닉네임을 연결시킴        
        PhotonNetwork.NickName = FirebaseDBManager.Instance.User.DisplayName; 
    }
}
