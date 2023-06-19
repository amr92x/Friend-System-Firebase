using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace FriendsSystem
{
    public class ViewContainer : MonoBehaviour
    {
        [SerializeField] private AssetReferenceGameObject _viewPrefab;
        [SerializeField] private View _view; // is the same prefab as the viewPrefab but for getting the type
        private AsyncOperationHandle<GameObject> opHandle;
        public GameObject Instance { get; private set; }

        public async UniTask Init(RectTransform parent)
        {
            opHandle = _viewPrefab.InstantiateAsync(parent);
            await opHandle.Task;
            if (opHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Instance = opHandle.Result;
                var rectT = Instance.GetComponent<RectTransform>();
                rectT.offsetMin = Vector2.zero;
                rectT.offsetMax = Vector2.zero;
                Instance.SetActive(false);
            }
        }

        public View GetView()
        {
            return _view;
        }

        public Type GetTypeOfView()
        {
            return _view.GetType();
        }

        protected void OnDestroy()
        {
             Addressables.Release(opHandle);
        }
    }
}