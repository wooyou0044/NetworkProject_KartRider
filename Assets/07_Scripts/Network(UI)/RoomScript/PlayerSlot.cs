using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSlot : MonoBehaviour
{
    [SerializeField] public PlayerPanel playerPanel;
    public bool IsEmpty => playerPanel == null; // playerPanel이 null이면 true 반환

    public void CreatePlayerPanel()
    {
        if (playerPanel == null) // 현재 슬롯이 비어있다면
        {
            var instancePanel = PhotonNetwork.Instantiate("PlayerPanelPrefab", transform.position, Quaternion.identity);
            instancePanel.transform.SetParent(gameObject.transform);
            playerPanel = instancePanel.GetComponent<PlayerPanel>();
        }
        //return playerPanel; // 생성된 패널 반환
    }

    public void ClearPlayerPanel()
    {
        if (playerPanel != null)
        {
            Destroy(playerPanel.gameObject); // 패널 제거
            playerPanel = null; // 참조 해제
        }
    }
    public void AssignPlayer(int actorNumber, string playerName)
    {
        if (playerPanel == null) CreatePlayerPanel(); // 패널이 없으면 생성
        playerPanel.UpdatePanel(playerName); // 패널에 닉네임 업데이트
    }

}
