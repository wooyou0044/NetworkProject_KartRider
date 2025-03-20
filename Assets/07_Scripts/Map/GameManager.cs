using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    private Transform _playerParent;
    public MapManager mapManager;
    
    // ToDo 실제 네트워크 연결하면 네트워크 상 정보로 바꿀 것
    [Header("생성할 카트 & 캐릭터 프리팹 지정")] 
    public GameObject kartPrefab;
    public GameObject characterPrefab;

    private void Start()
    {
        // ToDo 실제 네트워크 연결하면 네트워크 상 정보로 바꿀 것
        DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
        if (pool != null)
        {
            pool.ResourceCache.Add(kartPrefab.name, kartPrefab);
            pool.ResourceCache.Add(characterPrefab.name, characterPrefab);
        }
    }
    
    // ToDo 실제 네트워크 연결시 Intstantiate 어떻게 설정할지 체크 필요
    public override void OnJoinedRoom()
    {
        GameObject kart = PhotonNetwork.Instantiate(kartPrefab.name, Vector3.zero, Quaternion.identity);
        PhotonNetwork.Instantiate(characterPrefab.name, Vector3.zero, characterPrefab.transform.rotation);
        StartCoroutine(PlaceToMap(kart));
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("방 참여 실패, code : " + returnCode + " msg : " + message);
    }

    IEnumerator PlaceToMap(GameObject kart)
    {
        yield return new WaitUntil(() => kart);

        // ToDo : 랜덤으로 설정해줄거면 actorNumber 대신 다른걸로 [0~7 숫자]
        Player kartOwner = kart.GetPhotonView().Owner;
        int num = kartOwner.ActorNumber - 1;
        
        mapManager.PlaceToStartPos(num, kart);
    }
}
