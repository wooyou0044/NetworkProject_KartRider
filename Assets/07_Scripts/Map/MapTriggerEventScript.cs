using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class MapTriggerEventScript : MonoBehaviour
{
    private Collider _collider;
    private string _tag;
 
    /* 있을수도 있고 없을수도 있는 컴포넌트들 */
    private CheckPoint _checkPoint;
    
    public UnityEvent<Collider, GameObject> collisionEvent;
    
    void Awake()
    {
        _collider = GetComponent<Collider>();
        _tag = gameObject.tag;

        _checkPoint = GetComponent<CheckPoint>();
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);

        // 일단 다른사람이 충돌했으면 무시
        if (!other.gameObject.GetPhotonView().IsMine)
        {
            return;
        }
        
        if(_checkPoint != null)
        {
            _checkPoint.OnEnterCheckPoint();
        }
        
        collisionEvent.Invoke(other, this.gameObject);
    }
}
