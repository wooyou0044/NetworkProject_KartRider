using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class CheckPoint : MonoBehaviour
{
    /* 디버그용 Serialized 필드 */
    private bool _isPassed = false;

    public bool IsPassed => _isPassed;

    public void ResetFlag()
    {
        _isPassed = false;
    }

    public void OnEnterCheckPoint()
    {
        _isPassed = true;
    }
}
