#if UNITY_5_3_OR_NEWER || UNITY_5
using UnityEngine;

namespace Svelto.ObjectPool
{
    public class GameObjectPool : ObjectPool<GameObject>
    {
#if POOL_DEBUGGER
    public GameObjectPool()
    {
        GameObject poolDebugger = new GameObject("GameObjectPoolDebugger");

        poolDebugger.AddComponent<PoolDebugger>().SetPool(this);
    }
#endif
        public override void OnDispose()
        {
            for (var enumerator = _pools.GetEnumerator(); enumerator.MoveNext();)
                foreach (var obj in enumerator.Current.Value)
                    GameObject.Destroy(obj);
            
            for (var enumerator = _namedPools.GetEnumerator(); enumerator.MoveNext();)
                foreach (var obj in enumerator.Current.Value)
                    GameObject.Destroy(obj);
        }
    }
}
#endif