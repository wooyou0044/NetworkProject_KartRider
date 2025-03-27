using System;
using System.Collections.Generic;
using System.Diagnostics;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class TimeUIController : MonoBehaviour
{
    private Stopwatch _stopWatch;
    private List<TimeSpan> _lapTimeList;

    public MapManager mapManager;
    public Text lapTime;
    public Text currentLapTime;
    
    private string _defaultTimeFormat = "mm\\:ss\\.fff";
    
    private void Awake()
    {
        _lapTimeList = new List<TimeSpan>();
        _stopWatch = new Stopwatch();
    }    
    private void OnEnable()
    {
        mapManager.onFinishEvent.AddListener(AddLapTime);
    }

    private void OnDisable()
    {
        mapManager.onFinishEvent.RemoveListener(AddLapTime);
    }    

    // ToDo : 시간 시작할때 네트워크로 받아서 시작하도록?
    [PunRPC]
    public void StartTimer()
    {
        _stopWatch.Start();
    }

    public void AddLapTime()
    {
        // 처음 피니시 라인 지나갔을때 예외처리
        if (mapManager.MyCurrentLap == 1)
        {
            return;
        }
        
        TimeSpan timeSpan = _stopWatch.Elapsed;
        TimeSpan calculatedTimeSpan = timeSpan;
        int lapCount = _lapTimeList.Count;

        if (lapCount > 0)
        {
            calculatedTimeSpan = timeSpan - _lapTimeList[lapCount - 1];
        }
        
        string lapCountText = (_lapTimeList.Count + 1).ToString();
        string format = "Lap {0} : {1}";
        string lastLapTime = calculatedTimeSpan.ToString(_defaultTimeFormat);
        string lapTimeText = string.Format(format, lapCountText, lastLapTime);
        
        // 기존 텍스트 계속 추가하는 식으로
        lapTime.text = lapTime.text + lapTimeText + "\n";
        _lapTimeList.Add(timeSpan);
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        currentLapTime.text = "Total : " + _stopWatch.Elapsed.ToString(_defaultTimeFormat);
    }
}
