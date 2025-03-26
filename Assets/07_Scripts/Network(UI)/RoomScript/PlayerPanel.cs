using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class PlayerPanel : MonoBehaviour
{
    [SerializeField] public Image playerImg;
    [SerializeField] public TMP_Text PlayerNameText;
    [SerializeField] public Image playerIcon;
    public PhotonView pv;

    private void Start()
    {
        pv = GetComponent<PhotonView>();        
    }
    public void UpdatePanel(string playerName)
    {
        PlayerNameText.text = playerName;        
    }
    
}
