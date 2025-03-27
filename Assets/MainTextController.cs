using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainTextController : MonoBehaviour
{
    public Text mainText;
    [Range(0.1f, 0.9f)]
    public float fadeOutTime = 0.5f;

    public void SetColor(Color color)
    {
        mainText.color = color;
    }
    
    public void ShowMainText(string text)
    {
        mainText.text = text;
        mainText.gameObject.SetActive(true);
    }
    
    public void HideMainText()
    {
        mainText.text = String.Empty;
        mainText.gameObject.SetActive(false);
    }

    /* 1초 동안 텍스트 FadeOut 하며 표시해주기 */
    public IEnumerator ShowTextOneSecond(string text)
    {
        ShowMainText(text);
        StartCoroutine(TextToTransparent());
        yield return new WaitForSeconds(1f);
        HideMainText();
    }

    IEnumerator TextToTransparent()
    {
        float fadeToMinus = 1f / (fadeOutTime / 0.05f);
        float colorAlpha = mainText.color.a;
        
        if (colorAlpha < 1)
        {
            mainText.color = new Color(mainText.color.r, mainText.color.g, mainText.color.b, 1);
        }
        
        for (float f = fadeOutTime; f > 0; f -= 0.05f)
        {
            Color calculatedColor = mainText.color - new Color(0, 0, 0, fadeToMinus);
            mainText.color = calculatedColor;
            yield return new WaitForSeconds(0.05f);
        }
    }
}
