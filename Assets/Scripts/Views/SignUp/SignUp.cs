using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FriendsSystem
{
    public class SignUp : View<SignUpController>
    {
        public override bool HasBack() => true;
        [SerializeField] private TMP_InputField _userInput;
        [SerializeField] private TMP_InputField _emailInput;
        [SerializeField] private TMP_InputField _passwordInput;
        [SerializeField] private Button _submitButton;
        [SerializeField] private Button _backButton;

        protected void Start()
        {
            SetController(new SignUpController());
            _submitButton.onClick.AddListener(OnSubmitButtonClicked);
            _backButton.onClick.AddListener(OnBackButtonClicked);
        }

        private void OnEnable()
        {
            _userInput.text = string.Empty;
            _emailInput.text = string.Empty;
            _passwordInput.text = string.Empty;
        }
        private void OnSubmitButtonClicked()
        {
            if (_emailInput == null || _passwordInput == null || _userInput == null)
            {
                Debug.LogError($"Something is null [{_emailInput}] Or [{_passwordInput}] Or [{_userInput}]");
                return;
            }

            if (string.IsNullOrEmpty(_emailInput.text) || string.IsNullOrEmpty(_passwordInput.text) || string.IsNullOrEmpty(_userInput.text))
            {
                Debug.LogError($"Something is empty or null [{_emailInput.text}] Or [{_passwordInput.text}] Or [{_userInput.text}]");
                return;
            }

            if (!_emailInput.text.Contains('@'))
            {
                Debug.LogError("Email has no @");
                return;
            }
            _submitButton.interactable = false;
            Controller.CreateUserButtonPressed(_emailInput.text, _passwordInput.text, _userInput.text, CreateUserEventHandler);
        }

        private void OnBackButtonClicked()
        {
            UIService.Back();
        }
        private void CreateUserEventHandler(bool success)
        {
            if(success)
            {
                UIService.Back();
            }
            else
            {
                _submitButton.interactable = true;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _submitButton.onClick.RemoveAllListeners();
            _backButton.onClick.RemoveAllListeners();
        }
    }
}