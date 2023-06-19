using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace FriendsSystem
{
    // This example spans a random number of ParticleSystems using a pool so that old systems can be reused.
    public class Pooler<T> : MonoBehaviour where T : MonoBehaviour
    {
        // Collection checks will throw errors if we try to release an item that is already in the pool.
        public bool collectionChecks = true;
        [SerializeField] private T _prefab;
        [SerializeField] private Transform _parentTransform;
        private IObjectPool<T> m_Pool;

        public IObjectPool<T> Pool
        {
            get
            {
                if (m_Pool == null)
                {
                    m_Pool = new ObjectPool<T>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 10);
                }
                return m_Pool;
            }
        }

        private T CreatePooledItem()
        {
            Instantiate(_prefab, _parentTransform).TryGetComponent(out T item);
            return item;
        }

        // Called when an item is returned to the pool using Release
        private void OnReturnedToPool(T pooledObj)
        {
            pooledObj.gameObject.SetActive(false);
        }

        // Called when an item is taken from the pool using Get
        private void OnTakeFromPool(T pooledObj)
        {
            pooledObj.gameObject.SetActive(true);
        }

        // If the pool capacity is reached then any items returned will be destroyed.
        // We can control what the destroy behavior does, here we destroy the GameObject.
        private void OnDestroyPoolObject(T pooledObj)
        {
            Destroy(pooledObj.gameObject);
        }

        public void Release(T pooledObj)
        {
            Pool.Release(pooledObj);
        }
    }
}