using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ServerConnect : MonoBehaviourPunCallbacks
{
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public void ConnectToServer()
    {       
        PhotonNetwork.ConnectUsingSettings();//유저 세팅값으로 서버연결
        //파이어베이스의 유저네임과 포톤의 닉네임을 연결시킴
        //이제 파이어베이스의 닉네임과 유저의 닉네임이 같아짐
        PhotonNetwork.NickName = FirebaseDBManager.Instance.User.DisplayName;

    }
}
