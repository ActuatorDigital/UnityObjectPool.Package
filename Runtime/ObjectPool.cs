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
        // private readonly List<TUnityObject> _activeObjects;
        // private readonly List<TUnityObject> _retiredObjects;
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
            // _activeObjects = new List<TUnityObject>();
            // _retiredObjects = new List<TUnityObject>();
            for (int i = 0; i < poolSize; i++) {
                var initialObject = CreateNew();
                // _retiredObjects.Add(initialObject);
                _pooledObjects.Enqueue(initialObject);
                activate?.Invoke(initialObject);
            }
        }

        public int PoolSize => _pooledObjects.Count;
            //_activeObjects.Count + _retiredObjects.Count;

        public void Dispose()
        {
            Action<TUnityObject> destroyer = _destroy ?? Object.Destroy;
            foreach (var pooledObject in _pooledObjects)
                destroyer?.Invoke(pooledObject);
            // foreach (TUnityObject activeObject in _activeObjects)
            //     destroyer?.Invoke(activeObject);
            // foreach (TUnityObject retiredObject in _retiredObjects)
            //     destroyer?.Invoke(retiredObject);

            destroyer?.Invoke(_template);
            _pooledObjects.Clear();
            // _activeObjects.Clear();
            // _retiredObjects.Clear();
        }

        public void Retire(TUnityObject retiredObject)
        {
            _pooledObjects.Enqueue(retiredObject);
            // _activeObjects.Remove(retiredObject);
            // _retiredObjects.Add(retiredObject);
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
            
            // if (!_pooledObjects.Peek()) {
            //     // if (!_retiredObjects.Any()) {
            //     var commissionedObj = CreateNew();
            //     // _activeObjects.Add(commissionedObj);
            //     _activate?.Invoke(commissionedObj);
            //     return commissionedObj;
            // }

            // var reCommissionedObject = _retiredObjects.First();
            // var reCommissionedObject = _pooledObjects.Dequeue();
            // _retiredObjects.Remove(reCommissionedObject);
            // _activeObjects.Add(reCommissionedObject);
            // _activate?.Invoke(reCommissionedObject);
            // return reCommissionedObject;
        }

        private TUnityObject CreateNew()
        {
            var newUnityObject = Object.Instantiate(_template);
            _created?.Invoke(newUnityObject);
            return newUnityObject;
        }
    }
}