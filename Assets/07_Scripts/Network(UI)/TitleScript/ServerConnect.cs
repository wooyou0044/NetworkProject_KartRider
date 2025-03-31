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
        PhotonNetwork.ConnectUsingSettings();//���� ���ð����� ��������
    }
    //���� Ȯ���� �Ұ����� ��ȯ ���ִ� �޼���
    public bool Connect()
    {
        bool isConnectedToMaster = PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer;
        return isConnectedToMaster;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("���� ���� ����. ����: " + cause);
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("������ ���� ���� �Ϸ�");
        //���̾�̽��� �������Ӱ� ������ �г����� �����Ŵ 
        PhotonNetwork.NickName = FirebaseDBManager.Instance.User.DisplayName; 
    }
}
