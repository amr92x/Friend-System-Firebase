using UnityEngine;
using Firebase.Database;
using System;
using Firebase.Auth;
using FriendsSystem.API;

namespace FriendsSystem
{
    public class SendFriendRequest
    {
        private DatabaseReference _databaseReference;
        private DatabaseReference _myDBReference;
        private FirebaseAuth _auth;

        public SendFriendRequest()
        {
            // Set up the Firebase database reference
            _auth = FirebaseAuth.DefaultInstance;
        }

        public void SendRequest(string receiverUserID, Action<bool> callback)
        {
            if(_auth == null || _auth.CurrentUser== null)
            {
                Debug.LogError("No authenticated user signed in, Aborting");
                callback?.Invoke(false);
                return;
            }
            var currentUser = _auth.CurrentUser.UserId;
            // Create a unique key for the friend request
            string requestKey = FireDatabaseAPI.GetFriendRequestKey(receiverUserID);
            _databaseReference = FirebaseDatabase.DefaultInstance.RootReference.Child(FireDatabaseAPI.FRIEND_REQUESTS).Child(requestKey).Child(FireDatabaseAPI.REQUESTS);
            _myDBReference = FirebaseDatabase.DefaultInstance.RootReference.Child(FireDatabaseAPI.USERS_PRIVATE).Child(currentUser).Child(FireDatabaseAPI.REQUESTS);
            // Create a friend request object
            FriendRequest friendRequest = new(currentUser, receiverUserID, _auth.CurrentUser.DisplayName);

            // Save the friend request object to the database Under both the user id so they can see the requests and my db so I can see my sent requests
            var requestJson = JsonUtility.ToJson(friendRequest);
            _databaseReference.Child(currentUser).SetRawJsonValueAsync(requestJson)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        Debug.LogError("Failed to send friend request: " + task.Exception);
                        callback?.Invoke(false);
                    }
                    else
                    {
                        Debug.Log("Friend request sent successfully!");
                        callback?.Invoke(true);

                    }
                });
            _myDBReference.Push().SetValueAsync(receiverUserID)
                .ContinueWith(mydbTask =>
                {
                    if (mydbTask.IsFaulted || mydbTask.IsCanceled)
                    {
                        Debug.LogError("Failed to send friend request to my db: " + mydbTask.Exception);
                    }
                });
        }
    }

    [Serializable]
    public class FriendRequest
    {
        public string senderID;
        public string receiverID;
        public string senderName;
        public FriendRequestState state;

        public FriendRequest(string senderID, string receiverID, string senderName)
        {
            this.senderID = senderID;
            this.receiverID = receiverID;
            this.senderName = senderName;
            state = FriendRequestState.Pending;
        }

        public FriendRequest(string senderID, string receiverID, string senderName, FriendRequestState state) : this(senderID, receiverID, senderName)
        {
            this.senderID = senderID;
            this.receiverID = receiverID;
            this.senderName = senderName;
            this.state = state;
        }
    }

}