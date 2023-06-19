using Firebase.Database;
using Firebase.Extensions;
using FriendsSystem.API;
using System;
using UnityEngine;

namespace FriendsSystem
{
    public class FriendRequestsController : Controller
    {
        private FriendRequestsListener _listener;
        public event Action<string, string> FriendRequestRecieved;
        public event Action<string, string> FriendRequestChangedRemoved;

        private string _currentUserId;

        public FriendRequestsController(string currentUserId)
        {
            if (string.IsNullOrEmpty(currentUserId))
            {
                Debug.LogError("No user id received, make sure user is authorized");
                return;
            }

            _currentUserId = currentUserId;
            _listener = new(currentUserId);
            _listener.FriendRequestAdded += FriendRequestAddedHandler;
            _listener.FriendRequestChanged += FriendRequestChangedRemovedHandler;
            _listener.FriendRequestRemoved += FriendRequestChangedRemovedHandler;
        }

        public async void GetValues()
        {
            await _listener.GetValues().ContinueWithOnMainThread(task =>
            {
                if(task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError($"Error getting requests {task.Exception}");
                    return;
                }

                foreach (var snap in task.Result.Children)
                {
                    Debug.Log(snap);
                    var friendRequest = JsonUtility.FromJson<FriendRequest>(snap.GetRawJsonValue());
                    FriendRequestAddedHandler(friendRequest);
                }
            });
        }

        public void FriendRequestAccepted(string userId, string userName)
        {
            SetFriendRequestState(userId, userName, FriendRequestState.Accepted);
            //Add user to friends
            FireDatabaseAPI.AddToFriendsNode(_currentUserId, userId);
        }

        private void SetFriendRequestState(string userId, string userName, FriendRequestState newState)
        {
            string requestKey = FireDatabaseAPI.GetFriendRequestKey(_currentUserId);
            var recievedRequestDBRef = FirebaseDatabase.DefaultInstance.RootReference.Child(FireDatabaseAPI.FRIEND_REQUESTS).Child(requestKey).Child(FireDatabaseAPI.REQUESTS).Child(userId);
            recievedRequestDBRef.SetRawJsonValueAsync(JsonUtility.ToJson(new FriendRequest(userId, _currentUserId, userName, newState))).ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                    Debug.LogError($"{task.Exception}");
            });
        }

        public void FriendRequestDeclined(string userId)
        {
            FireDatabaseAPI.RemoveFriendRequest(_currentUserId, userId);
        }

        private void FriendRequestAddedHandler(FriendRequest friendRequest)
        {
            if (friendRequest.state != FriendRequestState.Pending)
                return;

            FriendRequestRecieved?.Invoke(friendRequest.senderID, friendRequest.senderName);
        }
        private void FriendRequestChangedRemovedHandler (FriendRequest friendRequest)
        {
            FriendRequestChangedRemoved?.Invoke(friendRequest.senderID, friendRequest.senderName);
        }

        public override void OnDestroy()
        {
            if(_listener != null )
            {
                _listener.FriendRequestAdded -= FriendRequestAddedHandler;
                _listener.FriendRequestChanged -= FriendRequestChangedRemovedHandler;
                _listener.FriendRequestRemoved -= FriendRequestChangedRemovedHandler;
                _listener.Dispose();
            }
        }
    }
}
