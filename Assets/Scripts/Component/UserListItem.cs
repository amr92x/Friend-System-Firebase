using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FriendsSystem
{
    public class UserListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _userNameText;
        [SerializeField] private Image _onlineStatus;
        [SerializeField] private Button _friendRequestButton;
        private Action<string, UserListItem> FriendRequestClicked;
        private string _userId;
        private void Start()
        {
            _friendRequestButton.onClick.AddListener(() => 
            {
                FriendRequestClicked(_userId, this);
                FriendRequestClicked = null;
            });
        }

        private void OnDestroy()
        {
            _friendRequestButton.onClick.RemoveAllListeners();
        }

        public void Set(string userId, UserSearchResult user, Action<string, UserListItem> onFriendRequestClick = null)
        {
            _userId = userId;
            _userNameText.text = user.UserName;
            _onlineStatus.color = user.IsOnline ? Color.green : Color.red;
            SetFriendRequestButton(user.CanSendFriendRequest);
            FriendRequestClicked = onFriendRequestClick;
        }

        public void SetFriendRequestButton(bool enabled)
        {
            _friendRequestButton.gameObject.SetActive(enabled);
        }
    }
}