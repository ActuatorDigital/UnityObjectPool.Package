// Copyright (c) AIR Pty Ltd. All rights reserved.

namespace AIR.ObjectPooling
{
    public interface IObjectPool<T>
        where T : IPoolableObject
    {
        T RequestNew();
        void Recycle(T retiredObject);
    }
}