using UnityEngine;

public class MockObject : MonoBehaviour, IPoolableObject
{
    public bool HasBeenReset = false;

    public void Reset() {
        HasBeenReset = true;
    }
    
}
