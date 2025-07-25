using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCont : MonoBehaviour
{
    private static SceneCont instance;
    private static CharacterSo _characterSo;

    private AsyncOperation oper;

    public static SceneCont Instance
    {
        get => instance;
    }

    public AsyncOperation Oper
    {
        get => oper;
        set => oper = value;
    }

    public CharacterSo SelectedCharacter
    {
        get => _characterSo;
        set => _characterSo = value;
    }

    /// <summary>
    /// ���� ����Ǿ �ı����� �ʰ�, ��� �������� ����ϱ� ���� �̱��� ����
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //�� ���� �� �ε��� ���� AsyncOperation ���Ϻ���
    public AsyncOperation SceneAsync(string sceneName)
    {
        SoundManager.instance.PlayBGM(SceneManager.GetActiveScene().buildIndex + 1);
        return SceneManager.LoadSceneAsync(sceneName);
    }
}
