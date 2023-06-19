using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;

namespace FriendsSystem
{
    public class MessageListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _senderName;
        private RectTransform _rectTransform;
        private void Awake()
        {
            _rectTransform = transform as RectTransform;
        }
        public void Set(string message, string senderName, DateTime time)
        {
            _messageText.text = $"{senderName}: {message} \n {time:HH:mm:ss ddd dd}";
            UpdateBoxHeight();
        }

        private async void UpdateBoxHeight()
        {
            await UniTask.Yield();
            var size = _rectTransform.sizeDelta;
            size.y = _messageText.rectTransform.sizeDelta.y;
            _rectTransform.sizeDelta = size;
        }
    }
}