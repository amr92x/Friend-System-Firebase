using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FriendsSystem
{
    public class Dashboard : View<DashboardController>
    {
        [SerializeField] private Button _searchButton;
        [SerializeField] private Button _logoutButton;
        [SerializeField] private Button _friendRequestsButton;
        [SerializeField] private TextMeshProUGUI _userNameText;
        [SerializeField] private FriendListItemPooler _pooler;
        private List<FriendListItem> _createdItems = new();
        public override bool HasBack() => false;

        private void Start()
        {
            SetController(new DashboardController());
            _userNameText.text = Controller.CurrentUserName;
            _searchButton.onClick.AddListener(OnSearchFriendsButtonClicked);
            _logoutButton.onClick.AddListener(OnLogOutButtonClicked);
            _friendRequestsButton.onClick.AddListener(OnFriendRequestsButtonClicked);
            SetFriendsList();
        }

        public override void OnBack()
        {
            ClearItems();
            SetFriendsList();
        }

        private void OnFriendRequestsButtonClicked()
        {
            _friendRequestsButton.interactable = false;
            UIService.Navigate<FriendRequests>(new FriendRequestsController(Controller.CurrentUserId));
        }

        private void OnLogOutButtonClicked()
        {
            _logoutButton.interactable = false;
            Controller.Logout(success =>
            {
                UIService.Navigate<Login>();
            });
        }

        private void OnSearchFriendsButtonClicked()
        {
            _searchButton.interactable = false;
            UIService.Navigate<FriendFinder>();
        }

        private void SetFriendsList()
        {
            Controller.GetFriends(friends =>
            {
                if(friends == null)
                {
                    Debug.LogError("Friends are null");
                    return;
                }

                foreach (var user in friends)
                {
                    DisplayFriend(user.Key, user.Value);
                }
            });
        }

        private void ClearItems()
        {
            for (int i = 0; i < _createdItems.Count; i++)
            {
                _pooler.Pool.Release(_createdItems[i]);
            }
            _createdItems.Clear();
        }

        private void DisplayFriend(string userId, User user)
        {
            var listItem = _pooler.Pool.Get();
            listItem.Set(userId, user, OnFriendItemClicked);
            _createdItems.Add(listItem);
        }

        private void OnFriendItemClicked(string userId, User user)
        {
            UIService.Navigate<Messenger>(new MessengerController(userId, user.UserName));
        }

        private void OnEnable()
        {
            _searchButton.interactable = true;
            _friendRequestsButton.interactable = true;
            _logoutButton.interactable = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _searchButton.onClick.RemoveAllListeners();
            _logoutButton.onClick.RemoveAllListeners();
            _friendRequestsButton.onClick.RemoveAllListeners();
        }
    }
}