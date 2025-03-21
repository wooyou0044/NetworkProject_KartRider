using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum MapEnum {
    DaisyCircuit,
    Default
}

public class MapManager : MonoBehaviourPunCallbacks
{
    private string sceneName;
    private Quaternion defaultRotation;

    /* 게임 종료 및 결과 전달 위한 게임 매니저, 종료시에만 호출하자. */
    private GameManager gameManager;
    
    /* 현재 랩, 종료 랩 수 설정*/
    [SerializeField] private int myCurrentLap = 0;    
    [Header("맵 전체 바퀴 수 설정")]
    public int totalLap = 3; 
    
    [Header("맵 시작 / 종료 포지션 설정")]
    public Transform[] startPos;
    public Transform finishLine;
    private CheckPoint[] _allCheckPoints;
    public CheckPoint[] essentialCheckPoints;    
    [SerializeField] 
    private Transform myLastcheckPoint;

    private void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;
        
        MapEnum mapRotation = (MapEnum) Enum.Parse(typeof(MapEnum), sceneName);
        switch (mapRotation)
        {
            case MapEnum.DaisyCircuit: 
                defaultRotation = Quaternion.Euler(0, 180, 0); break;
            case MapEnum.Default: 
                defaultRotation = Quaternion.Euler(0, 0, 0); break;
            default: 
                Debug.Log("설정된 맵이 없습니다");
                defaultRotation = Quaternion.Euler(0, 0, 0);
                break;
        }
        
        _allCheckPoints = FindObjectsOfType<CheckPoint>();
    }

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
    
    // 바닥으로 떨어졌을때
    public void OnTouchDeadZone(Collider kart, GameObject deadZone)
    {
        RespawnToPos(kart.gameObject);
    }

    // 결승선 닿았을때 호출하는 메서드
    // 완전 종료 필요할 경우에 게임 매니저 호출.
    public void OnTouchFinishLine(Collider kart, GameObject line)
    {
        Debug.Log("OnTouchFinishLine");
        
        if (myCurrentLap > 0 && !IsEssentialCheckPointsPassed())
        {
            Debug.Log("필수 체크 포인트 안지나갔음");
            return;
        }
        
        if(myCurrentLap < totalLap)
        {
            myCurrentLap++;
            myLastcheckPoint = finishLine;
            ResetAllCheckPoints();
        }
        else
        {
            gameManager.OnFinished();
        }
    }

    /* 체크 포인트 닿았을때 호출하는 메서드 */
    public void OnTouchCheckPoint(Collider kart, GameObject checkPoint)
    {
        Debug.Log("OnTouchCheckPoint, name : " + checkPoint.name);
        myLastcheckPoint = checkPoint.transform.parent;
    }

    // 맵에서 부스터 닿았을떄 메서드
    public void OnTouchBooster(Collider kart, GameObject booster)
    {
        
    }

    // 그냥 마지막 체크포인트로 돌리고 싶을 때
    public void RespawnToPos(GameObject playerKart)
    {
        RespawnToPos(playerKart, null);
    }
    
    // 리스폰시 실행 (카트의 R키 누를때 혹은 바닥으로 떨어졌을떄)
    public void RespawnToPos(GameObject playerKart, Transform somePoint)
    {
        Rigidbody kartRigid = playerKart.GetComponent<TempCarScript>().Rigidbody;

        Vector3 penalty = Vector3.up * 2.5f;
        
        // Rigidbody를 붙인채로 transform을 변경하면, 속도가 실제로 변한다.
        // isKinematic을 켰다 꺼주자 (운동 효과 제거)
        kartRigid.isKinematic = true;

        // 특정 리스폰 포인트가 있다면, 그 포인트로 이동, 로테이션은 어떤 기준으로 할지 설정 필요
        if (somePoint != null)
        {
            playerKart.transform.position = somePoint.position + penalty;
            playerKart.transform.rotation = defaultRotation;
        }
        else
        {
            playerKart.transform.position = myLastcheckPoint.position + penalty;
            playerKart.transform.rotation = myLastcheckPoint.rotation;
        }
        
        kartRigid.isKinematic = false;        
    }

    /* ToDo: 플레이어 카트의 리지드 바디 찾아서 설정해주기 */ 
    public void PlaceToStartPos(int randomNum, GameObject playerKart)
    {
        Rigidbody kartRigid = playerKart.GetComponent<TempCarScript>().Rigidbody;
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
}
