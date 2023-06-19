using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FriendsSystem
{
    public class Messenger : View<MessengerController>
    {
        [SerializeField] private TMP_InputField _messageInput;
        [SerializeField] private Button _sendButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private MessageListItemPooler _pooler;
        [SerializeField] private ScrollRect _scrollView;
        private List<MessageListItem> _createdItems = new();

        private void Start()
        {
            _sendButton.onClick.AddListener(OnSendMessageButtonClicked);
            _backButton.onClick.AddListener(OnBackButtonClicked);
        }

        private void OnBackButtonClicked()
        {
            if(Controller != null) 
            { 
                Controller.MessageRecieved -= DisplayChatMessage; 
            }
            UIService.Back(true);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Controller != null)
            {
                Controller.OnDisable();
                Controller.MessageRecieved -= DisplayChatMessage;
            }
            _sendButton.onClick.RemoveAllListeners();
            _backButton.onClick.RemoveAllListeners();
        }

        public void OnEnable()
        {
            _messageInput.text = string.Empty;
            ClearChat();
        }

        public void OnDisable()
        {
            if(Controller != null)
            {
                Controller.OnDisable();
            }
        }

        private void ClearChat()
        {
            for (int i = 0; i < _createdItems.Count; i++)
            {
                _pooler.Pool.Release(_createdItems[i]);
            }
            _createdItems.Clear();
        }

        public override bool HasBack() => true;

        public void OnSendMessageButtonClicked()
        {
            if (string.IsNullOrEmpty(_messageInput.text))
            {
                Debug.LogWarning("Attempting to send emtpy message, prevented");
                return;
            }

            Controller.OnSendMessageSubmit(_messageInput.text);
        }

        public override void SetController(Controller controller = null)
        {
            if (controller == null) 
            {
                Debug.LogError("No Messenger controller passed to creation of View");
                UIService.Back();
                return;
            }

            if (Controller != null) 
            {
                Controller.MessageRecieved -= DisplayChatMessage;
            }

            ClearChat();
            if (Controller == null)
            {
                Controller = controller as MessengerController;
            }
            else if (controller is MessengerController messengerController && messengerController.OtherId != Controller.OtherId)
            {
                //Replace current otherId with the new otherId. 
                Controller.SetOtherId(messengerController.OtherId, messengerController.OtherName);
            }
            Controller.Init();
            Controller.MessageRecieved += DisplayChatMessage;
        }

        private void DisplayChatMessage(Message message)
        {
            var listItem = _pooler.Pool.Get();
            var senderName = message.SenderId == Controller.OtherId ? Controller.OtherName : Controller.MyName;
            listItem.Set(message.Text, senderName, message.Timestamp);
            _createdItems.Add(listItem);
        }
    }
}

