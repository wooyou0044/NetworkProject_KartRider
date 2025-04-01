using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("카메라")]
    public CinemachineVirtualCamera virtualCamera;

    [Header("인게임 UI 요소")] 
    public KartUIController kartUIController;
    public InventoryUI inventoryUI;
    public TimeUIController timeUIController;
    public MainTextController mainTextController;
    public MiniMapController mnMapController;
    public RankUIController rankUIController;
    
    [Header("맵 요소들 매니저")]
    public MapManager mapManager;

    // 전체 캐릭터 풀 설정
    private CharacterSo[] _characterSoArray;

    // ToDo 실제 네트워크 연결하면 네트워크 상 정보로 바꿀 것
    [Header("생성할 카트 & 캐릭터 프리팹 지정")] 
    public GameObject kartPrefab;
    public CharacterSo characterSo;
    
    [Header("게임 진행과 관련한 변수")]
    public int startCountDownSeconds = 3;
    public int retireCountDownSeconds = 10;
    
    private PhotonView _gameManagerView;
    private Player _winner;

    // 방장이 가지고 있는 준비된 참가자들 리스트
    private List<Player> _readyPlayers;
    // 포톤 instantiate한 카트 인스턴스
    [HideInInspector] public TestCHMKart kartCtrl;

    public GameObject playerChar { get; private set; }

    private void Awake()
    {
        _readyPlayers = new List<Player>();
        _characterSoArray = Resources.LoadAll<CharacterSo>("Character");
    }
    
    private void Start()
    {
        // ToDo 실제 네트워크 연결하면 네트워크 상 캐릭터, 카트 정보로 바꿀 것
        _gameManagerView = GetComponent<PhotonView>();
        DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
        if (pool != null)
        {
            pool.ResourceCache.Add(kartPrefab.name, kartPrefab);
            foreach (var soCharacter in _characterSoArray)
            {
                pool.ResourceCache.Add(soCharacter.characterName, soCharacter.characterPrefab);                
            }
        }

        // 방에 있다가 씬 전환인 경우 카트 생성 호출
        if (PhotonNetwork.InRoom)
        {
            InstantiateObject();
        }
    }
    
    public void InstantiateObject()
    {
        GameObject kart = PhotonNetwork.Instantiate(kartPrefab.name, Vector3.zero, Quaternion.identity);
        // kart에 붙어 있는 Controller 가져오기
        kartCtrl = kart.GetComponent<TestCHMKart>();
        //PhotonNetwork.Instantiate(characterPrefab.name, Vector3.zero, Quaternion.identity);
        GameObject playerChar = PhotonNetwork.Instantiate(characterSo.characterName, Vector3.zero, Quaternion.identity);
        kartCtrl.playerCharAni = playerChar.GetComponent<Animator>();
        StartCoroutine(PlaceToMap(kart));
    }

    IEnumerator PlaceToMap(GameObject kart)
    {
        yield return new WaitUntil(() => kart);
        
        virtualCamera.LookAt = kart.transform;
        virtualCamera.Follow = kart.transform;
        virtualCamera.gameObject.GetComponent<TestCHMCamer>().SetKart(kart);
        kartUIController.SetKart(kart);
        inventoryUI.SetKart(kart);
        mnMapController.SetFollower(kart.transform);
        
        // ToDo : 랜덤으로 설정해줄거면 actorNumber 대신 다른걸로 [0~7 숫자]
        Player kartOwner = kart.GetPhotonView().Owner;
        int num = kartOwner.ActorNumber - 1;
        
        mapManager.PlaceToStartPos(num, kart);
        SendLoadFinished();
    }
    
    /* 방장 클라에 준비 다 되었다고 쏘기 */
    public void SendLoadFinished()
    {
        _gameManagerView.RPC("ReceiveLoadFinished", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
    }

    /* 방장 클라가 방에 접속한 인원들 모두 로딩 되었는지 확인한다 */
    [PunRPC]
    public void ReceiveLoadFinished(Player player)
    {
        _readyPlayers.Add(player);

        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        Debug.Log("playerCount : " + playerCount);
        Debug.Log("ReadyPlayers : " + _readyPlayers.Count);
        
        if (_readyPlayers.Count == playerCount)
        {
            _gameManagerView.RPC("StartCountDown", RpcTarget.AllViaServer);
        }
    }
    
    /* 방장에게 누군가 나갔다고 전달 */
    public void RemoveReadyPlayer(Player player)
    {
        _readyPlayers.Remove(player);
    }
    
    [PunRPC]
    public void StartCountDown()
    {
        StartCoroutine("CountDown");
    }
    
    // ToDo : 카운트 다운 끝나면 움직이기, 실제 3초보단 살짝 길겠지만, 굳이 필요할까? 
    IEnumerator CountDown()
    {
        rankUIController.InitRankUI();
        yield return new WaitForSeconds(0.5f);
        while(startCountDownSeconds > 0)
        {
            StartCoroutine(mainTextController.ShowTextOneSecond(startCountDownSeconds.ToString()));
            startCountDownSeconds--;
            yield return new WaitForSeconds(1f);
        }
        
        // 카트 움직이기
        StartCoroutine(mainTextController.ShowTextOneSecond("GO!"));
        kartCtrl.isRacingStart = true;
        timeUIController.StartTimer();
    }
    
    /* 누군가 피니시 라인에 들어왔다 (최종 골인) */
    // 1. 리타이어 카운트 세기
    // 2. 진짜 누가 이겼는지 확인 필요 (할라나?)
    [PunRPC]
    public void OnSomePlayerFinish(Player player)
    {
        if (_winner == null)
        {
            _winner = player;
        }
        
        Debug.Log(_winner + "플레이어가 골인했습니다.");
    }

    // 들어왔을떄 전달
    public void OnFinished()
    {
        if (_winner == null)
        {
            _winner = PhotonNetwork.LocalPlayer;
            _gameManagerView.RPC("OnSomePlayerFinish", RpcTarget.AllViaServer, _winner);
        }
    }
}
