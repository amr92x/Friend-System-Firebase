using Firebase.Auth;
using UnityEngine;
using System;

namespace FriendsSystem
{
    public class SignUpController : Controller
    {
        private FirebaseAuth _auth;

        public async void CreateUserButtonPressed(string email, string password, string userName, Action<bool> callback)
        {
            if(_auth == null)
                _auth = FirebaseAuth.DefaultInstance;

            var task = _auth.CreateUserWithEmailAndPasswordAsync(email, password);
            await task;

            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                callback?.Invoke(false);
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                callback?.Invoke(false);
                return;
            }
            callback?.Invoke(true);

            // Firebase user has been created.
            AuthResult result = task.Result;
            UserProfile profile = new()
            {
                DisplayName = userName
            };
            string userId = result.User.UserId;
            _ = result.User.UpdateUserProfileAsync(profile);

            await FireDatabaseAPI.UpdateProfile(userName, userId, false);
        }

        public override void OnDestroy()
        {
        }
    }
}