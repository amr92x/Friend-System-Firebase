using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FriendsSystem
{
    public class FriendRequestListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _senderNameText;
        [SerializeField] private Button _acceptButton;
        [SerializeField] private Button _declineButton;

        private string _senderId;
        private Action<string, string, bool> DecisionSelected;
        
        public string SenderId => _senderId;


        private void Start()
        {
            _acceptButton.onClick.AddListener(OnAcceptButtonClicked);
            _declineButton.onClick.AddListener(OnDeclineButtonClicked);
        }

        public void Set(string userId, string userName, Action<string, string, bool> callback)
        {
            _senderId = userId;
            _senderNameText.text = userName;
            DecisionSelected = callback;
        }

        private void OnAcceptButtonClicked()
        {
            DecisionSelected?.Invoke(_senderId, _senderNameText.text, true);
        }

        private void OnDeclineButtonClicked()
        {
            DecisionSelected?.Invoke(_senderId, _senderNameText.text, false);
        }

        private void OnDestroy()
        {
            _acceptButton.onClick.RemoveAllListeners();
            _declineButton.onClick.RemoveAllListeners();
        }
    }
}