using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace FriendsSystem
{
    public class UIService : MonoBehaviour
    {
        [SerializeField] private List<ViewContainer> _views;

        private static bool _initiated = false;
        private static Dictionary<Type, ViewContainer> _viewsDictionary;
        private static UIService Instance;
        private static GameObject _currentView;
        private static Stack<GameObject> _backStack;
        private static GameObject _forwardView;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            _viewsDictionary = new();
            _backStack = new();
        }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            foreach (var view in _views)
            {
                _viewsDictionary.Add(view.GetTypeOfView(), view);
            }
            _initiated = true;
        }

        public static void Back(bool keepCurrentAlive = false)
        {
            if (_backStack.Count == 0)
                return;

            if (keepCurrentAlive)
            {
                _forwardView = _currentView;
                _forwardView.SetActive(false);
            }
            else
                DisposeOfCurrentView();

            _currentView = _backStack.Pop();
            _currentView.SetActive(true);
            if(_currentView.TryGetComponent(out View currentView))
            {
                currentView.OnBack();
            }
        }

        public static async void Navigate<T>(Controller controller = null) where T : View
        {
            await UniTask.WaitUntil(() => _initiated);

            var viewType = typeof(T);
            bool hasBack = false;

            if (_forwardView != null && _forwardView.TryGetComponent(out View forwardView) && forwardView is T)
            {
                if (controller != null)
                    forwardView.SetController(controller);
                hasBack = forwardView.HasBack();
                SetCurrentView(hasBack, _forwardView);

            }
            else if(_viewsDictionary.ContainsKey(viewType))
            {
                var viewContainer = _viewsDictionary[viewType];
                var view = viewContainer.GetView();

                hasBack = view.HasBack();

                if (viewContainer.Instance == null)
                {
                    await viewContainer.Init(Instance.transform as RectTransform);
                }
                var viewGO = viewContainer.Instance;

                if (controller != null && viewGO != null && viewGO.TryGetComponent(out View instanceView))
                {
                    instanceView.SetController(controller);
                }

                SetCurrentView(hasBack, viewGO);
            }
        }

        private static void SetCurrentView(bool hasBack, GameObject viewGO)
        {
            if (hasBack)
            {
                if (_currentView != null)
                    _currentView.SetActive(false);
                _backStack.Push(_currentView);
            }
            else
            {
                DisposeOfCurrentView();
                _backStack.Clear();
            }

            _currentView = viewGO;
            _currentView?.SetActive(true);
        }

        private static void DisposeOfCurrentView()
        {
            if (_currentView != null)
            {
                Destroy(_currentView);
            }
        }
    }
}