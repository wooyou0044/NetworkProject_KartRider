using System;
using Cinemachine;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum MapEnum {
    DaisyCircuit,
    Default
}

public class MapManager : MonoBehaviourPunCallbacks
{
    private string _sceneName;
    private Vector3 _defaultRotationVector;

    /* 게임 종료 및 결과 전달 위한 게임 매니저, 종료시에만 호출하자. */
    private GameManager _gameManager;
    
    /* 현재 랩, 종료 랩 설정 */
    private int _myCurrentLap = 0;    
    
    [Header("맵 전체 바퀴 수 설정")]
    public int totalLap = 3; 
    
    [Header("맵 시작 / 종료 포지션 설정")]
    public Transform[] startPos;
    public Transform finishLine;

    [Header("필수 체크포인트 설정")]
    public CheckPoint[] essentialCheckPoints;

    [Header("Dolly Track Waypoint 설정")]
    public GameObject checkPoints;
    private CheckPoint[] _allCheckPoints;    
    
    // 돌리 트랙 설정 (순위 측정용)
    public CinemachineSmoothPath dollyPath;
    
    // 디버깅용 내 마지막 체크포인트 보기
    [Header("디버그 필드")]
    [SerializeField] private Transform myLastcheckPoint;
    [HideInInspector] public UnityEvent onFinishEvent;
    
    public int MyCurrentLap => _myCurrentLap;

    private void Awake()
    {
        _sceneName = SceneManager.GetActiveScene().name;
        
        MapEnum mapEnum = (MapEnum) Enum.Parse(typeof(MapEnum), _sceneName);

        // 차가 기본으로 돌아가있으므로 수정
        Vector3 kartRotationVector = new Vector3(0, -90, 0);
        switch (mapEnum)
        {
            case MapEnum.DaisyCircuit :
                _defaultRotationVector = kartRotationVector + Vector3.zero;
                break;
            case MapEnum.Default:
                _defaultRotationVector = kartRotationVector + Vector3.zero;
                break;
            default: 
                Debug.Log("설정된 맵이 없습니다");
                _defaultRotationVector = kartRotationVector + Vector3.zero;
                break;
        }
        
        _allCheckPoints = checkPoints.transform.GetComponentsInChildren<CheckPoint>();
        SetAllCheckPointToDollyTrack();
    }

    private void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
    
    // 바닥으로 떨어졌을때
    public void OnTouchDeadZone(Collider kart, GameObject deadZone)
    {
        if (!kart.gameObject.GetPhotonView().IsMine)
        {
            return;
        }
        
        Debug.Log("OnTouchDeadZone");
        RespawnToPos(kart.gameObject);
    }

    // 결승선 닿았을때 호출하는 메서드
    // 완전 종료 필요할 경우에 게임 매니저 호출.
    public void OnTouchFinishLine(Collider kart, GameObject line)
    {
        PhotonView pv = kart.gameObject.GetPhotonView();
        TestCHMKart kartCtrl = kart.GetComponent<TestCHMKart>();
        
        if (pv == null || !pv.IsMine)
        {
            return;
        }
        
        Debug.Log("OnTouchFinishLine");
        
        if (MyCurrentLap > 0 && !IsEssentialCheckPointsPassed())
        {
            Debug.Log("필수 체크 포인트 안지나갔음");
            return;
        }
        
        if(MyCurrentLap < totalLap)
        {
            _myCurrentLap = MyCurrentLap + 1;
            myLastcheckPoint = finishLine;
            ResetAllCheckPoints();
            pv.RPC("SetLap", RpcTarget.All, MyCurrentLap);
        }
        else
        {
            ResetAllCheckPoints();
            _gameManager.OnFinished();
            pv.RPC("SetFinish", RpcTarget.All, true);
            
            // 조작 안되게 하고 더 할 처리 필요한것 있는지 확인
            kartCtrl.isRacingStart = false;
        }
        
        onFinishEvent.Invoke();
    }

    /* 체크 포인트 닿았을때 호출하는 메서드 */
    public void OnTouchCheckPoint(Collider kart, GameObject checkPoint)
    {
        if (!kart.gameObject.GetPhotonView().IsMine)
        {
            return;
        }
        
        Debug.Log("OnTouchCheckPoint, name : " + checkPoint.name);
        myLastcheckPoint = checkPoint.transform.parent;
    }

    // 그냥 마지막 체크포인트로 돌리고 싶을 때
    public void RespawnToPos(GameObject playerKart)
    {
        RespawnToPos(playerKart, null);
    }
    
    // 리스폰시 실행 (카트의 R키 누를때 혹은 바닥으로 떨어졌을떄)
    public void RespawnToPos(GameObject playerKart, Transform somePoint)
    {
        Rigidbody kartRigid = playerKart.GetComponent<Rigidbody>();

        Vector3 penalty = Vector3.up * 2.5f;
        
        // Rigidbody를 붙인채로 transform을 변경하면, 속도가 실제로 변한다.
        // isKinematic을 켰다 꺼주자 (운동 효과 제거)
        kartRigid.isKinematic = true;

        // 특정 리스폰 포인트가 있다면, 그 포인트로 이동, 로테이션은 어떤 기준으로 할지 설정 필요
        if (somePoint != null)
        {
            playerKart.transform.position = somePoint.position + penalty;
            playerKart.transform.rotation = somePoint.transform.rotation;
            playerKart.transform.Rotate(_defaultRotationVector);
        }
        else
        {
            playerKart.transform.position = myLastcheckPoint.position + penalty;
            playerKart.transform.rotation = myLastcheckPoint.transform.rotation;
            playerKart.transform.Rotate(_defaultRotationVector);
        }
        
        kartRigid.isKinematic = false;        
    }
    
    // 카트 맵에 위치하면서 맵 매니저와 관련한 부분들 초기화
    public void PlaceToStartPos(int randomNum, GameObject playerKart)
    {
        Rigidbody kartRigid = playerKart.GetComponent<Rigidbody>();
        Transform startingPoint = startPos[randomNum];
        myLastcheckPoint = startingPoint;

        // Rigidbody를 붙인채로 transform을 변경하면, 속도가 실제로 변한다.
        // isKinematic을 켰다 꺼주자 (운동 효과 제거)
        kartRigid.isKinematic = true;

        // 출발선 표시 살짝 뒤에서 시작
        Vector3 startBehind = Vector3.left * 2;
        playerKart.transform.position = startingPoint.position + startBehind;
        playerKart.transform.rotation = startingPoint.rotation;
        kartRigid.isKinematic = false;
    }
    
    private bool IsEssentialCheckPointsPassed()
    {
        foreach (var checkPoint in essentialCheckPoints)
        {
            if (!checkPoint.IsPassed)
            {
                return false;
            }
        }

        return true;
    }
    
    private void ResetAllCheckPoints()
    {
        foreach (var checkPoint in _allCheckPoints)
        {
            checkPoint.ResetFlag();
        }
    }

    private void SetAllCheckPointToDollyTrack()
    {
        Vector3 checkPointRoot = dollyPath.gameObject.transform.localPosition;
        CinemachineSmoothPath.Waypoint[] waypoints = new CinemachineSmoothPath.Waypoint[_allCheckPoints.Length+1];        
        dollyPath.m_Waypoints = waypoints;
        
        // 0번째 웨이포인트는 시작점으로 설정
        int index = 0;
        waypoints[index++].position = Vector3.zero;
        
        foreach (CheckPoint cp in _allCheckPoints)
        {
            waypoints[index++].position = cp.transform.parent.localPosition - checkPointRoot;
        }
    }
}
