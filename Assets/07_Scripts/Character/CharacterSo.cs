using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CharacterData", menuName= "Scriptable Object/Kart Character Data")]
public class CharacterSo : ScriptableObject
{
    [Header("캐릭터 프리팹")]
    public GameObject characterPrefab;

    [Header("캐릭터 아이콘")]
    public Sprite characterIcon;
    
    [Header("캐릭터 메인 / 서브 컬러")]
    public Color mainColor;
    public Color subColor;
    
    [Header("캐릭터 영어 / 한글 이름")]
    public string characterName;
    public string characterKrName;
}
