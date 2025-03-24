using UnityEngine;
using UnityEngine.UI;

public class LapUIController : MonoBehaviour
{
    public MapManager mapManager;

    public Text currentLap;
    public Text totalLap;

    private void OnEnable()
    {
        mapManager.onFinishEvent.AddListener(SetCurrentLap);
    }

    private void OnDisable()
    {
        mapManager.onFinishEvent.RemoveListener(SetCurrentLap);
    }

    private void Start()
    {
        SetTotalLap();
    }

    public void SetTotalLap()
    {
        string formatText = " / {0} LAPS";
        totalLap.text = string.Format(formatText, mapManager.totalLap);
    }
    
    public void SetCurrentLap()
    {
        currentLap.text = mapManager.MyCurrentLap.ToString();
    }
}
