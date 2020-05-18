// Copyright (c) AIR Pty Ltd. All rights reserved.

using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace AIR.ObjectPooling
{
    public class ObjectPool<TUnityObject> : IDisposable
        where TUnityObject : Object
    {
        private readonly List<TPooledObject<TUnityObject>> _pooledObjects;
        private readonly TUnityObject _template;
        private readonly Action<TUnityObject> _activate;
        private readonly Action<TUnityObject> _retire;
        private readonly Action<TUnityObject> _destroy;

        public ObjectPool(
            TUnityObject template,
            int poolSize = 0,
            Action<TUnityObject> activate = null,
            Action<TUnityObject> retire = null,
            Action<TUnityObject> destroy = null)
        {
            _pooledObjects = new List<TPooledObject<TUnityObject>>();
            for (int i = 0; i < poolSize; i++) {
                var newObj = GrowPool();
                activate?.Invoke(newObj.Inner);
            }

            _template = template;
            _activate = activate;
            _retire = retire;
            _destroy = destroy;
        }

        public int PoolSize => _pooledObjects.Count;

        public void Dispose()
        {
            Action<TUnityObject> destroyer = _destroy ?? Object.Destroy;
            foreach (TPooledObject<TUnityObject> pooledObject in _pooledObjects)
                destroyer?.Invoke(pooledObject.Inner);

            destroyer?.Invoke(_template);
            _pooledObjects.Clear();
        }

        public void Recycle(TUnityObject retiredObject)
        {
            for (var i = 0; i < _pooledObjects.Count; i++) {
                var pooledObject = _pooledObjects[i];
                if (pooledObject.Inner.Equals(retiredObject)) {
                    pooledObject.Retire();
                    _retire?.Invoke(pooledObject.Inner);
                    break;
                }
            }
        }

        public TUnityObject ActivateNew()
        {
            for (var i = 0; i < _pooledObjects.Count; i++) {
                var pooledObject = _pooledObjects[i];
                if (pooledObject.IsRetired) {
                    pooledObject.Reactivate();
                    _activate?.Invoke(pooledObject.Inner);
                    return pooledObject.Inner;
                }
            }

            var commissionedObj = GrowPool();
            commissionedObj.Reactivate();
            _activate?.Invoke(commissionedObj.Inner);
            return commissionedObj.Inner;
        }

        private TPooledObject<TUnityObject> GrowPool()
        {
            var newUnityObject = Object.Instantiate(_template);
            var newPooledObject = new TPooledObject<TUnityObject>(newUnityObject);
            _pooledObjects.Add(newPooledObject);
            return newPooledObject;
        }

        private class TPooledObject<TInnerObject>
        {
            public readonly TInnerObject Inner;

            public TPooledObject(TInnerObject inner)
            {
                Inner = inner;
                IsRetired = true;
            }

            public bool IsRetired { get; private set; }
            public void Retire() => IsRetired = true;
            public void Reactivate() => IsRetired = false;
        }
    }
}