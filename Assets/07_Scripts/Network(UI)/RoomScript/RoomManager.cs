using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class RoomManager : MonoBehaviourPunCallbacks
{

    private void Start()
    {
        Player[] players = PhotonNetwork.PlayerList;

        foreach (var player in players)
        {
            //이미지로 띄워주면 됨
            //배열의 0 번부터 차례대로
            Debug.Log("방 안의 사람들 목록"+ player.NickName);
        }
        
        if (PhotonNetwork.IsMasterClient == false)
        {
            //만약 마스터가 아니라면 스타트 버튼이 아니라 준비완료 버튼으로 띄우기
        }
    }

    public void StartGame()
    {
        if(PhotonNetwork.IsMasterClient == true)
        {
            //인 게임으로 씬이동
            PhotonNetwork.LoadLevel("InGameScene");
        }
    }
}

