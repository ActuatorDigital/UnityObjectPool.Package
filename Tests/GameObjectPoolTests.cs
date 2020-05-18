// Copyright (c) AIR Pty Ltd. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using AIR.ObjectPooling;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

[TestFixture]
public class GameObjectPoolTests
{
    private MockPoolableGameObject _mockPooledObject;

    [SetUp]
    public void SetUp() =>
        _mockPooledObject = new GameObject(nameof(MockPoolableGameObject))
            .AddComponent<MockPoolableGameObject>();

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(_mockPooledObject.gameObject);

    [Test]
    public void Constructor_ForRangeOfPoolSizes_InitialPoolSizeCorrect([NUnit.Framework.Range(0, 20)] int poolSize)
    {
        // Act
        var pool = new ComponentPool<MockPoolableGameObject>(_mockPooledObject, poolSize);

        // Assert
        Assert.AreEqual(poolSize, pool.PoolSize);
        pool.Dispose();
    }

    [Test]
    public void RequestNew_RequestsCountSameAsPoolSizeRange_PoolSizeDoesNotExpand(
        [NUnit.Framework.Range(1, 20)] int poolSize)
    {
        // Arrange
        var pool = new ComponentPool<MockPoolableGameObject>(_mockPooledObject, poolSize);

        // Act
        var createdObjects = new List<MockPoolableGameObject>();
        for (int i = 0; i < poolSize; i++) {
            var newPooledObject = pool.RequestNew();
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
            createdObjects.Select(co => co.isActiveAndEnabled),
            Is.All.True,
            "One or more requested objects where not active.");
        pool.Dispose();
    }

    [Test]
    public void RequestNew_RequestCountTwicePoolSize_PoolSizeDoubles([NUnit.Framework.Range(1, 20)] int poolSize)
    {
        // Arrange
        var pool = new ComponentPool<MockPoolableGameObject>(_mockPooledObject, poolSize);

        // Act
        var twicePoolSize = poolSize * 2;
        for (int i = 0; i < twicePoolSize; i++)
            pool.RequestNew();

        // Assert
        Assert.AreEqual(poolSize * 2, pool.PoolSize, "The pool's size should have doubled, but didn't");
        pool.Dispose();
    }

    [Test]
    public void Recycle_AllRequestedObjects_AllAreDisabled()
    {
        // Arrange
        const int POOL_SIZE = 10;
        var poolParent = new GameObject("PoolParent").transform;
        var pool = new ComponentPool<MockPoolableGameObject>(_mockPooledObject, POOL_SIZE, poolParent);
        var requestedObjects = new List<MockPoolableGameObject>();
        for (int i = 0; i < POOL_SIZE; i++) {
            var pooledObject = pool.RequestNew();
            requestedObjects.Add(pooledObject);
        }

        // Act
        foreach (var pooledObject in requestedObjects)
            pool.Recycle(pooledObject);

        // Assert
        Assert.That(
            requestedObjects.Select(ro => ro.isActiveAndEnabled),
            Is.All.False,
            "All recycled objects should have been disabled, but where not.");
        Assert.That(
            requestedObjects.Select(ro => ro.transform.parent.Equals(poolParent)),
            Is.All.True,
            "All recycled objects where not assigned to pool parent.");
        Object.DestroyImmediate(poolParent.gameObject);
    }

    [Test]
    public void RequestNew_FreshPool_AllNewObjectsWhereNotReset()
    {
        // Arrange
        const int REQUEST_COUNT = 20;
        var pool = new ComponentPool<MockPoolableGameObject>(_mockPooledObject);
        var requestedObjets = new List<MockPoolableGameObject>();

        // Act
        for (int i = 0; i < REQUEST_COUNT; i++)
            requestedObjets.Add(pool.RequestNew());

        // Assert
        Assert.That(
            requestedObjets.Select(ro => ro.FreshGo && !ro.ResetCalled),
            Is.All.True,
            "Not All requested objects where reset.");
        pool.Dispose();
    }

    [Test]
    public void RequestNew_RecycledPool_AllNewObjectsWhereReset()
    {
        // Arrange
        const int REQUEST_COUNT = 20;
        var pool = new ComponentPool<MockPoolableGameObject>(_mockPooledObject);
        var pooledObjects = new List<MockPoolableGameObject>();
        for (int i = 0; i < REQUEST_COUNT; i++)
            pooledObjects.Add(pool.RequestNew());
        foreach (var requestedObject in pooledObjects)
            pool.Recycle(requestedObject);

        // Act
        for (int i = 0; i < REQUEST_COUNT; i++)
            pooledObjects.Add(pool.RequestNew());

        // Assert
        Assert.That(
            pooledObjects.Select(ro => ro.ResetCalled),
            Is.All.True,
            "Not All requested objects where reset.");
        pool.Dispose();
    }

    private class MockPoolableGameObject : MonoBehaviour, IPoolableObject
    {
        public bool FreshGo = true;
        public bool ResetCalled = false;
        public void Reset()
        {
            FreshGo = false;
            ResetCalled = true;
        }
    }
}