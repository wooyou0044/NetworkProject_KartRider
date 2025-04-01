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
    /// <summary>
    /// 씬이 변경되어도 파괴되지 않고, 어느 씬에서도 사용하기 위해 싱글톤 구현
    /// </summary>
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
