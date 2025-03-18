using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("맵 시작 / 종료 포지션 설정")]
    public Transform[] startPos;
    public GameObject endLine;
    
    public void Start()
    {
        
    }
    
    
    // 결승선 닿았을때 호출하는 메서드
    public void OnTouchFinishLine(GameObject kart)
    {
        
    }

    public void InitPlayer(int randomNum, Transform playerKartTr)
    {
        Transform startingPoint = startPos[randomNum];
        playerKartTr.position = startingPoint.position;
        playerKartTr.rotation = startingPoint.rotation;
        playerKartTr.Rotate(Vector3.up, 180);
    }
}
