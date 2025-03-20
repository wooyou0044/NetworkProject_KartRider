using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{    
    [SerializeField] public LobbyUIManager lobbyUiMgr;
    public delegate void RoomListUpdated(List<RoomInfo> roomList);
    public static event RoomListUpdated OnRoomListUpdateEvent;

    private void Start()
    {
        PhotonNetwork.JoinLobby();
    }
    
    public void CearteRoomBtnClick()
    {
        PhotonNetwork.CreateRoom(lobbyUiMgr.roomNameInputField.text);
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(lobbyUiMgr.roomNameText.text);
    }
    public override void OnJoinedRoom()
    {
        SceneCont.Instance.SceneAsync("RoomScene");
        SceneCont.Instance.Oper.allowSceneActivation = true;
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        OnRoomListUpdateEvent?.Invoke(roomList);
        foreach (var roomInfo in roomList)
        {
            var room = Instantiate(lobbyUiMgr.roomPrefab, lobbyUiMgr.roomListPanel.transform);
            

        }
    }
}
