//Block-scoped namespace (traditional) as it lasted till C# 9
namespace Services
{
    public static class ServerService
    {
        private static int _count;
        private static readonly ReaderWriterLockSlim _lock = new();

        public static int GetCount()
        {
            //Any reader calling this will only be able to enter
            //if writer releases the lock (ExitWriteLock())
            //but still allows multiple readers in parallel
            _lock.EnterReadLock();

            try
            {
                return _count;
            }
            //Finally is needed so that the lock is guaranteed to be released
            //in case if something goes wrong
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public static void AddToCount(int value)
        {
            //Second writer must wait till first one releases the lock
            //so only one writer at a time
            _lock.EnterWriteLock();

            try
            {
                _count += value;
            }
            //Finally is needed so that the lock is guaranteed to be released
            //in case if something goes wrong
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        //For testing
        public static void Reset()
        {
            _lock.EnterWriteLock();

            try
            {
                _count = 0;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        //For testing (readers cannot call GetCount while there is a writer)
        public static void AddToCountTest(int value)
        {
            _lock.EnterWriteLock();

            try
            {
                //Pausing a thread to simulate long write operation
                Thread.Sleep(1000); 
                _count += value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
    }
}