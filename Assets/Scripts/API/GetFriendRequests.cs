using UnityEngine;
using Firebase.Database;
using System;
using Firebase.Auth;
using System.Collections.Generic;
using Firebase.Extensions;
using System.Threading.Tasks;

namespace FriendsSystem.API
{
    public enum FriendRequestState
    {
        Accepted = 1,
        Pending = 0,
        Declined = -1
    }

    public class GetFriendRequests
    {
        private string _currentUserId;
        public GetFriendRequests()
        {
            var auth = FirebaseAuth.DefaultInstance;
            if (auth == null || auth.CurrentUser == null) 
            {
                Debug.LogError("No authenticated user, aborting!");
                return;
            }
            _currentUserId = auth.CurrentUser.UserId;
        }

        public async void GetSentRequests(Action<List<User>> callback)
        {
            var sentRequestsDBRef = FirebaseDatabase.DefaultInstance.RootReference.Child(FireDatabaseAPI.USERS_PRIVATE).Child(_currentUserId).Child(FireDatabaseAPI.REQUESTS);
            var usersDBRef = FirebaseDatabase.DefaultInstance.RootReference.Child(FireDatabaseAPI.USERS);
            var foundRequests = new List<User>();

            var task = sentRequestsDBRef.GetValueAsync();
            await task;

            if (task.IsFaulted && task.IsCanceled)
            {
                Debug.LogError("Couldnt get any requests");
                callback?.Invoke(null);
                return;
            }

            foreach (var snap in task.Result.Children)
            {
                await usersDBRef.Child(snap.Value.ToString()).GetValueAsync().ContinueWithOnMainThread(subtask =>
                {
                    foundRequests.Add(JsonUtility.FromJson<User>(subtask.Result.GetRawJsonValue()));
                });
            }

            callback?.Invoke(foundRequests);
        }

        public async Task<HashSet<string>> GetSentRequestsAsIDs()
        {
            var sentRequestsDBRef = FirebaseDatabase.DefaultInstance.RootReference.Child(FireDatabaseAPI.USERS_PRIVATE).Child(_currentUserId).Child(FireDatabaseAPI.REQUESTS);
            var foundRequests = new HashSet<string>();

            var task = sentRequestsDBRef.GetValueAsync();
            await task;

            if (task.IsFaulted && task.IsCanceled)
            {
                Debug.LogError($"Couldnt get any requests {task.Exception}");
                return foundRequests;
            }

            foreach (var snap in task.Result.Children)
            {
                Debug.LogError($"Fetching : user id = {snap.Value} as {_currentUserId}");
                foundRequests.Add(snap.Value.ToString());
            }

            return foundRequests;
        }

        /// <summary>
        /// When a friend accepts the request, we check the sent requests using the names we have in the Users-private/requests DB and look them up in the friend requests. then check if it was accpeted.
        /// </summary>
        public async Task<HashSet<string>> GetAccptedSentRequests()
        {
            HashSet<string> users = new();
            var getFriends = new GetFriendRequests().GetSentRequestsAsIDs();
            await getFriends;

            HashSet<string> retrieved = getFriends.Result;
            if (retrieved.Count == 0)
            {
                Debug.Log($"No retrieved users");
                return new();
            }

            foreach (var userId in retrieved)
            {
                Debug.Log($"Getting friend request from shared db for user: {userId} as user {_currentUserId}");
                string requestKey = FireDatabaseAPI.GetFriendRequestKey(userId);
                var task = FirebaseDatabase.DefaultInstance.RootReference.Child(FireDatabaseAPI.FRIEND_REQUESTS).Child(requestKey).Child(FireDatabaseAPI.REQUESTS).Child(_currentUserId).GetValueAsync();
                await task;
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError($"Couldnt get {task.Exception}");
                    return users;
                }

                var friendRequest = JsonUtility.FromJson<FriendRequest>(task.Result.GetRawJsonValue());
                Debug.Log($"Fetched Friend request = [{task.Result.GetRawJsonValue()}]");
                if (friendRequest != null && friendRequest.state == FriendRequestState.Accepted) 
                {
                    Debug.Log($"Fetched Friend request added : {task.Result.GetRawJsonValue()}");
                    users.Add(userId);
                }
            }
            return users;
        }

        /// <summary>
        /// Get a list of all incoming friend requests.
        /// </summary>
        /// <param name="callback">Callback with the returned values</param>
        /// <param name="forceState">Force a selected state of requests to be retrieved (Accepted/Pending/Declined requests)</param>
        /// <param name="state">The forced state of the friend requests to be retrieved</param>
        public async void GetIncomingRequests(Action<List<FriendRequest>> callback, bool forceState = false, FriendRequestState state = FriendRequestState.Pending)
        {
            string requestKey = FireDatabaseAPI.GetFriendRequestKey(_currentUserId);
            var recievedRequestsDBRef = FirebaseDatabase.DefaultInstance.RootReference.Child(FireDatabaseAPI.FRIEND_REQUESTS).Child(requestKey).Child(FireDatabaseAPI.REQUESTS);
            var foundRequests = new List<FriendRequest>();

            var task = recievedRequestsDBRef.GetValueAsync();
            await task;

            if (task.IsFaulted && task.IsCanceled)
            {
                Debug.LogError("Couldnt get any requests");
                callback?.Invoke(null);
                return;
            }

            foreach (var freq in task.Result.Children)
            {
                var friendRequest = JsonUtility.FromJson<FriendRequest>(freq.GetRawJsonValue());

                if (friendRequest == null || (forceState && friendRequest.state != state))
                    continue;

                foundRequests.Add(friendRequest);
            }

            callback?.Invoke(foundRequests);
        }
    }
}