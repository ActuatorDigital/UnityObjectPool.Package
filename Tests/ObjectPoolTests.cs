using NUnit.Framework;
using UnityEditor;

namespace Tests {
    public class ObjectPoolTests {

        MockObject GetMockObjectPrefab() {
            return AssetDatabase
                .LoadAssetAtPath<MockObject>(
                    "Assets/Tests/MockObject.prefab" );
        }

        [Test]
        public void CanGetObject() {

            // Arrange
            var prefab = GetMockObjectPrefab();
            var objectPool = new ObjectPool<MockObject>(1, prefab);

            // Act
            var objectInstance = objectPool.GetNew();

            // Assert
            Assert.IsNotNull(objectInstance);
        }

        [Test]
        public void ObjectPoolInitializes() {

            // Arrange
            const int POOL_SIZE = 100;
            var prefab = GetMockObjectPrefab();

            // Act
            var objectPool = new ObjectPool<MockObject>(POOL_SIZE, prefab);

            // Assert
            Assert.AreEqual(POOL_SIZE, objectPool.PoolSize); 
        }

        [Test]
        public void ObjectPoolGrowsDynamically() {
            // Arrange
            var prefab = GetMockObjectPrefab();
            const int POOL_SIZE = 1, EXPANDED_SIZE = 2;
            var objectPool = new ObjectPool<MockObject>(POOL_SIZE, prefab);

            // Act
            for (int i = 0; i < 2; i++) 
                objectPool.GetNew();

            // Assert
            Assert.AreNotEqual(POOL_SIZE, objectPool.PoolSize);
            Assert.AreEqual(EXPANDED_SIZE, objectPool.PoolSize);
        }

        [Test]
        public void CanRecycleObject() {
            // Arrange
            var prefab = GetMockObjectPrefab();
            const int POOL_SIZE = 1;
            var objectPool = new ObjectPool<MockObject>(POOL_SIZE, prefab);
            var pooledObject = objectPool.GetNew();

            // Act
            objectPool.Recycle(pooledObject);

            // Assert
            Assert.IsFalse(pooledObject.isActiveAndEnabled);
            Assert.IsNotNull(pooledObject.transform.parent);
        }
    }
}
