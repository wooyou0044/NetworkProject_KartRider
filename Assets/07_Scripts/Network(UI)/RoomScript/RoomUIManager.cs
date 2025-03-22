using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomUIManager : MonoBehaviour
{
    [Header("준비 버튼")]
    [SerializeField] public Button startBtn;
    [SerializeField] public Button readyBtn;
    [SerializeField] public Button readyCanCelBtn;

    [Header("맵(트랙변경) 버튼")]
    [SerializeField] public Button MapChangeBtn;

    [Header("방 타이틀 변경 버튼")]
    [SerializeField] public Button roomTitleChangeBtn;

    [Header("방 나가기 버튼")]
    [SerializeField] public Button exitBtn;



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
