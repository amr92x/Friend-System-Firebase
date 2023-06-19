using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FriendsSystem
{
    public class FriendFinder : View<FriendFinderController>
    {
        [SerializeField] private TMP_InputField _searchInput;
        [SerializeField] private Button _searchSubmitButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private UserListItemPooler _pool;
        private List<UserListItem> _createdItems = new();
        public override bool HasBack() => true;

        private void Start()
        {
            SetController(new FriendFinderController());
            Controller.SearchFinished += DisplaySearchResult;
            _searchSubmitButton.onClick.AddListener(OnSubmitClicked);
            _backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnEnable()
        {
            _searchInput.text = string.Empty;
            ClearPool();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Controller.SearchFinished -= DisplaySearchResult;
            _searchSubmitButton.onClick.RemoveAllListeners();
            _backButton.onClick.RemoveAllListeners();
        }

        private void OnSubmitClicked()
        {
            if (string.IsNullOrEmpty(_searchInput.text))
            {
                return;
            }
            Controller.OnSearchButtonPressed(_searchInput.text);
        }
        private void OnBackClicked()
        {
            UIService.Back();
        }

        public void DisplaySearchResult(Dictionary<string, UserSearchResult> users)
        {
            ClearPool();

            foreach (var user in users)
            {
                var listItem = _pool.Pool.Get();
                listItem.Set(user.Key, user.Value, OnFriendRequestClicked);
                _createdItems.Add(listItem);
            }
        }

        private void ClearPool()
        {
            for (int i = 0; i < _createdItems.Count; i++)
            {
                _pool.Pool.Release(_createdItems[i]);
            }
            _createdItems.Clear();
        }

        private void OnFriendRequestClicked(string userId, UserListItem userItem)
        {
            userItem.SetFriendRequestButton(false);
            var sendRequest = new SendFriendRequest();
            sendRequest.SendRequest(userId, (success) =>
            {
                userItem.SetFriendRequestButton(!success);
            });
            //UIService.Navigate<Messenger>(new MessengerController(userId, userName.UserName));
        }
    }
}