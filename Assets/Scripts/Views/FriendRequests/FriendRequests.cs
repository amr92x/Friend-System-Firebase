using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FriendsSystem
{
    public class FriendRequests : View<FriendRequestsController>
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private FriendRequestListItemPooler _pooler;
        private Dictionary<string, FriendRequestListItem> _createdItems = new();

        private void Start()
        {
            _backButton.onClick.AddListener(() => UIService.Back());
        }

        public override bool HasBack() => true;
        public override void SetController(Controller controller)
        {
            Controller = controller as FriendRequestsController;
            Controller.FriendRequestRecieved += Controller_FriendRequestRecieved;
            Controller.FriendRequestChangedRemoved += Controller_FriendRequestRemoved;
            Controller.GetValues();
        }

        private void Controller_FriendRequestRecieved(string userId, string userName)
        {
            DisplayFriendRequests(userId, userName);
        }

        private void Controller_FriendRequestRemoved(string userId, string _)
        {
            if (!_createdItems.ContainsKey(userId))
                return;

            var item = _createdItems[userId];
            if(item == null)
            {
                return;
            }
            _pooler.Pool.Release(item);
            _createdItems.Remove(userId);
        }

        private void DisplayFriendRequests(string userId, string userName)
        {
            if (_createdItems.ContainsKey(userId))
            {
                return;
            }

            var listItem = _pooler.Pool.Get();
            listItem.Set(userId, userName, OnUserAnswer);
            _createdItems.Add(userId, listItem);
        }

        public void OnUserAnswer(string userId, string userName, bool accept)
        {
            if(accept)
            {
                Controller.FriendRequestAccepted(userId, userName);
            }
            else
            {
                Controller.FriendRequestDeclined(userId);
            }
        }

        protected override void OnDestroy()
        {
            _backButton.onClick.RemoveAllListeners();
            if (Controller != null) 
            {
                Controller.FriendRequestRecieved -= Controller_FriendRequestRecieved;
                Controller.FriendRequestChangedRemoved -= Controller_FriendRequestRemoved;
            }
            base.OnDestroy();
        }


    }
}
