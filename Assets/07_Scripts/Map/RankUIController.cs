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

    // 순차 정렬된 카트리스트 저장용
    private List<GameObject> _sortedKartList;
    
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
        _sortedKartList = new List<GameObject>(_kartList);
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
            // 랭크 나타내는 UI 객체에 Player의 TagObject를 담아둔다.
            kart.gameObject.GetPhotonView().Owner.TagObject = rankElement;
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
    
    // 리타이어 될때까지 랭크 갱신
    private IEnumerator RankUpdate()
    {
        while (_gameManager.retireCountDownSeconds > 0)
        {
            SortRankByDistance();
            yield return new WaitForSeconds(0.5f);            
        }
    }

    // 간 거리에 따라 랭크 정렬하기
    private void SortRankByDistance()
    {
        _sortedKartList.Sort((kart1, kart2) =>
        {
            float kartPos1 = _kartDict[kart1].RankManager.GetTotalPos();
            float kartPos2 = _kartDict[kart2].RankManager.GetTotalPos();
            return kartPos2.CompareTo(kartPos1);
        });

        int rank = 0;
        foreach (var kart in _sortedKartList)
        {
            rank++;
            _kartDict[kart].RankManager.SetRank(rank);
            _kartDict[kart].rankText.text = rank.ToString();

            int bfRank = _kartDict[kart].RankManager.GetBfRank();
            int currentRank = _kartDict[kart].RankManager.GetRank();
            // 랭크가 변동될때만 UI 갱신
            if (bfRank != currentRank)
            {
                StartCoroutine(UpdateRankUI(_kartDict[kart], bfRank, currentRank));                
            }
        }
    }

    // 랭크 UI 부드럽게 이동
    private IEnumerator UpdateRankUI(RankUIComponent kartRank, int bfRank, int rank)
    {
        RectTransform rankElementTransform = kartRank.rectTransform;

        float duration = 0.25f;
        float elapsedTime = 0f;

        Vector2 currentPos = GetRankElementPos(bfRank);
        Vector2 toChangePos = GetRankElementPos(rank);        
        
        // Debug.Log("Player : " + kartRank.namePlate.text + "Rank :" + bfRank + " -> " + rank);
        
        while (elapsedTime < duration)
        {
            rankElementTransform.anchoredPosition = Vector2.Lerp(currentPos, toChangePos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        
        rankElementTransform.anchoredPosition = toChangePos;
    }
    
    private Vector2 GetRankElementPos(int rank)
    {
        Vector2 cellSize = gridRoot.cellSize;
        Vector2 spacing = gridRoot.spacing;        
        
        float fixedY = -cellSize.y / 2;
        float cellSizeY = cellSize.y * rank;
        float spacingY = spacing.y * (rank - 1);
        float changedPosY = -(fixedY + cellSizeY + spacingY);

        return new Vector2(cellSize.x / 2, changedPosY);
    }
    
    // Players 하위에 있는 GameObject에서, 해당 순위를 찾아서 반환해준다.
    public GameObject GetKartObjectByRank(int rank)
    {
        return _sortedKartList[rank - 1];
    }
}
