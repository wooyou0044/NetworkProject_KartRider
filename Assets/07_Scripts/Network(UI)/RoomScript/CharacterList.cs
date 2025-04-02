using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CharacterList : MonoBehaviour
{
    public List<CharacterManager> characterManager;
    public RawImage characterImage;

    private int currentIndex = 0;
    private void Start()
    {
        characterManager = new List<CharacterManager>();
        SetCharacterResources();
    }
    public void SetCharacterResources()
    {
        List<CharacterSo> characters = Resources.LoadAll<CharacterSo>("Character").ToList();

        for (int i = 0; i < characters.Count; i++)
        {
            var createCharacter = Instantiate(characters[i].characterPrefab, Vector3.zero, Quaternion.Euler(-90, -90, 0));
            characterManager.Add(createCharacter.GetComponent<CharacterManager>());
            createCharacter.gameObject.SetActive(false);
        }
        characterManager[3].transform.rotation = Quaternion.Euler(0, 0, 0);
        characterManager[currentIndex].gameObject.SetActive(true);
    }
    public void CharacterChangeNextBtn()
    {

        characterManager[currentIndex].gameObject.SetActive(false);
        currentIndex = (currentIndex + 1) % characterManager.Count;
        characterManager[currentIndex].gameObject.SetActive(true);

    }
    public void PreviousCharacterBtn()
    {

        characterManager[currentIndex].gameObject.SetActive(false);
        currentIndex = (currentIndex - 1 + characterManager.Count) % characterManager.Count; // Ã¹ ¹øÂ°
        characterManager[currentIndex].gameObject.SetActive(true);

    }
}
