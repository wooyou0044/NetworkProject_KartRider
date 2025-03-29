using System;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(DollyRealKart), typeof(TestCHMKart), typeof(PhotonView))]
public class RankManager : MonoBehaviour
{
    [Serializable]
    public struct RankData
    {
        public int bfRank;
        public int rank;

        public int lap;
        public float totalPos;
    }
    
    public RankData kartRankData;

    /* 랭크 데이터 위해 필요한 카트 설정 */
    private TestCHMKart _kart;
    private DollyRealKart _dollyRealKart;
    private PhotonView _pv;    
    
    public int GetLap()
    {
        return kartRankData.lap;
    }

    [PunRPC]
    public void SetLap(int lap, PhotonMessageInfo info)
    {
        // 다른 플레이어 클라이언트에서 달라고 있는 나의 카트들에게도 랩을 적용해달라
        if (info.photonView.Owner.Equals( _pv.Owner))
        {
            kartRankData.lap = lap;            
        }
    }    
    
    public int GetRank()
    {
        return kartRankData.rank;
    }

    public void SetRank(int rank)
    {
        kartRankData.bfRank = kartRankData.rank;
        kartRankData.rank = rank;
        
        if(kartRankData.bfRank != kartRankData.rank)
        {
            OnRankChanged();
        }
    }
    
    public float GetTotalPos()
    {
        return kartRankData.totalPos;
    }
    
    // 현재 위치값을 가지고 랩 수와 더해 실제 지나온 위치를 계산한다
    public void SetDollyPos(float dollyPos)
    {
        float pastLength = (GetLap() - 1) * _dollyRealKart.DollyPath.PathLength;
        float calculatedPos = pastLength + dollyPos;
        kartRankData.totalPos = calculatedPos;
    }

    private void Awake()
    {
        _dollyRealKart = GetComponent<DollyRealKart>();
        _kart = GetComponent<TestCHMKart>();
        _pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        kartRankData.rank = 1;
        kartRankData.bfRank = 1;
    }
    
    public void OnRankChanged()
    {
        Debug.Log("랭킹 바뀜");
    }

    public void FixedUpdate()
    {
        // 널포인터 나는 케이스 있어서 방어처리
        if(_dollyRealKart.DollyPath == null)
        {
            return;
        }
        
        SetDollyPos(_dollyRealKart.CalculateTrackPosition());
        Debug.Log(GetTotalPos());
    }
}
