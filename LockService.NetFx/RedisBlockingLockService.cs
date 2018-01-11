using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Redis;

namespace LockService.NetFx
{
    public class RedisBlockingLockService : ILockService
    {
        private int _lockTimeoutInSeconds;
        private IRedisClientsManager _redisClientManager;
        private bool _isReleased;
        private string _resourceName;

        public RedisBlockingLockService(int lockTimeoutInSeconds, IRedisClientsManager redisClientsManager = null)
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
                    while (!redisClient.SetValueIfNotExists($"{resourceName}", $"mutexAcquired:{DateTime.UtcNow}", TimeSpan.FromSeconds(_lockTimeoutInSeconds)))
                    {
                        Task.Delay(TimeSpan.FromSeconds(2)).Wait();
                    }
                    return true;
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
            if (!_isReleased)
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
