// Copyright (c) AIR Pty Ltd. All rights reserved.

using System;
using UnityEngine.UIElements;

namespace AIR.ObjectPooling
{
    public class ObjectPool<T> : IObjectPool<T>
        where T : IPoolableObject
    {
        private T[] _pooledObjects;

        public ObjectPool(int poolSize, T prototype) => throw new NotImplementedException();

        public int PoolSize { get; set; }

        public T RequestNew() => throw new NotImplementedException();

        public void Recycle(T retiredObject) => throw new NotImplementedException();
    }
}