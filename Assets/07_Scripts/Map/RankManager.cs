using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(MapManager))]
public class RankManager : MonoBehaviourPunCallbacks
{
    private struct RankData
    {
        public int BfRank;
        public int Rank;
    
        public int Lap;
        public float TotalPos;
    }

    private RankData _myRankData;
    private MapManager _mapManager;

    public int GetRank()
    {
        return _myRankData.Rank;
    }

    public void SetRank(int rank)
    {
        _myRankData.BfRank = _myRankData.Rank;
        _myRankData.Rank = rank;
        
        if(_myRankData.BfRank != _myRankData.Rank)
        {
            OnRankChanged();
        }
    }
    
    public float GetTotalPos()
    {
        return _myRankData.TotalPos;
    }
    
    public void SetLap(int lap)
    {
        _myRankData.Lap = lap;
    }
    
    public void SetDollyPos(float dollyPos)
    {
        float calculatedPos = dollyPos * _myRankData.Lap;
        _myRankData.TotalPos = calculatedPos;
    }    

    private void Start()
    {
        _mapManager = GetComponent<MapManager>();
    }    
    
    public void OnRankChanged()
    {
        Debug.Log("랭킹 바뀜");
    }
}
