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
    
    public void OnTouchFinishLine(GameObject kart)
    {
        
    }

    public void InitPlayer(int randomNum, Transform playerKartTr)
    {
        Debug.Log(startPos[randomNum].name);
        Debug.Log(startPos[randomNum].transform.position);
        Debug.Log(startPos[randomNum].transform.localPosition);
        Debug.Log("InitPlayer");
        Vector3 startingPoint = startPos[randomNum].position;
        playerKartTr.position = startingPoint;
    }
}
