using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FriendsSystem
{
    public class FriendListItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _onlineStatus;

        private Action<string, User> _onClick;
        private string _userId;
        private User _user;

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClick?.Invoke(_userId, _user);
        }

        public void Set(string id, User user, Action<string, User> onclick)
        {
            _userId = id;
            _user = user;
            _nameText.text = user.UserName;
            _onClick = onclick;
            _onlineStatus.color = user.IsOnline ? Color.green : Color.red;
        }
    }
}