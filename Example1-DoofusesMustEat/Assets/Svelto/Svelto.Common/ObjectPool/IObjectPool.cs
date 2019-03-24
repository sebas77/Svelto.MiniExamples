using System;

namespace Svelto.ObjectPool
{
    public interface IObjectPool<T>
    {
        void Recycle(T go, int    pool);
        void Recycle(T go, string poolName);

        void Preallocate(string poolName, int size, Func<T> onFirstUse);
        void Preallocate(int    pool,     int size, Func<T> onFirstUse);

        T Use(string poolName, Func<T> onFirstUse = null);
        T Use(int    pool,     Func<T> onFirstUse = null);
    }
}