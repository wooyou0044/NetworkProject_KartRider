using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class RankUIController : MonoBehaviour
{
    public GameObject rankElementPrefab;
    public GameObject playersObject;
    public GridLayoutGroup gridRoot;

    private GameManager _gameManager;
    private List<GameObject> _kartList;
    // 현재 카트와 랭크 UI를 매칭시켜주는 딕셔너리
    private Dictionary<GameObject, RankUIComponent> _kartDict;
    
    public void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }
    
    public void SetPlayers()
    {
        _kartList = new List<GameObject>();
        foreach (Transform child in playersObject.transform)
        {
            _kartList.Add(child.gameObject);
        }
    }

    // 전체 유저의 랭크 UI 그려주기
    public void InitRankUI()
    {
        SetPlayers();
        
        _kartDict = new Dictionary<GameObject, RankUIComponent>();        
        
        // 방에 플레이어 들어오는 갱신있을시 예외처리
        if (playersObject.transform.childCount > 0)
        {
            DestroyAllRankUIChildren();
            StopAllCoroutines();
        }

        foreach (var kart in _kartList)
        {
            GameObject rankElement = Instantiate(rankElementPrefab, gridRoot.transform);
            SetPlayerRankUI(rankElement, kart);
        }

        StartCoroutine(RankUpdate());
    }

    // 예외적으로 랭크 UI를 갱신할때 다 지워주고 다시 그리도록 해주는 옵션
    public void DestroyAllRankUIChildren()
    {
        foreach (Transform child in gridRoot.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void SetPlayerRankUI(GameObject rankElement, GameObject kart)
    {
        RankUIComponent rankUIComponent = rankElement.GetComponent<RankUIComponent>();
        RankManager manager = kart.GetComponent<RankManager>();
        rankUIComponent.RankManager = manager;

        PhotonView pv = kart.GetPhotonView();
        if (pv.IsMine)
        {
            CharacterSo characterSo = _gameManager.characterSo;
            rankUIComponent.playerIndicator.gameObject.SetActive(true);
            rankUIComponent.playerIndicator.color = characterSo.mainColor;
            rankUIComponent.rankTextBg.color = characterSo.mainColor;
            rankUIComponent.namePlateBg.color = characterSo.subColor;
            rankUIComponent.Icon.sprite = characterSo.characterIcon;
        }
        else
        {
            rankUIComponent.Icon.gameObject.SetActive(false);
            rankUIComponent.playerIndicator.gameObject.SetActive(false);
        }

        rankUIComponent.rankText.text = manager.GetRank().ToString();
        rankUIComponent.namePlate.text = pv.Owner.NickName;
        
        // GetComponent 안하기 위한 딕셔너리 추가
        _kartDict.Add(kart, rankUIComponent);
    }
    
    private IEnumerator RankUpdate()
    {
        while (_gameManager.retireCountDownSeconds > 0)
        {
            MoveRankByDistance();
            yield return new WaitForSeconds(0.5f);            
        }
    }

    private void MoveRankByDistance()
    {
        List<GameObject> sortedKartList = new List<GameObject>(_kartList);
        sortedKartList.Sort((kart1, kart2) =>
        {
            float kartPos1 = _kartDict[kart1].RankManager.GetTotalPos();
            float kartPos2 = _kartDict[kart2].RankManager.GetTotalPos();
            return kartPos2.CompareTo(kartPos1);
        });

        int rank = 0;
        foreach (var kart in sortedKartList)
        {
            rank++;
            _kartDict[kart].RankManager.SetRank(rank);
            _kartDict[kart].rankText.text = rank.ToString();
        }
    }
}
