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
        PhotonNetwork.ConnectUsingSettings();
        if(FirebaseDBManager.Instance.User.DisplayName == null)
        {

        }
        //서버 접속 시도, 저장된 닉네임 확인, 저장된 닉네임이 없다면
        //user.DisplayName이 없다면 만들어야지 
        //인풋필드 생성
        
        //PhotonNetwork.NickName = FirebaseDBManager.Instance.User.DisplayName;
    }
}
