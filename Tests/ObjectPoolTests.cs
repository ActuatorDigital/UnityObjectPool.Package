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
    public void TearDown()
    {
        if (_mockPooledObject != null)
            Object.DestroyImmediate(_mockPooledObject.gameObject);
    }

    [Test]
    public void Constructor_ForRangeOfPoolSizes_InitialPoolSizeCorrect([NUnit.Framework.Range(1, 20)] int poolSize)
    {
        // Act
        var pool = new ObjectPool<MockPooledGameObject>(
            _mockPooledObject,
            poolSize,
            null,
            null,
            null,
            o => Object.DestroyImmediate(o.gameObject));

        // Assert
        Assert.AreEqual(poolSize, pool.PoolSize);
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
            null,
            o => o.RetireCalled = true,
            o => Object.Destroy(o.gameObject));
        var requestedObjects = new List<MockPooledGameObject>();
        for (int i = 0; i < POOL_SIZE; i++) {
            var pooledObject = pool.ActivateNew();
            requestedObjects.Add(pooledObject);
        }

        // Act
        foreach (var pooledObject in requestedObjects)
            pool.Retire(pooledObject);

        // Assert
        Assert.That(
            requestedObjects.Select(ro => ro.RetireCalled),
            Is.All.True,
            "All recycled objects should have been disabled, but where not.");
        Object.DestroyImmediate(poolParent.gameObject);
        pool.Dispose();
    }

    [Test]
    public void RequestNew_FreshPool_AllNewObjectsWhereNotReset()
    {
        // Arrange
        const int REQUEST_COUNT = 20;
        var pool = new ObjectPool<MockPooledGameObject>(
            _mockPooledObject,
            0,
            null,
            null,
            null,
            o => Object.DestroyImmediate(o.gameObject));
        var requestedObjects = new List<MockPooledGameObject>();

        // Act
        for (int i = 0; i < REQUEST_COUNT; i++)
            requestedObjects.Add(pool.ActivateNew());

        // Assert
        Assert.That(
            requestedObjects.Select(ro => !ro.ActivateCalled),
            Is.All.True,
            "Not All requested objects where reset.");
        pool.Dispose();
    }

    [Test]
    public void RequestNew_RecycledPool_AllNewObjectsWhereReset()
    {
        // Arrange
        const int REQUEST_COUNT = 20;
        var pool = new ObjectPool<MockPooledGameObject>(
            _mockPooledObject,
            0,
            null,
            (o) => o.ActivateCalled = true,
            null,
            o => Object.DestroyImmediate(o.gameObject));
        var pooledObjects = new List<MockPooledGameObject>();
        for (int i = 0; i < REQUEST_COUNT; i++)
            pooledObjects.Add(pool.ActivateNew());
        foreach (var requestedObject in pooledObjects)
            pool.Retire(requestedObject);

        // Act
        for (int i = 0; i < REQUEST_COUNT; i++)
            pooledObjects.Add(pool.ActivateNew());

        // Assert
        Assert.That(
            pooledObjects.Select(ro => ro.ActivateCalled),
            Is.All.True,
            "Not All requested objects where reset.");
        pool.Dispose();
    }

    private class MockPooledGameObject : MonoBehaviour
    {
        public bool ActivateCalled = false;
        public bool RetireCalled = false;

        public void Reset()
        {
            ActivateCalled = true;
        }
    }
}