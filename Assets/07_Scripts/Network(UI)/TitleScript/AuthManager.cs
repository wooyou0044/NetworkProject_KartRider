using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using Firebase.Database;

public class AuthManager : MonoBehaviour
{
    public TitleUI titleUI;
    public ServerConnect serverCon;    

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependStatus = task.Result;
            if (dependStatus == DependencyStatus.Available)
            {
                FirebaseDBManager.Instance.Auth = FirebaseAuth.DefaultInstance;
                FirebaseDBManager.Instance.DbRef = FirebaseDatabase.DefaultInstance.RootReference;
            }
            else
            {
                Debug.LogError("파이어베이스 오류" + dependStatus + "사용 불가");
            }
        });
    }
    
    public void Login()
    {
        StartCoroutine(LoginCoroutine(titleUI.loginEmailField.text, titleUI.loginpasswordField.text));
        //serverCon
    }
    
    IEnumerator LoginCoroutine(string email, string password)
    {
        var loginTask = FirebaseDBManager.Instance.Auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);
        
        if(loginTask.Exception != null)
        {
            FirebaseException firebasEx = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebasEx.ErrorCode;
            string message = "";
            titleUI.errorMessage.gameObject.SetActive(true);
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "MissingEmail";
                    break;
                case AuthError.MissingPassword:
                    message = "MissingPassword";
                    break;
                default:
                    message = "Email or Password or UserNotFound";
                    break;
            }
            titleUI.errorMessage.text = message;
        }
        else
        {
            titleUI.errorMessage.gameObject.SetActive(false);
            FirebaseDBManager.Instance.User = loginTask.Result.User;
            titleUI.successMessage.gameObject.SetActive(true);
            titleUI.successMessage.text ="Login Successful!";
            titleUI.loginPanel.gameObject.SetActive(false);
            //서버 접속 시도 하면됨 서버 접속 시도하고 텍스트 메세지 끄기
        }
    }

    public void Signup()
    {
        StartCoroutine(SignUpCoroutine(titleUI.signUpEmailField.text, titleUI.signUpPasswordField.text));
    }
    IEnumerator SignUpCoroutine(string email, string password)
    {
        var signUpTask = FirebaseDBManager.Instance.Auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(predicate: () => signUpTask.IsCompleted);
        titleUI.loginPanel.gameObject.SetActive(false);
        titleUI.signUpPansl.gameObject.SetActive(true);
        if (signUpTask.Exception != null)
        {
            FirebaseException firebaseEx = signUpTask.Exception.GetBaseException() as FirebaseException;

            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "";            
            titleUI.errorMessage.gameObject.SetActive(true);
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "MissingEmail";
                    break;
                case AuthError.MissingPassword:
                    message = "MissingPassword";
                    break;
                case AuthError.WeakPassword:
                    message = "Kindly use at least 6 characters.";
                    break;
                case AuthError.EmailAlreadyInUse:
                    message = "This email is already in use.";
                    break;
                default:
                    message = "Please contact the administrator";
                    break;
            }
            titleUI.errorMessage.text = message;
        }
        else
        {
            FirebaseDBManager.Instance.User = signUpTask.Result.User;
            titleUI.signUpCompleteBtn.interactable = false;
            titleUI.signUpLoingToBtn.interactable = false;
            titleUI.successMessage.gameObject.SetActive(true);//메세지 키고
            titleUI.errorMessage.text = "Please Wait"; //메세지 바꾸고
            yield return new WaitForSeconds(1); //1초 대기
            titleUI.signUpPansl.gameObject.SetActive(false); //회원가입창 끄고
            titleUI.errorMessage.text = "SignUp Successful!";//메세지 바꾸고
            yield return new WaitForSeconds(0.5f); //잠깐 대기
            titleUI.signUpCompleteBtn.interactable = true;
            titleUI.signUpLoingToBtn.interactable = true;
            titleUI.errorMessage.gameObject.SetActive(false); //메세지 끄고
            titleUI.loginPanel.gameObject.SetActive(true); // 로그인 판넬 키기
        }
    }
}
