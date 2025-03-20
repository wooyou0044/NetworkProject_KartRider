using System.Collections;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] private int totalLap = 3; 
    
    [Header("맵 시작 / 종료 포지션 설정")]
    public Transform[] startPos;
    public GameObject endLine;
    public GameObject deadZone;
    
    public void Start()
    {
        deadZone.GetComponent<Collider>();
    }

    // 바닥으로 떨어졌을때
    public void OnTouchDeadZone()
    {
        
    }

    // 결승선 닿았을때 호출하는 메서드
    public void OnTouchFinishLine(GameObject kart)
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
        playerKart.transform.rotation = startingPoint.rotation;
        kartRigid.isKinematic = false;
    }
}
