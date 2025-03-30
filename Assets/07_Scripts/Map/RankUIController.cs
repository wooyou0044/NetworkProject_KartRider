using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class RankUIController : MonoBehaviour
{
    public GameObject rankElementPrefab;
    public GameObject playersObject;
    public GridLayoutGroup gridRoot;
    
    private List<GameObject> _kartGameObjects;

    public void SetPlayers()
    {
        _kartGameObjects = new List<GameObject>();
        foreach (Transform child in playersObject.transform)
        {
            _kartGameObjects.Add(child.gameObject);
        }
    }

    public void InitRankUI(GameManager gameManager)
    {
        // 방 갱신있을시 예외처리
        if (playersObject.transform.childCount > 0)
        {
            DestroyAllRankUIChildren();
        }
        
        SetPlayers();
        foreach (var kart in _kartGameObjects)
        {
            GameObject rankElement = Instantiate(rankElementPrefab, gridRoot.transform);

            RankUIComponent rankUIComponent = rankElement.GetComponent<RankUIComponent>();
            RankManager manager = kart.GetComponent<RankManager>();
            PhotonView pv = kart.GetPhotonView();

            if (pv.IsMine)
            {
                CharacterSo characterSo = gameManager.characterSo;
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
        }
    }

    // 예외적으로 랭크 UI를 갱신할때 다 지워주고 다시 그리도록 해주는 옵션
    public void DestroyAllRankUIChildren()
    {
        foreach (Transform child in gridRoot.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
