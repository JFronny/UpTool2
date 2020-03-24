using System;
using System.Threading;

namespace Installer
{
    public static class MutexLock
    {
        private static Mutex _mutex;
        private static bool _hasHandle;

        public static void Lock()
        {
            _mutex = new Mutex(false,
                "Global\\{c0c1e002-9e13-4e8f-a035-dbdc5128e00e}",
                out bool _);
            _hasHandle = false;
            try
            {
                _hasHandle = _mutex.WaitOne(5000, false);
                if (_hasHandle)
                    return;
                throw new MutexLockLockedException();
            }
            catch (AbandonedMutexException)
            {
#if DEBUG
                Debug.WriteLine("Mutex abandoned");
#endif
                _hasHandle = true;
            }
        }

        public static void Unlock()
        {
            if (_hasHandle)
                _mutex.ReleaseMutex();
        }
    }

    public class MutexLockLockedException : Exception
    {
    }
}