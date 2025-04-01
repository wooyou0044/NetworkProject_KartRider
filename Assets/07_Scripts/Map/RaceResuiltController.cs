using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class RaceResultController : MonoBehaviour
{
    public GameObject raceResultPrefab;
    public GameObject playersObject;
    public GridLayoutGroup gridRoot;
    
    private GameManager _gameManager;
    private MapManager _mapManager;
    private RankUIController _rankUIController;

    private float _totalLength;
    
    public void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _mapManager = FindObjectOfType<MapManager>();
        _rankUIController = _gameManager.rankUIController;
    }

    public void OnEnable()
    {
        InitRankUI();
        _totalLength = _mapManager.totalLap * _mapManager.dollyPath.PathLength;
    }

    // 전체 유저의 랭크 UI 그려주기
    public void InitRankUI()
    {
        Transform playersTr = playersObject.transform;
        
        for (int i = 0; i < playersTr.childCount; i++)
        {;
            GameObject kart = _rankUIController.GetKartObjectByRank(i+1);
            RankManager rankManager = kart.GetComponent<RankManager>();

            string timeOrPosStr = "";
            
            if (rankManager.IsFinish())
            {
                timeOrPosStr = _gameManager.finishedPlayerTime[kart.GetPhotonView().Owner].ToString();                
            }
            else
            {
                timeOrPosStr = (_totalLength - rankManager.GetTotalPos()).ToString();
            }
            
            GameObject rankElement = Instantiate(raceResultPrefab, gridRoot.transform);
            SetPlayerRankUI(rankElement, kart, timeOrPosStr);
        }

        StartCoroutine(RankUpdate());
    }

    private void SetPlayerRankUI(GameObject rankElement, GameObject kart, string timeOrPosStr)
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
        rankUIComponent.timeOrPosText.text = timeOrPosStr;
    }
    
    // 리타이어 될때까지 랭크 갱신
    private IEnumerator RankUpdate()
    {
        while (_gameManager.retireCountDownSeconds > 0)
        {
            yield return new WaitForSeconds(0.5f);            
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
}
