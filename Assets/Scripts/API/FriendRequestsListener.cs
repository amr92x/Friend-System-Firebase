using UnityEngine;
using Firebase.Database;
using System;
using System.Threading.Tasks;

namespace FriendsSystem.API
{
    public class FriendRequestsListener
    {
        public event Action<FriendRequest> FriendRequestAdded;
        public event Action<FriendRequest> FriendRequestChanged;
        public event Action<FriendRequest> FriendRequestRemoved;

        private DatabaseReference _friendRequestDB;
        private EventHandler<ChildChangedEventArgs> ChildAddedListener;
        private EventHandler<ChildChangedEventArgs> ChildRemovedListener;
        private EventHandler<ChildChangedEventArgs> ChildChangedListener;

        /// <summary>
        /// Make sure to dispose after usuage has ended
        /// </summary>
        /// <param name="userId"></param>
        public FriendRequestsListener(string userId) 
        {
            if (string.IsNullOrEmpty(userId)) 
            {
                Debug.LogError("Invalid user id entered, couldnt start listener");
                return;
            }
            string requestKey = FireDatabaseAPI.GetFriendRequestKey(userId);
            _friendRequestDB = FirebaseDatabase.DefaultInstance.RootReference.Child(FireDatabaseAPI.FRIEND_REQUESTS).Child(requestKey).Child(FireDatabaseAPI.REQUESTS);
            ChildAddedListener = ChildAdded;
            ChildChangedListener = ChildChanged;
            ChildRemovedListener = ChildRemoved;

            _friendRequestDB.ChildAdded += ChildAddedListener;
            _friendRequestDB.ChildChanged += ChildChangedListener;
            _friendRequestDB.ChildRemoved += ChildRemovedListener;
        }

        public Task<DataSnapshot> GetValues()
        {
            return _friendRequestDB.GetValueAsync();
        }

        private void ChildAdded(object sender, ChildChangedEventArgs e)
        {
            FriendRequestAdded?.Invoke(JsonUtility.FromJson<FriendRequest>(e.Snapshot.GetRawJsonValue()));
        }

        private void ChildChanged(object sender, ChildChangedEventArgs e)
        {
            FriendRequestChanged?.Invoke(JsonUtility.FromJson<FriendRequest>(e.Snapshot.GetRawJsonValue()));
        }

        private void ChildRemoved(object sender, ChildChangedEventArgs e)
        {
            FriendRequestRemoved?.Invoke(JsonUtility.FromJson<FriendRequest>(e.Snapshot.GetRawJsonValue()));
        }

        public void Dispose()
        {
            _friendRequestDB.ChildAdded -= ChildAddedListener;
            _friendRequestDB.ChildChanged -= ChildChangedListener;
            _friendRequestDB.ChildRemoved -= ChildRemovedListener;
        }
    }
}