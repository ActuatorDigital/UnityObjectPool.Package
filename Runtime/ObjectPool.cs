// Copyright (c) AIR Pty Ltd. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace AIR.ObjectPooling
{

    public class ObjectPool<TUnityObject> : IDisposable
        where TUnityObject : Object
    {
        private readonly Queue<TUnityObject> _pooledObjects;
        private readonly TUnityObject _template;

        private readonly Action<TUnityObject> _activate;
        private readonly Action<TUnityObject> _retire;
        private readonly Action<TUnityObject> _destroy;
        private readonly Action<TUnityObject> _created;

        public ObjectPool(
            TUnityObject template,
            int poolSize = 0,
            Action<TUnityObject> created = null,
            Action<TUnityObject> activate = null,
            Action<TUnityObject> retire = null,
            Action<TUnityObject> destroy = null)
        {
            _template = template;
            _activate = activate;
            _retire = retire;
            _destroy = destroy;
            _created = created;

            _pooledObjects = new Queue<TUnityObject>();
            for (int i = 0; i < poolSize; i++) {
                var initialObject = CreateNew();
                _pooledObjects.Enqueue(initialObject);
                activate?.Invoke(initialObject);
            }
        }

        public int PoolSize => _pooledObjects.Count;

        public void Dispose()
        {
            Action<TUnityObject> destroyer = _destroy ?? Object.Destroy;
            foreach (var pooledObject in _pooledObjects)
                destroyer?.Invoke(pooledObject);

            destroyer?.Invoke(_template);
            _pooledObjects.Clear();
        }

        public void Retire(TUnityObject retiredObject)
        {
            _pooledObjects.Enqueue(retiredObject);
            _retire?.Invoke(retiredObject);
        }

        public TUnityObject ActivateNew()
        {

            if (_pooledObjects.Any()) {
                var reCommissionedObject = _pooledObjects.Dequeue();
                _activate?.Invoke(reCommissionedObject);
                return reCommissionedObject;
            }

            var commissionedObj = CreateNew();
            _activate?.Invoke(commissionedObj);
            return commissionedObj;
        }

        private TUnityObject CreateNew()
        {
            var newUnityObject = Object.Instantiate(_template);
            _created?.Invoke(newUnityObject);
            return newUnityObject;
        }
    }
}