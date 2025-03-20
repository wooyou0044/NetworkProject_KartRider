using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MapTriggerEventScript : MonoBehaviour
{
    private Collider _collider;
    private string _tag;
    
    public MapManager mapManager;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        _tag = gameObject.tag;        
    }

    public void OnTriggerEnter(Collider other)
    {
        switch(_tag)
        {
            case "Deadzone" : return;
            default : 
                Debug.LogError("아무것도 지정되지 않은 이벤트"); 
                break;
        }   
    }
}
