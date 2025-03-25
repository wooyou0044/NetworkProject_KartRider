using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonConnectManager : MonoBehaviourPunCallbacks
{
    private string _gameVersion = "1";
    private string _testRoomName = "TestRoom";
    private string _testLobbyName = "scTestLobby";

    void Start()
    {
        TestConnectPhotonServer();
    }    
    
    // 테스트용 코드 덩어리들, 마스터 접속 상태인지 확인 후 접속 & 방 만들기 까지 
    public void TestConnectPhotonServer()
    {
        PhotonNetwork.GameVersion = _gameVersion;
        
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            PhotonNetwork.JoinOrCreateRoom(
                _testRoomName,
                new RoomOptions{ MaxPlayers = 8 },
                new TypedLobby(_testLobbyName, LobbyType.Default)
            );            
        }
    }

    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.InRoom)
        {
            PhotonNetwork.JoinOrCreateRoom(
                _testRoomName,
                new RoomOptions{ MaxPlayers = 8 },
                new TypedLobby(_testLobbyName, LobbyType.Default)
            );            
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            Debug.Log("player" + player.ActorNumber + " : " + player.NickName);
        }
        
        Debug.Log("roomName : " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("currentPlayers : " + PhotonNetwork.PlayerList.Length);
    }
}
