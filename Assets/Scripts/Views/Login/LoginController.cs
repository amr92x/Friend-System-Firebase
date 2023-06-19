using UnityEngine;
using Firebase.Auth;
using System;

namespace FriendsSystem
{
    public class LoginController : Controller
    {
        private const string SAVED_USER = "saved-user";
        private const string REMEMBERED = "remembered";
        public Action<bool> LoginSuccessfull;
        private FirebaseAuth _auth;
        public bool HasUserSaved;
        public string UserId => _auth != null && _auth.CurrentUser != null ? _auth.CurrentUser.UserId : string.Empty;
        public LoginController()
        {
            _auth = FirebaseAuth.DefaultInstance;
        }
        
        public bool CheckSavedUser()
        {
            if (_auth.CurrentUser == null)
                return false;

            //var savedUser = PlayerPrefs.GetString(SAVED_USER);
            //var remembered = PlayerPrefs.GetInt(REMEMBERED, 0);
            //if (remembered == 1 && !string.IsNullOrEmpty(savedUser) && _auth.CurrentUser.UserId == savedUser)
            //{
            //    return true;
            //}
            return false;
        }
        private async void Login(string email, string password, bool rememberMe)
        {
            if (_auth == null)
                _auth = FirebaseAuth.DefaultInstance;

            var task = _auth.SignInWithEmailAndPasswordAsync(email, password);
            await task;
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                LoginSuccessfull?.Invoke(false);
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                LoginSuccessfull?.Invoke(false);
                return;
            }
            AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            await FireDatabaseAPI.UpdateProfile(result.User.DisplayName, result.User.UserId, true);

            PlayerPrefs.SetString(SAVED_USER, rememberMe ? UserId : string.Empty);
            PlayerPrefs.SetInt(REMEMBERED, rememberMe ? 1 : 0);

            LoginSuccessfull?.Invoke(true);
        }

        public void OnSubmit(string email, string password, bool rememberMe)
        {
            Login(email, password, rememberMe);
        }

        public override void OnDestroy()
        {
        }
    }
}