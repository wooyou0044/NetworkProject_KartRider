using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/* 테스트용 포톤 커넥터, 디버깅용으로 로그인 안해도 들어가서 멀티 되도록 설정 */
public class PhotonConnectManager : MonoBehaviourPunCallbacks
{
    private string _gameVersion = "1";
    private string _testRoomName = "scTestRoom";
    private string _testLobbyName = "scTestLobby";

    private GameManager _gameManager;
    private RankUIController _rankUIController;
    
    void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _rankUIController = FindObjectOfType<RankUIController>();
    }
    
    void Start()
    {
        TestConnectPhotonServer();
    }
    
    // 테스트용 코드 덩어리들, 마스터 접속 상태인지 확인 후 접속 & 방 만들기 까지 
    public void TestConnectPhotonServer()
    {
        PhotonNetwork.GameVersion = _gameVersion;
        PhotonNetwork.NickName = "테스트" + Random.Range(0, 1000);
        
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            if (!PhotonNetwork.InRoom)
            {
                PhotonNetwork.JoinOrCreateRoom(
                    _testRoomName,
                    new RoomOptions { MaxPlayers = 8 },
                    new TypedLobby(_testLobbyName, LobbyType.Default)
                );
            }
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

    // 테스트용, 이럴 일 없겠지만 누군가 방에 참가했을 때
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            Debug.Log("player" + player.ActorNumber + " : " + player.NickName);
        }
        
        Debug.Log("roomName : " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("currentPlayers : " + PhotonNetwork.PlayerList.Length);
        
        // RPC로 뉴비에게 카트의 랩 정보 세팅해줌
        _gameManager.kartCtrl.gameObject.GetPhotonView().RPC("SetLap", newPlayer, _gameManager.mapManager.MyCurrentLap);
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("플레이어 나감 : " + otherPlayer.NickName);
        Debug.Log(otherPlayer.TagObject);
        
        if (PhotonNetwork.IsMasterClient)
        {
            _gameManager.RemoveReadyPlayer(otherPlayer);
        }
        
        ((GameObject)otherPlayer.TagObject).SetActive(false);
        Destroy((GameObject)otherPlayer.TagObject);
    }

    /* 테스트용 방 곧바로 입장시, 바로 카트 생성해준다. */
    public override void OnJoinedRoom()
    {
        _gameManager.InstantiateObject();
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("방 참여 실패, code : " + returnCode + " msg : " + message);
    }
}
