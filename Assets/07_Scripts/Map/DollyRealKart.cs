using Cinemachine;
using UnityEngine;

public class DollyRealKart : MonoBehaviour
{
    private float _trackPosition;
    private float _trackPosToLength;
    private CinemachineSmoothPath _dollyPath;
    
    public Transform trackingObject;

    public CinemachineSmoothPath DollyPath
    {
        get => _dollyPath;
        set => _dollyPath = value;
    }

    public float GetTrackPosToLength()
    {
        return _trackPosToLength;
    }
    
    public void CalculateTrackPosition()
    {
        _trackPosition = _dollyPath.FindClosestPoint(trackingObject.position,0, -1, 1);
        _trackPosToLength = (_trackPosition / _dollyPath.m_Waypoints.Length) * _dollyPath.PathLength;
    }

    private void FixedUpdate()
    {
        if (_dollyPath == null)
        {
            return;
        }

        CalculateTrackPosition();
    }
}
