using System.Runtime.CompilerServices;
using System.Threading;

namespace Svelto.DataStructures
{
#if !UNITY_WEBGL    
    public struct ReaderWriterLockSlimEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterReadLock()           { _slimLock.EnterReadLock(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitReadLock()            { _slimLock.ExitReadLock(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterUpgradableReadLock() { _slimLock.EnterUpgradeableReadLock(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitUpgradableReadLock()  { _slimLock.ExitUpgradeableReadLock(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterWriteLock()          { _slimLock.EnterWriteLock(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitWriteLock()           { _slimLock.ExitWriteLock(); }

        ReaderWriterLockSlim _slimLock;

        public static ReaderWriterLockSlimEx Create()
        {
            return new ReaderWriterLockSlimEx() {_slimLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion)};
        }
    }
#else
//ATTENTION THIS IS NOT A REAL IMPLEMENTATION, WEBGL DOES NOT SUPPORT MULTITHREADING
    public struct ReaderWriterLockSlimEx
    {
        public void EnterReadLock() { ThrowOnMoreThanOne(ref _readLock); }
        public void ExitReadLock()  { ReleaseLock(ref _readLock); }
        
        public void EnterUpgradableReadLock() { ThrowOnMoreThanOne(ref _upgradableReadLock); }
        public void ExitUpgradableReadLock()  { ReleaseLock(ref _upgradableReadLock); }
        public void EnterWriteLock()          { ThrowOnMoreThanOne(ref _writeLock); }
        public void ExitWriteLock()           { ReleaseLock(ref _writeLock); }
        
        void ThrowOnMoreThanOne(ref int readLock)
        {
            if (_readLock++ > 0) throw new System.Exception("RecursiveLock not supported");
        }

        void ReleaseLock(ref int readLock) { --readLock; }

        int _readLock;
        int _upgradableReadLock;
        int _writeLock;

        public static ReaderWriterLockSlimEx Create() { return new ReaderWriterLockSlimEx(); }
    }
#endif
}