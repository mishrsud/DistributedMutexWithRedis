using System;

namespace LockService.Core
{
    public interface ILockService : IDisposable
    {
        /// <summary>
        /// The name of the resource that will be locked for exclusive access
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns>true if lock was acquired, false otherwise</returns>
        bool TryAcquireTimedLock(string resourceName);

        /// <summary>
        /// Releases a previously acquired lock
        /// </summary>
        void ReleaseLock();
    }
}