using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

/* RankUI를 편하게 조정할수 있도록 담아두는 스크립트 */
public class RankUIComponent : MonoBehaviour
{
    public RectTransform rectTransform;
    
    [Header("나인지 화살표 표시")]
    public Image playerIndicator;

    [Header("숫자 랭크 표시")]
    public Image rankTextBg;
    public Text rankText;

    [Header("플레이어 이름 표시")] 
    public Image namePlateBg;
    public Text namePlate;

    [Header("캐릭터 아이콘 표시")]
    public Image Icon;

    private RankManager _rankManager;

    public RankManager RankManager
    {
        get => _rankManager;
        set => _rankManager = value;
    }
}
