using Firebase.Auth;
using FriendsSystem.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FriendsSystem
{
    public class DashboardController : Controller
    {
        private FirebaseAuth _auth;
        private List<FriendRequest> _friendRequests;
        private string _currentUserId;
        public string CurrentUserId => _currentUserId;
        public string CurrentUserName => _auth != null && _auth.CurrentUser != null ? _auth.CurrentUser.DisplayName : "Error";
        public bool HasFriendRequests => _friendRequests != null && _friendRequests.Count > 0;
        private GetFriendRequests _friendRequestsGetter;
        private FriendRequestsListener _listener;
        public DashboardController()
        {
            _auth = FirebaseAuth.DefaultInstance;
            _currentUserId = _auth != null && _auth.CurrentUser != null ? _auth.CurrentUser.UserId : string.Empty;
            _friendRequestsGetter = new GetFriendRequests();
            _friendRequestsGetter.GetIncomingRequests(reqs =>
            {
                if (reqs != null)
                {
                    _friendRequests = reqs;
                }
            }, true, FriendRequestState.Pending);
        }

        public async Task CheckAcceptedFriends()
        {
            var task = _friendRequestsGetter.GetAccptedSentRequests();
            await task;

            if(task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Error checking accepted friend requests {task.Exception}");
                return;
            }

            var users = task.Result;
            if (users.Count > 0)
            {
                //Process friends who accepted
                foreach (var userId in users)
                {
                    Debug.Log($"Processing user {userId}");
                    FireDatabaseAPI.AddToFriendsNode(_currentUserId, userId);
                    //await FireDatabaseAPI.RemoveFriendRequest(userId, _currentUserId); //should remove from DB, unless needed, but should be affected by security rules if uncommeneted
                }
            }
        }

        public async void GetFriends(Action<Dictionary<string, User>> callback)
        {
            Debug.Log("Get friends called");
            await CheckAcceptedFriends();

            var friendsTask = FireDatabaseAPI.GetFriends(_currentUserId);
            await friendsTask;

            if (friendsTask.IsFaulted || friendsTask.IsCanceled)
            {
                Debug.LogError($"Failed to load friends {friendsTask.Exception}");
                callback?.Invoke(null);
                return;
            }

            var users = new Dictionary<string, User>();

            foreach (var friend in friendsTask.Result.Children)
            {
                var userTask = FireDatabaseAPI.GetUser(friend.Key);
                await userTask;
                if (userTask.IsFaulted || userTask.IsCanceled)
                {
                    Debug.LogError($"Failed to load users {userTask.Exception}");
                    callback?.Invoke(null);
                    return;
                }
                users[friend.Key] = JsonUtility.FromJson<User>(userTask.Result.GetRawJsonValue());
            }

            callback?.Invoke(users);
        }

        public async void Logout(Action<bool> callback)
        {
            if (_auth != null && _auth.CurrentUser != null) 
            {
                await FireDatabaseAPI.UpdateProfile(_auth.CurrentUser.DisplayName, _auth.CurrentUser.UserId, false);
                _auth.SignOut();
                callback?.Invoke(true);
            }
            else
            {
                callback?.Invoke(false);
            }
        }

        public override void OnDestroy()
        {
            _listener?.Dispose();
        }
    }
}