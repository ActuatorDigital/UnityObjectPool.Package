// Copyright (c) AIR Pty Ltd. All rights reserved.

using AIR.ObjectPooling;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

[Ignore("Not Implemented")]
[TestFixture]
public class ObjectPoolTests
{
    [Test]
    public void CanGetObject()
    {
        // Arrange
        var prefab = GetMockObjectPrefab();
        var objectPool = new ObjectPool<MockObject>(1, prefab);

        // Act
        var objectInstance = objectPool.RequestNew();

        // Assert
        Assert.IsNotNull(objectInstance);
    }

    [Test]
    public void ObjectPoolInitializes()
    {
        // Arrange
        const int POOL_SIZE = 100;
        var prefab = GetMockObjectPrefab();

        // Act
        var objectPool = new ObjectPool<MockObject>(POOL_SIZE, prefab);

        // Assert
        Assert.AreEqual(POOL_SIZE, objectPool.PoolSize);
    }

    [Test]
    public void ObjectPoolGrowsDynamically()
    {
        // Arrange
        var prefab = GetMockObjectPrefab();
        const int POOL_SIZE = 1, EXPANDED_SIZE = 2;
        var objectPool = new ObjectPool<MockObject>(POOL_SIZE, prefab);

        // Act
        for (int i = 0; i < 2; i++)
            objectPool.RequestNew();

        // Assert
        Assert.AreNotEqual(POOL_SIZE, objectPool.PoolSize);
        Assert.AreEqual(EXPANDED_SIZE, objectPool.PoolSize);
    }

    [Test]
    public void CanRecycleObject()
    {
        // Arrange
        var prefab = GetMockObjectPrefab();
        const int POOL_SIZE = 1;
        var objectPool = new ObjectPool<MockObject>(POOL_SIZE, prefab);
        var pooledObject = objectPool.RequestNew();

        // Act
        objectPool.Recycle(pooledObject);

        // Assert
        Assert.IsFalse(pooledObject.isActiveAndEnabled);
        Assert.IsNotNull(pooledObject.transform.parent);
    }

    private MockObject GetMockObjectPrefab() =>
        AssetDatabase.LoadAssetAtPath<MockObject>(
                "Packages/com.air.objectpooling/Tests/MockObject.prefab");

    private class MockObject : MonoBehaviour, IPoolableObject
    {
        public bool HasBeenReset = false;

        public void Reset() => HasBeenReset = true;
    }
}