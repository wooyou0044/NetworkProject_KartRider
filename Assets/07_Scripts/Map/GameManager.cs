using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
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
    public RaceResultController raceResultController;

    // 전체 캐릭터 풀 설정
    private CharacterSo[] _characterSoArray;

    // ToDo 실제 네트워크 연결하면 네트워크 상 정보로 바꿀 것
    [Header("생성할 카트 & 캐릭터 프리팹 지정")] 
    public GameObject kartPrefab;
    public CharacterSo characterSo;
    
    [Header("게임 진행과 관련한 변수")]
    public int startCountDownSeconds = 3;
    public int retireCountDownSeconds = 10;
    public int backToRoomCountDownSeconds = 3;
    
    private Player _winner;

    // 포톤 instantiate한 카트 인스턴스
    [HideInInspector] public TestCHMKart kartCtrl;
    public GameObject playerChar { get; private set; }
    
    // 방장이 가지고 있는 준비된 참가자들 리스트
    private List<Player> _readyPlayers;
    // 결과창 위한 플레이어 리스트 & 딕셔너리
    public Dictionary<Player, float> finishedPlayerTime;

    private Coroutine retireCountDown;
    
    private void Awake()
    {
        _readyPlayers = new List<Player>();
        _characterSoArray = Resources.LoadAll<CharacterSo>("Character");
        
        finishedPlayerTime = new Dictionary<Player, float>();
    }
    
    private void Start()
    {
        // ToDo 실제 네트워크 연결하면 네트워크 상 캐릭터, 카트 정보로 바꿀 것
        DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
        if (pool != null)
        {
            pool.ResourceCache.Add(kartPrefab.name, kartPrefab);
            foreach (var soCharacter in _characterSoArray)
            {
                if (pool.ResourceCache.ContainsKey(soCharacter.characterName))
                {
                    continue;
                }                
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
        gameObject.GetPhotonView().RPC("ReceiveLoadFinished", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
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
            gameObject.GetPhotonView().RPC("StartCountDown", RpcTarget.AllViaServer);
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
    
    // 카운트다운 세기, 실제 3초보단 살짝 길다. 
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
    
    [PunRPC]
    public void OnSomePlayerFinished(Player player, float elapsedTime)
    {
        if (_winner == null)
        {
            _winner = player;
            retireCountDown = StartCoroutine(RetireCountDown());
        }

        // 다른 사람들거 추가로 저장
        if (!PhotonNetwork.LocalPlayer.Equals(player))
        {
            finishedPlayerTime.Add(player, elapsedTime);
            if (raceResultController.gameObject.activeInHierarchy)
            {
                raceResultController.UpdateFinished(player);
            }
        }
    }

    public IEnumerator RetireCountDown()
    {
        while(retireCountDownSeconds > 0)
        {
            StartCoroutine(mainTextController.ShowTextOneSecond(retireCountDownSeconds.ToString()));
            retireCountDownSeconds--;
            yield return new WaitForSeconds(1f);
        }
        
        if (!kartCtrl.GetComponent<RankManager>().IsFinish())
        {
            TurnOffInGameUI();
            timeUIController.StopTimer();
            mainTextController.SetColor(Color.white);
            mainTextController.mainText.alignment = TextAnchor.MiddleLeft;
            mainTextController.ShowMainText("RETIRE");
        }

        TurnOffMyKartControl();
        ShowFinalResult();
        
        // ToDo 방으로 돌아가기
        while (backToRoomCountDownSeconds > 0)
        {
            string defaultTxt = "게임 완료, 방으로 돌아갑니다.. ";
            raceResultController.backToRoomText.text = defaultTxt + backToRoomCountDownSeconds;
            backToRoomCountDownSeconds--;
            yield return new WaitForSeconds(1f);
        }

        if (PhotonNetwork.LocalPlayer.Equals(PhotonNetwork.MasterClient))
        {
            PhotonNetwork.LoadLevel("RoomScene");            
        }
    }

    // 들어왔을떄 전달
    public void OnFinished()
    {
        RankManager rankManager = kartCtrl.gameObject.GetComponent<RankManager>();
        PhotonView kartPv = kartCtrl.gameObject.GetPhotonView();
        
        timeUIController.StopTimer();
        float elapsedTime = timeUIController.GetElapsedTime();
        
        // 내 기록 저장
        finishedPlayerTime.Add(kartPv.Owner, elapsedTime);        
        
        // Rank 매니저 나와 다른 사람들에게 업데이트, 내꺼는 미리 업데이트함
        rankManager.SetFinish(true);
        kartPv.RPC("SetFinish", RpcTarget.Others, true);
        gameObject.GetPhotonView().RPC("OnSomePlayerFinished", RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer, elapsedTime);
        
        TurnOffInGameUI();
        TurnOffMyKartControl();
        ShowFinalResult();
    }

    public void TurnOffInGameUI()
    {
        kartUIController.gameObject.SetActive(false);
        inventoryUI.gameObject.SetActive(false);
        mnMapController.gameObject.SetActive(false);
        rankUIController.gridRoot.gameObject.SetActive(false);
    }
    
    public void ShowFinalResult()
    {
        raceResultController.gameObject.SetActive(true);
    }

    // 조작 안되게 하고 더 할 처리 필요한것 있는지 확인
    public void TurnOffMyKartControl()
    {
        kartCtrl.isRacingStart = false;
        StartCoroutine(kartCtrl.DecelerateOverTime(1f));                
    }
}
