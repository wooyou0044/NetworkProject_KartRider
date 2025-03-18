using System.Collections;
using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    private Transform _playerParent;
    public MapManager mapManager;
    
    // To-Do 실제 네트워크 연결하면 네트워크 상 정보로 바꿀 것
    [Header("생성할 카트 & 캐릭터 프리팹 지정")] 
    public GameObject kartPrefab;
    public GameObject characterPrefab;

    private GameObject _localPlayerObject;

    private void Start()
    {
        _playerParent = GameObject.Find("Players").transform;
        
        // To-Do 실제 네트워크 연결하면 네트워크 상 정보로 바꿀 것
        DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
        if (pool != null)
        {
            pool.ResourceCache.Add(kartPrefab.name, kartPrefab);
            pool.ResourceCache.Add(characterPrefab.name, characterPrefab);
        }
    }
    
    // To-Do 실제 네트워크 연결시 Intstantiate 어떻게 설정할지 체크 필요
    public override void OnJoinedRoom()
    {
        Debug.Log("방 참여 성공");
        
        GameObject kart = PhotonNetwork.Instantiate(kartPrefab.name, Vector3.zero, Quaternion.identity);
        Transform characterPos = kart.transform.GetChild(0).GetChild(0).GetChild(0);
        
        GameObject character = PhotonNetwork.Instantiate(characterPrefab.name, characterPos.position, characterPrefab.transform.rotation);
        character.transform.parent = characterPos;
        kart.transform.parent = _playerParent;
        _localPlayerObject = kart;

        StartCoroutine(PlaceToMap());
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("방 참여 실패, code : " + returnCode + " msg : " + message);
    }

    public IEnumerator PlaceToMap()
    {
        yield return new WaitUntil(() => _localPlayerObject != null);
        Debug.Log("PlaceToMap : " + _localPlayerObject.name);
        mapManager.InitPlayer(0, _localPlayerObject.transform);
    }
}
