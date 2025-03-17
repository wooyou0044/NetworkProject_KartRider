using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCont : MonoBehaviour
{
    private static SceneCont instance;    
    private AsyncOperation oper;
    public static SceneCont Instance { get => instance; }
    public AsyncOperation Oper { get => oper; set => oper = value; }

    private void Awake()
    {        
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
   
    public AsyncOperation SceneAsync(string sceneName)
    {
        return SceneManager.LoadSceneAsync(sceneName);
    }
    
}
