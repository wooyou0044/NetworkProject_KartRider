using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private LobbyUIManager lobbyUiMgr;
    [SerializeField] private RoomEntry roomEntry;

    private void Start()
    {
        PhotonNetwork.JoinLobby();
    }

    public void CreateRoomBtnClick()
    {
        CreateRoom(lobbyUiMgr.roomNameInputField.text, lobbyUiMgr.roomPasswordInputField.text);
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(roomEntry.roomNameText.text);
    }
    public override void OnJoinedRoom()
    {
        //맵 변경은 코루틴으로 돌리자
        SceneCont.Instance.SceneAsync("RoomScene");
        SceneCont.Instance.Oper.allowSceneActivation = true;
    }

    public void CreateRoom(string roomName, string password)
    {
        Hashtable custom = new Hashtable
        {
            { "RoomName", roomName },
            { "Password", password }
        };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 1,
            CustomRoomProperties = custom,
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "Password" }
        };
        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 기존 방 리스트 정리
        foreach (Transform child in lobbyUiMgr.roomListPanel.transform)
        {
            Destroy(child.gameObject);
        }

        int roomIndex = 1;
        // 새로운 방 리스트 업데이트
        foreach (RoomInfo roomInfo in roomList)
        {
            string roomNumber = roomIndex.ToString("D6"); // 6자리 형식으로 변환 (예: 000001)
            roomInfo.CustomProperties["RoomNumber"] = roomNumber;
            lobbyUiMgr.AddRoomToList(roomInfo);
            roomIndex++;            
        }
    }
    public void JoinRoom(string roomName)
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            return;
        }
        PhotonNetwork.JoinRoom(roomName);
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        switch (returnCode)
        {
            case 32765: // 방이 가득 참
                lobbyUiMgr.RoomJoinFaildeText("자리가 없어 입장할 수 없습니다.");
                break;

            case 32764: // 방 닫힘 또는 게임 시작
                lobbyUiMgr.RoomJoinFaildeText("게임이 이미 진행중이라 입장할 수 없습니다.");
                break;

            case 32762: // 인증 실패
                lobbyUiMgr.RoomJoinFaildeText("입력하신 비밀번호가 일치하지 않습니다. 다시 확인해 주세요.");
                break;

            default: // 기타 실패 사유                
                lobbyUiMgr.RoomJoinFaildeText($"방 입장에 실패했습니다. 오류 코드: {returnCode}");
                break;
        }
    }
}