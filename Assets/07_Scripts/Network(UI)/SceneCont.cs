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
   
    //씬 변경 시 로딩을 위한 AsyncOperation 리턴변수
    public AsyncOperation SceneAsync(string sceneName)
    {
        return SceneManager.LoadSceneAsync(sceneName);
    }
    
}
