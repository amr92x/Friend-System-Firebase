using Firebase.Auth;
using Firebase.Database;
using System;
using System.Linq;
using UnityEngine;

namespace FriendsSystem
{
    [Serializable]
    public class Message
    {
        public string SenderId;
        public string Text;
        public DateTime Timestamp;

        public Message(string senderId, string text)
        {
            SenderId = senderId;
            Text = text;
            Timestamp = DateTime.UtcNow;
        }

        public Message(string senderId, string text, DateTime timestamp)
        {
            SenderId = senderId;
            Text = text;
            Timestamp = timestamp;
        }
    }

    public class MessengerController : Controller
    {
        public event Action<Message> MessageRecieved;

        private FirebaseAuth _auth;
        private DatabaseReference _database;
        private EventHandler<ChildChangedEventArgs> MessageListener;
        private string _otherId;
        private string _otherName;
        private string _myId;
        private string _myName;
        public string OtherId { get => _otherId; }
        public string OtherName { get => _otherName; }
        public string MyName { get => _myName; }
        public MessengerController(string otherId, string otherName)
        {
            SetOtherId(otherId, otherName);
        }

        public void Init()
        {
            _auth = FirebaseAuth.DefaultInstance;
            _myId = _auth.CurrentUser.UserId;
            _myName = _auth.CurrentUser.DisplayName;

            if (_database != null)
                _database.ChildAdded -= MessageListener;

            if (_myId.Length == 0 && OtherId.Length == 0)
                Debug.LogError("Something wrong with User ids");

            string messageKey = GetMessageKey();
            _database = FirebaseDatabase.DefaultInstance.RootReference.Child(FireDatabaseAPI.MESSAGES).Child(messageKey);
            MessageListener = CurrentListener;
            _database.ChildAdded += MessageListener;
            _database.GetValueAsync();

            void CurrentListener(object sender, ChildChangedEventArgs e)
            {
                var json = e.Snapshot.GetRawJsonValue();
                if (string.IsNullOrEmpty(json))
                    return;

                var value = JsonUtility.FromJson<Message>(json);
                MessageRecieved?.Invoke(value);
            }
        }

        private string GetMessageKey()
        {
            // Kind of a hack, but I need both user to see the same Child of the database, and the name should be consistent for both when accessing the database containing their chat
            return $"msgId-{(_myId.First() > OtherId.First() ? _myId : OtherId)}-{(_myId.First() > OtherId.First() ? OtherId : _myId)}";
        }

        public void SetOtherId(string otherId, string otherName)
        {
            if (ValidateId(otherId))
                _otherId = otherId;
            else
                return;
            _otherName = otherName;
        }

        public void OnSendMessageSubmit(string message)
        {
            if (string.IsNullOrEmpty(_otherId))
            {
                Debug.LogError("Other id is not set");
                return;
            }

            if(_database == null)
            {
                Debug.LogError("Data base is null");
                return;
            }
            _database.Push().SetRawJsonValueAsync(JsonUtility.ToJson(new Message(_myId, message, DateTime.UtcNow)));
        }
    
        private bool ValidateId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("Other id is not set");
                return false;
            }
            return true;
        }


        public void OnDisable()
        {
            _database.ChildAdded -= MessageListener;
        }

        public override void OnDestroy()
        {
            _database.ChildAdded -= MessageListener;
        }
    }
}

