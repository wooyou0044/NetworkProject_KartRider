using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RaceResultController : MonoBehaviour
{
    public GameObject raceResultPrefab;
    public GameObject playersObject;
    public GridLayoutGroup gridRoot;
    public Text backToRoomText;
    
    private GameManager _gameManager;
    private MapManager _mapManager;
    private RankUIController _rankUIController;

    private Dictionary<GameObject, RankUIComponent> _rankDict;

    private float _totalLength;
    
    // 실제 결승점 0,0,0 보다 트리거 영역이 넓어서 오차보정 값
    private float _collisionDiffrence = 8f;
    
    public void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _mapManager = FindObjectOfType<MapManager>();
        _rankUIController = _gameManager.rankUIController;

        _rankDict = new Dictionary<GameObject, RankUIComponent>();
    }

    public void OnEnable()
    {
        _totalLength = _mapManager.totalLap * _mapManager.dollyPath.PathLength - _collisionDiffrence;
        InitRankUI();
    }

    public void OnDisable()
    {
        foreach (Transform child in gridRoot.transform)
        {
            Destroy(child.gameObject);
        }
    }

    // 전체 유저의 랭크 UI 그려주기
    public void InitRankUI()
    {
        Transform playersTr = playersObject.transform;
        
        for (int i = 0; i < playersTr.childCount; i++)
        {;
            GameObject kart = _rankUIController.GetKartObjectByRank(i+1);
            GameObject rankElement = Instantiate(raceResultPrefab, gridRoot.transform);
            RankUIComponent rankUIComponent = rankElement.GetComponent<RankUIComponent>();
            SetPlayerRankUI(rankElement, kart);

            // Player와 rankElement를 연결해주는 딕셔너리에 저장
            if (!_rankDict.ContainsKey(kart))
            {
                _rankDict.Add(kart, rankUIComponent);
            }
        }

        for (int i = 0; i < playersTr.childCount; i++)
        {
            GameObject kart = playersTr.GetChild(i).GameObject();
            StartCoroutine(RankElementUpdate(_rankDict[kart]));            
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
            
        if (manager.IsFinish())
        {
            float finishedTime = _gameManager.finishedPlayerTime[kart.GetPhotonView().Owner];
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(finishedTime);
            string finishedTimeStr = string.Format("{0:00}:{1:00}:{2:000}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
            
            rankUIComponent.timeOrPosText.color = Color.white;
            rankUIComponent.timeOrPosText.text = finishedTimeStr;
        }
        else
        {
            string leftPos = " - " + Math.Floor(_totalLength - manager.GetTotalPos()) + " M";
            rankUIComponent.timeOrPosText.color = Color.red;
            rankUIComponent.timeOrPosText.text = leftPos;
        }

        rankUIComponent.RankManager = manager;
    }
    
    // 리타이어 될때까지 랭크 갱신
    private IEnumerator RankElementUpdate(RankUIComponent rankUIComponent)
    {
        while (_gameManager.retireCountDownSeconds > 0)
        {
            UpdateRankUI(rankUIComponent);
            yield return new WaitForFixedUpdate();
        }
    }

    // 랭크 UI 갱신
    private void UpdateRankUI(RankUIComponent rankUIComponent)
    {
        RankManager manager = rankUIComponent.RankManager;
        
        if (rankUIComponent.RankManager.IsFinish())
        {
            return;
        }
        
        string leftPos = " - " + Math.Floor(_totalLength - manager.GetTotalPos()) + " M";
        rankUIComponent.timeOrPosText.color = Color.red;
        rankUIComponent.timeOrPosText.text = leftPos;
    }

    public void UpdateFinished(Player player)
    {
        Transform playersTr = playersObject.transform;
        GameObject targetKart = null;
        
        for (int i = 0; i < playersTr.childCount; i++)
        {
            Player kartPlayer = playersTr.GetChild(i).gameObject.GetPhotonView().Owner;

            if (player.Equals(kartPlayer))
            {
                targetKart = playersTr.GetChild(i).gameObject;
                break;
            }
        }

        if (targetKart == null)
        {
            Debug.LogError("타겟이 없습니다~~~~ 에러 발생~~~~");
            return;
        }
        
        RankUIComponent rankUIComponent  = _rankDict[targetKart];
        RankManager manager = rankUIComponent.RankManager;
        
        float finishedTime = _gameManager.finishedPlayerTime[player];
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(finishedTime);
        string finishedTimeStr = string.Format("{0:00}:{1:00}:{2:000}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        
        rankUIComponent.timeOrPosText.color = Color.white;
        rankUIComponent.timeOrPosText.text = finishedTimeStr;
    }

    // 순위 변경시 이동
    // private IEnumerator MoveRankElement(RankUIComponent rankUIComponent)
    // {
    //     RectTransform rankElementTransform = rankUIComponent.rectTransform;
    //     
    //     float duration = 0.25f;
    //     float elapsedTime = 0f;
    //
    //     Vector2 currentPos = GetRankElementPos(bfRank);
    //     Vector2 toChangePos = GetRankElementPos(rank);        
    //     
    //     // Debug.Log("Player : " + rankUIComponent.namePlate.text + "Rank :" + bfRank + " -> " + rank);
    //     
    //     while (elapsedTime < duration)
    //     {
    //         rankElementTransform.anchoredPosition = Vector2.Lerp(currentPos, toChangePos, elapsedTime / duration);
    //         elapsedTime += Time.deltaTime;
    //         yield return new WaitForFixedUpdate();
    //     }
    //     
    //     rankElementTransform.anchoredPosition = toChangePos;        
    // }
    
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
}
