using Cinemachine;
using UnityEngine;

public class DollyRealKart : MonoBehaviour
{
    private float _trackPosition;

    public CinemachinePathBase dollyTrack;
    public Transform toFollow;

    private void Start()
    {
        if (dollyTrack == null)
        {
            Debug.LogError("돌리 카트 에러 발생!!!");
        }
    }

    public float GetTrackPosition()
    {
        return _trackPosition;
    }
    
    public void CalculateTrackPosition()
    {
        _trackPosition = dollyTrack.FindClosestPoint(toFollow.position,0, -1, 1);
    }

    private void FixedUpdate()
    {
        CalculateTrackPosition();
        Debug.Log(GetTrackPosition());
    }
}
