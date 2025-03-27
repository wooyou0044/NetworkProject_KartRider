using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    public Camera miniMapCamera;
    public Transform toFollow;

    [Range(100f, 250f)] public float upPosition;
    
    public void SetFollower(Transform someKart)
    {
        toFollow = someKart;
        SetCameraPos(toFollow);
    }

    private void SetCameraPos(Transform syncTarget)
    {
        miniMapCamera.gameObject.SetActive(true);
        SyncPos(syncTarget);
    }

    private void SyncPos(Transform syncTarget)
    {
        Vector3 upPos = new Vector3(0, upPosition, 0);
        miniMapCamera.transform.position = syncTarget.transform.position + upPos;
   }

    // Update is called once per frame
    void LateUpdate()
    {
        if (toFollow != null)
        {
            SyncPos(toFollow);            
        }
    }
}
