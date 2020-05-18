// Copyright (c) AIR Pty Ltd. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using AIR.ObjectPooling;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

[TestFixture]
public class ObjectPoolTests
{
    private MockPooledGameObject _mockPooledObject;

    [SetUp]
    public void SetUp()
    {
        _mockPooledObject = new GameObject(GetType().Name)
            .AddComponent<MockPooledGameObject>();
    }

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(_mockPooledObject);

    [Test]
    public void Constructor_ForRangeOfPoolSizes_InitialPoolSizeCorrect([NUnit.Framework.Range(0, 20)] int poolSize)
    {
        // Act
        var pool = new ObjectPool<MockPooledGameObject>(_mockPooledObject, poolSize);

        // Assert
        Assert.AreEqual(poolSize, pool.PoolSize);
        pool.Dispose();
    }

    [Test]
    public void RequestNew_RequestsCountSameAsPoolSizeRange_PoolSizeDoesNotExpand(
        [NUnit.Framework.Range(1, 20)] int poolSize)
    {
        // Arrange
        var pool = new ObjectPool<MockPooledGameObject>(
            _mockPooledObject,
            poolSize,
            (po) => po.ResetCalled = true
            );

        // Act
        var createdObjects = new List<MockPooledGameObject>();
        for (int i = 0; i < poolSize; i++) {
            var newPooledObject = pool.ActivateNew();
            createdObjects.Add(newPooledObject);
        }

        // Assert
        CollectionAssert.IsNotEmpty(createdObjects);
        CollectionAssert.AllItemsAreNotNull(createdObjects);
        Assert.AreEqual(
            poolSize,
            pool.PoolSize,
            "Only one object was requested, but multiple cached in pool.");
        Assert.That(
            createdObjects.Select(co => co.ResetCalled),
            Is.All.True,
            "One or more requested objects where not active.");
        pool.Dispose();
    }

    [Test]
    public void RequestNew_RequestCountTwicePoolSize_PoolSizeDoubles([NUnit.Framework.Range(1, 20)] int poolSize)
    {
        // Arrange
        var pool = new ObjectPool<MockPooledGameObject>(_mockPooledObject, poolSize);

        // Act
        var twicePoolSize = poolSize * 2;
        for (int i = 0; i < twicePoolSize; i++)
            pool.ActivateNew();

        // Assert
        Assert.AreEqual(poolSize * 2, pool.PoolSize, "The pool's size should have doubled, but didn't");
        pool.Dispose();
    }

    [Test]
    public void Recycle_AllRequestedObjects_AllAreRecycled()
    {
        // Arrange
        const int POOL_SIZE = 10;
        var poolParent = new GameObject("PoolParent").transform;
        var pool = new ObjectPool<MockPooledGameObject>(
            _mockPooledObject,
            POOL_SIZE,
            null,
            o => o.RecycleCalled = true);
        var requestedObjects = new List<MockPooledGameObject>();
        for (int i = 0; i < POOL_SIZE; i++) {
            var pooledObject = pool.ActivateNew();
            requestedObjects.Add(pooledObject);
        }

        // Act
        foreach (var pooledObject in requestedObjects)
            pool.Recycle(pooledObject);

        // Assert
        Assert.That(
            requestedObjects.Select(ro => ro.RecycleCalled),
            Is.All.True,
            "All recycled objects should have been disabled, but where not.");
        Object.DestroyImmediate(poolParent.gameObject);
    }

    [Test]
    public void RequestNew_FreshPool_AllNewObjectsWhereNotReset()
    {
        // Arrange
        const int REQUEST_COUNT = 20;
        var pool = new ObjectPool<MockPooledGameObject>(_mockPooledObject);
        var requestedObjects = new List<MockPooledGameObject>();

        // Act
        for (int i = 0; i < REQUEST_COUNT; i++)
            requestedObjects.Add(pool.ActivateNew());

        // Assert
        Assert.That(
            requestedObjects.Select(ro => !ro.ResetCalled),
            Is.All.True,
            "Not All requested objects where reset.");
        pool.Dispose();
    }

    [Test]
    public void RequestNew_RecycledPool_AllNewObjectsWhereReset()
    {
        // Arrange
        const int REQUEST_COUNT = 20;
        var pool = new ObjectPool<MockPooledGameObject>(_mockPooledObject, 0, (o) => o.ResetCalled = true);
        var pooledObjects = new List<MockPooledGameObject>();
        for (int i = 0; i < REQUEST_COUNT; i++)
            pooledObjects.Add(pool.ActivateNew());
        foreach (var requestedObject in pooledObjects)
            pool.Recycle(requestedObject);

        // Act
        for (int i = 0; i < REQUEST_COUNT; i++)
            pooledObjects.Add(pool.ActivateNew());

        // Assert
        Assert.That(
            pooledObjects.Select(ro => ro.ResetCalled),
            Is.All.True,
            "Not All requested objects where reset.");
        pool.Dispose();
    }

    private class MockPooledGameObject : MonoBehaviour
    {
        public bool ResetCalled = false;
        public bool RecycleCalled = false;

        public void Reset()
        {
            ResetCalled = true;
        }
    }
}