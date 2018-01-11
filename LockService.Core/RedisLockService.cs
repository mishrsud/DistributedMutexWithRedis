using System;
using ServiceStack.Redis;

namespace LockService.Core
{
    // TODO: This isn't production ready yet
    public class RedisLockService : ILockService
    {
        private readonly int _lockTimeoutInSeconds;
        private bool _isReleased = false;
        private readonly IRedisClientsManager _redisClientManager;
        private string _resourceName;

        public RedisLockService(int lockTimeoutInSeconds, IRedisClientsManager redisClientsManager = null)
        {
            _lockTimeoutInSeconds = lockTimeoutInSeconds;
            _redisClientManager = redisClientsManager ?? new RedisManagerPool("localhost:6379");
        }

        public bool TryAcquireTimedLock(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                throw new ArgumentException("A resource name is required", nameof(resourceName));
            }

            _resourceName = resourceName;

            using (var redisClient = _redisClientManager.GetClient() as RedisClient)
            {
                if (redisClient == null) return false;

                try
                {
                    // TODO: The timeout parameter
                    if (redisClient.SetValueIfNotExists($"{resourceName}", $"mutexAcquired:{DateTime.UtcNow}", TimeSpan.FromSeconds(_lockTimeoutInSeconds)))
                    {
                        return true;
                    }
                    
                    return false;
                }
                catch (Exception)
                {
                    // LOG
                    return false;
                } 
            }
        }

        public void Dispose()
        {
            if (_isReleased)
            {
                using (var redisClient = _redisClientManager.GetClient() as RedisClient)
                {
                    redisClient?.Remove(_resourceName);
                }
            }

            _isReleased = true;
        }

        public void ReleaseLock()
        {
            Dispose();
        }
    }
}