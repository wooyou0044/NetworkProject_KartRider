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
    //연결 확인을 불값으로 반환 해주는 메서드
    public bool Connect()
    {
        bool isConnectedToMaster = PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer;
        return isConnectedToMaster;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("연결 끊김 감지. 사유: " + cause);
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("마스터 서버 접속 완료");
        //파이어베이스의 유저네임과 포톤의 닉네임을 연결시킴 
        PhotonNetwork.NickName = FirebaseDBManager.Instance.User.DisplayName; 
    }
}
