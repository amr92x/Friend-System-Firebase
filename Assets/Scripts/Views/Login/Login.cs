using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace FriendsSystem
{
    public class Login : View<LoginController>
    {
        [SerializeField] private TMP_InputField _emailInput;
        [SerializeField] private TMP_InputField _passwordInput;
        [SerializeField] private Button _submitButton;
        [SerializeField] private Button _signUpButton;
        [SerializeField] private Toggle _rememberMeToggle;

        void Start()
        {
            SetController(new LoginController());

            if (Controller.CheckSavedUser())
            {
                LoginCompleteEventHandler(true);
                return;
            }

            Controller.LoginSuccessfull += LoginCompleteEventHandler;
            _submitButton.interactable = true;
            _submitButton.onClick.AddListener(OnSubmitButtonClicked);
            _signUpButton.onClick.AddListener(OnSignUpButtonClicked);
        }
        public override bool HasBack() => false;

        private void LoginCompleteEventHandler(bool success)
        {
            if (success) 
            {
                UIService.Navigate<Dashboard>();
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
            _signUpButton.onClick.RemoveAllListeners();
            Controller.LoginSuccessfull -= LoginCompleteEventHandler;
        }

        private void OnEnable()
        {
            _submitButton.interactable = true;
            _emailInput.text = string.Empty;    
            _passwordInput.text = string.Empty;
        }

        private void OnSubmitButtonClicked()
        {
            if (_emailInput == null || _passwordInput == null)
            {
                Debug.LogError($"Something is null [{_emailInput}] Or [{_passwordInput}]");
                return;
            }

            if (string.IsNullOrEmpty(_emailInput.text) || string.IsNullOrEmpty(_passwordInput.text))
            {
                Debug.LogError($"Something is empty or null [{_emailInput.text}] Or [{_passwordInput.text}]");
                return;
            }

            if (!_emailInput.text.Contains('@'))
            {
                Debug.LogError("Email has no @");
                return;
            }
            _submitButton.interactable = false;
            Controller.OnSubmit(_emailInput.text, _passwordInput.text, _rememberMeToggle.isOn);
        }

        private void OnSignUpButtonClicked()
        {
            UIService.Navigate<SignUp>();
        }
    }
}