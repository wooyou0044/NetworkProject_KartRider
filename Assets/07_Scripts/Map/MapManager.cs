using Photon.Pun;
using UnityEngine;

public class MapManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private int totalLap = 3; 
    
    [Header("맵 시작 / 종료 포지션 설정")]
    public Transform[] startPos;
    public Transform[] checkPoints;

    // 바닥으로 떨어졌을때
    public void OnTouchDeadZone(Collider otherKart)
    {
        Debug.Log("OnTouchDeadZone");
    }

    // 결승선 닿았을때 호출하는 메서드
    public void OnTouchFinishLine(Collider otherKart)
    {
        Debug.Log("OnTouchFinishLine");
    }

    // 맵에서 부스터 닿았을떄 메서드
    public void OnTouchBooster(Collider otherKart)
    {
        
    }

    public void RespawnToLastPos(GameObject playerKart)
    {
        
    }
    
    public void PlaceToStartPos(int randomNum, GameObject playerKart)
    {
        Transform startingPoint = startPos[randomNum];
        Rigidbody kartRigid = playerKart.GetComponent<TempCarScript>().Rigidbody;

        // Rigidbody를 붙인채로 transform을 변경하면, 속도가 실제로 변한다.
        // isKinematic을 켰다 꺼주자 (운동 효과 제거)
        kartRigid.isKinematic = true;
        playerKart.transform.position = startingPoint.position;
        playerKart.transform.rotation = Quaternion.Euler(0, -175, 0);
        kartRigid.isKinematic = false;
    }
}
