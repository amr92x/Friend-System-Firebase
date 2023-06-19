using UnityEngine;
using Firebase;
using Firebase.Analytics;
using Firebase.Auth;
using Firebase.Database;
using System.Threading.Tasks;

namespace FriendsSystem
{
    public class FireDatabaseAPI : MonoBehaviour
    {
        //As the Rules on the console
        public const string USERS = "users";
        public const string USERS_PRIVATE = "users-private";
        public const string MESSAGES = "messages";
        public const string FRIEND_REQUESTS = "friend-requests";
        public const string REQUESTS = "requests";
        public const string FRIENDS = "friends";

        private void Start()
        {
            InitFirebase();
            UIService.Navigate<Login>();
        }

        private void InitFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
            });
        }

        private async void OnApplicationQuit()
        {
            var auth = FirebaseAuth.DefaultInstance;
            if (auth != null && auth.CurrentUser != null)
            {
                await UpdateProfile(auth.CurrentUser.DisplayName, auth.CurrentUser.UserId, false);
            }
        }

#if UNITY_ANDROID

        private async void OnApplicationPause(bool pause)
        {
            var auth = FirebaseAuth.DefaultInstance;
            if (auth != null && auth.CurrentUser != null)
            {
                await UpdateProfile(auth.CurrentUser.DisplayName, auth.CurrentUser.UserId, false);
            }
        }

#endif
        public static Task<DataSnapshot> GetUsers()
        {
            return FirebaseDatabase.DefaultInstance.RootReference.Child(USERS).GetValueAsync();
        }

        public static Task<DataSnapshot> GetUser(string userId)
        {
            return FirebaseDatabase.DefaultInstance.RootReference.Child(USERS).Child(userId).GetValueAsync();
        }

        public static Task<DataSnapshot> GetFriends(string currentUserId)
        {
            return FirebaseDatabase.DefaultInstance.RootReference.Child(USERS_PRIVATE).Child(currentUserId).Child(FRIENDS).GetValueAsync();
        }

        public static Task UpdateProfile(string userName, string userId, bool isOnline)
        {
            User user = new(userName);
            user.IsOnline = isOnline;
            return FirebaseDatabase.DefaultInstance.RootReference.Child(USERS).Child(userId).SetRawJsonValueAsync(JsonUtility.ToJson(user));
        }

        public static void AddToFriendsNode(string currentUser, string userId)
        {
            FirebaseDatabase.DefaultInstance.RootReference.Child(USERS_PRIVATE).Child(currentUser).Child(FRIENDS).Child(userId).SetValueAsync(userId);
        }

        public static Task RemoveFriendRequest(string reciever, string sender)
        {
            string requestKey = GetFriendRequestKey(reciever);
            Debug.Log($"Removing Requests Node {reciever}/{sender}");
            return RemoveNode(FirebaseDatabase.DefaultInstance.RootReference
                .Child(FRIEND_REQUESTS)
                .Child(requestKey)
                .Child(REQUESTS)
                .Child(sender));
        }

        public static Task RemoveNode(DatabaseReference db)
        {
            return db.RemoveValueAsync().ContinueWith(task =>
            {
                if(task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError($"Failed to remove node {task.Exception}");
                }
                else if(task.IsCompleted)
                {
                    Debug.LogError($"remove node Succeded");
                }
            });
        }

        public static string GetFriendRequestKey(string receiverUserID)
        {
            return $"fr-{receiverUserID}";
        }
    }
}