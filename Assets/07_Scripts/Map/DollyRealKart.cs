using System;
using Cinemachine;
using UnityEngine;

public class DollyRealKart : MonoBehaviour
{
    private float _trackPosition;
    private float _trackPosToLength;
    private CinemachineSmoothPath _dollyPath;
    
    public Transform trackingObject;

    public void Awake()
    {
        _dollyPath = FindObjectOfType<CinemachineSmoothPath>();
    }

    public CinemachineSmoothPath DollyPath
    {
        get => _dollyPath;
    }

    public float GetTrackPosToLength()
    {
        return _trackPosToLength;
    }
    
    public float CalculateTrackPosition()
    {
        if (DollyPath == null)
        {
            Debug.LogError("Dolly Path is not set");
            return 0f;
        }
        
        _trackPosition = _dollyPath.FindClosestPoint(trackingObject.position,0, -1, 1);
        _trackPosToLength = (_trackPosition / _dollyPath.m_Waypoints.Length) * _dollyPath.PathLength;
        return _trackPosToLength;
    }
}
