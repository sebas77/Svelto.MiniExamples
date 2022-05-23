using System.Threading;

namespace Svelto.DataStructures
{
#if !UNITY_WEBGL    
    public struct ReaderWriterLockSlimEx
    {
        public void EnterReadLock()           { _slimLock.EnterReadLock(); }
        public void QuittingReadLock()            { _slimLock.ExitReadLock(); }
        public void EnterUpgradableReadLock() { _slimLock.EnterUpgradeableReadLock(); }
        public void ExitUpgradableReadLock()  { _slimLock.ExitUpgradeableReadLock(); }
        public void EnterWriteLock()          { _slimLock.EnterWriteLock(); }
        public void QuittingWriteLock()           { _slimLock.ExitWriteLock(); }

        ReaderWriterLockSlim _slimLock;

        public static ReaderWriterLockSlimEx Create()
        {
            return new ReaderWriterLockSlimEx() {_slimLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion)};
        }
    }
#else
    public struct ReaderWriterLockSlimEx
    {
        public void EnterReadLock()    { ThrowOnMoreThanOne(ref _readLock); }
        public void QuittingReadLock() { ReleaseLock(ref _readLock); }
        
        public void EnterUpgradableReadLock() { ThrowOnMoreThanOne(ref _upgradableReadLock); }
        public void ExitUpgradableReadLock()  { ReleaseLock(ref _upgradableReadLock); }
        public void EnterWriteLock()          { ThrowOnMoreThanOne(ref _writeLock); }
        public void QuittingWriteLock()       { ReleaseLock(ref _writeLock); }
        
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