using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour, IPoolableObject {

    Transform _poolParent;
    List<T> _objects;
    T _prefab;

    public int PoolSize { get { return _objects.Count;  } }

    public ObjectPool(int poolSize, T prefab){

        _poolParent = new GameObject(typeof(T).FullName + "_Pool").transform;
        _objects = new List<T>();
        _prefab = prefab;

        for (int i = 0; i < poolSize; i++) {
            var newObj = GrowPool();
            newObj.gameObject.SetActive(false);
            newObj.transform.parent = _poolParent;
        }

    }

    public void Recycle(T retiredObject) {
        retiredObject.transform.parent = _poolParent;
        retiredObject.gameObject.SetActive(false);
    }

    public T GetNew() {

        foreach (MonoBehaviour obj in _objects) {

            var isRetired =
                !obj.gameObject.activeInHierarchy &&
                obj.transform.parent == _poolParent;

            if (isRetired) {
                var nextFreeObj = obj as T;
                nextFreeObj.transform.parent = null;
                nextFreeObj.gameObject.SetActive(true);
                (nextFreeObj as IPoolableObject).Reset();
                return nextFreeObj;
            }

        }

        return GrowPool();
    }

    private T GrowPool() {
        var newPoolObject = Object.Instantiate(_prefab);
        _objects.Add(newPoolObject);
        return newPoolObject;
    }
}

public interface IPoolableObject {
    void Reset();
}