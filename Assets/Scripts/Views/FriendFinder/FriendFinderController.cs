using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using FriendsSystem.API;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FriendsSystem
{
    [System.Serializable]
    public class UserSearchResult : User
    {
        public bool CanSendFriendRequest;

        public UserSearchResult(string userName) : base(userName)
        {

        }
    }

    public class FriendFinderController : Controller
    {
        public event Action<Dictionary<string, UserSearchResult>> SearchFinished;
        private FirebaseAuth _auth;
        private DatabaseReference _friendsDatabase;
        private HashSet<string> _friendsList;
        private HashSet<string> _friendsRequestsList;
        private string _currentUserId;
        private EventHandler<ChildChangedEventArgs> FriendsDBChanged;
        private void Init()
        {
            _friendsList = null;
            _auth = FirebaseAuth.DefaultInstance;
            if (_auth == null || _auth.CurrentUser == null)
            {
                Debug.LogError("No authenticated user, aborting");
                return;
            }
            _currentUserId = _auth.CurrentUser.UserId;

            _friendsDatabase = FirebaseDatabase.DefaultInstance.RootReference.Child(FireDatabaseAPI.USERS_PRIVATE).Child(_currentUserId).Child(FireDatabaseAPI.FRIENDS);
            GetFriendsList();
            FriendsDBChanged = FriendsDatabaseChanged;
            _friendsDatabase.ChildAdded += FriendsDBChanged;
            _friendsDatabase.ChildRemoved += FriendsDBChanged;
        }

        private void FriendsDatabaseChanged(object sender, ChildChangedEventArgs e)
        {
            _friendsList = null;
            GetFriendsList(); 
        }

        public void OnSearchButtonPressed(string searchWord)
        {
            FindUsers(searchWord);
        }

        private void GetFriendsList()
        {
            _friendsDatabase.GetValueAsync().ContinueWith(task =>
            {
                _friendsList = new HashSet<string>();
                foreach (var snap in task.Result.Children)
                {
                    _friendsList.Add(snap.Key);
                }
            });
        }

        private async void FindUsers(string searchWord)
        {
            if (_auth == null)
                Init();

            await UniTask.WaitUntil(()=> _friendsList != null);

            //Check friend Requests
            _friendsRequestsList = new();
            await new GetFriendRequests().GetSentRequestsAsIDs().ContinueWith(hashset => _friendsRequestsList = hashset.Result);

            Dictionary<string, UserSearchResult> users = new();
            searchWord = searchWord.ToLower();
            var usersTask = FireDatabaseAPI.GetUsers();
            await usersTask;
            if (usersTask.IsFaulted)
            {
                Debug.LogError("Error finding users: " + usersTask.Exception);
                return;
            }

            if (usersTask.IsCompleted)
            {
                DataSnapshot snapshot = usersTask.Result;

                if (snapshot != null && snapshot.HasChildren)
                {
                    // Loop through each child (user) that matches the criteria
                    foreach (var childSnapshot in snapshot.Children)
                    {
                        var otherId = childSnapshot.Key;
                        // Deserialize the user JSON into a User object
                        UserSearchResult user = JsonUtility.FromJson<UserSearchResult>(childSnapshot.GetRawJsonValue());
                        // Exclude the current user from the results
                        user.CanSendFriendRequest = !(_friendsList.Contains(otherId) || _friendsRequestsList.Contains(otherId));
                        if (otherId != _currentUserId && user.UserName.ToLower().Contains(searchWord))
                        {
                            users[otherId] = user;
                        }
                    }
                }
                else
                {
                    Debug.Log("No users found with the specified criteria.");
                }
            }
            SearchFinished?.Invoke(users);
        }

        public override void OnDestroy()
        {
            if(_friendsDatabase != null)
            {
                _friendsDatabase.ChildAdded -= FriendsDBChanged;
                _friendsDatabase.ChildRemoved -= FriendsDBChanged;
            }
        }
    }
}