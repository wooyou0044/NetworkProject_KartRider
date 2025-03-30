using System;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(DollyRealKart), typeof(TestCHMKart), typeof(PhotonView))]
public class RankManager : MonoBehaviour, IPunObservable
{
    [Serializable]
    public struct RankData
    {
        public int bfRank;
        public int rank;

        public int lap;
        public float totalPos;

        public bool isFinish;
    }
    
    public RankData kartRankData;

    /* 랭크 데이터 위해 필요한 카트 설정 */
    private TestCHMKart _kart;
    private DollyRealKart _dollyRealKart;
    private PhotonView _pv;
    
    private float _cachedDollyPos;
    
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
    }
    
    public float GetTotalPos()
    {
        return kartRankData.totalPos;
    }
    
    // 현재 위치값을 가지고 랩 수와 더해 실제 지나온 위치를 계산한다
    public void SetDollyPos(float dollyPos)
    {
        // 널 포인터 방지용 방어코드
        if (_dollyRealKart.DollyPath == null)
        {
            return;
        }
        
        float pastLength = (GetLap() - 1) * _dollyRealKart.DollyPath.PathLength;
        float calculatedPos = pastLength + dollyPos;
        kartRankData.totalPos = calculatedPos;
    }
    
    public bool GetFinish()
    {
        return kartRankData.isFinish;
    }
    
    public void SetFinish(bool isFinish)
    {
        kartRankData.isFinish = isFinish;
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

    private void FixedUpdate()
    {
        // 널 포인터 방어코드
        if (_dollyRealKart.DollyPath == null)
        {
            return;
        }
        
        _cachedDollyPos = _dollyRealKart.CalculateTrackPosition();
        SetDollyPos(_cachedDollyPos);
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) 
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_cachedDollyPos);
            // Debug.Log("Send : " + _cachedDollyPos);
        }
        else
        {
            // 무시할 예외 조건들 설정
            if (!_pv.Owner.Equals(info.photonView.Owner))
            {
                return;
            }

            if (stream.Count == 0)
            {
                return;
            }
                
            float otherDollyPos = (float)stream.ReceiveNext();
            SetDollyPos(otherDollyPos);
                
            // Debug.Log("Receive from : " + info + "pos : " + otherDollyPos);
        }
    }
}
