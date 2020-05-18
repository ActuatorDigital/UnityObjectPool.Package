// Copyright (c) AIR Pty Ltd. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AIR.ObjectPooling
{
    public class ComponentPool<T> : IDisposable, IObjectPool<T>
        where T : Component, IPoolableObject
    {
        private readonly Transform _poolParent;
        private readonly List<T> _pooledObjects;
        private readonly T _template;

        public ComponentPool(T template, int poolSize = 0, Transform parent = null)
        {
            _pooledObjects = new List<T>();
            _template = template;
            _poolParent = parent == null
                ? new GameObject(typeof(T).FullName + "_Pool").transform
                : parent;

            for (int i = 0; i < poolSize; i++) {
                var newObj = GrowPool();
                newObj.gameObject.SetActive(false);
                newObj.transform.parent = _poolParent;
            }
        }

        public int PoolSize => _pooledObjects.Count;

        public void Recycle(T retiredObject)
        {
            retiredObject.transform.parent = _poolParent;
            retiredObject.gameObject.SetActive(false);
        }

        public T RequestNew()
        {
            foreach (T obj in _pooledObjects) {
                var isRetired =
                    !obj.gameObject.activeInHierarchy &&
                    obj.transform.parent == _poolParent;

                if (isRetired) {
                    var nextFreeObj = obj;
                    nextFreeObj.transform.parent = null;
                    nextFreeObj.gameObject.SetActive(true);
                    nextFreeObj.Reset();
                    return nextFreeObj;
                }
            }

            return GrowPool();
        }

        public void Dispose()
        {
            Object.Destroy(_poolParent.gameObject);
            _pooledObjects.Clear();
        }

        private T GrowPool()
        {
            var newPoolObject = Object.Instantiate(_template);
            _pooledObjects.Add(newPoolObject);
            return newPoolObject;
        }
    }
}