using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

public class FirebaseDBManager : MonoBehaviour
{
    private static FirebaseDBManager instance;
    [SerializeField] private FirebaseUser user;
    [SerializeField] private FirebaseAuth auth;
    [SerializeField] private DatabaseReference dbRef;
    public string CurrentSessionId { get; set; } // ���� ���� ID ����

    public static FirebaseDBManager Instance { get => instance;}
    public FirebaseUser User { get => user; set => user = value; }
    public FirebaseAuth Auth { get => auth; set => auth = value; }
    public DatabaseReference DbRef { get => dbRef; set => dbRef = value; }

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

    public void OnApplicationQuit()
    {        
        //�α׾ƿ� �����Ǹ� ���̾�̽� �α��� ���� false�� ��ȯ
        DbRef.Child("users").Child(FirebaseDBManager.Instance.User.UserId)
            .Child("isLoggedIn").SetValueAsync(false);
        auth.SignOut();
    }
}
