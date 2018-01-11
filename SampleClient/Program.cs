using LockService.NetFx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SampleClient
{
    class Program
    {
        private const string ResourceName = "TestResource";

        static void Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            //RunWithNonBlockingService(cancellationToken);
            RunWithBlockingService(cancellationToken);

            Console.WriteLine("Running, press enter to stop");
            Console.ReadLine();
            cancellationTokenSource.Cancel();
        }

        private static void RunWithBlockingService(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                var redisBlockingLockService = new RedisBlockingLockService(10);
                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine($"{DateTime.UtcNow:G} | Attempting to acquire lock");
                    
                    if (redisBlockingLockService.TryAcquireTimedLock(ResourceName))
                    {
                        Console.WriteLine($"{DateTime.UtcNow:G} | Lock acquired Working..");
                        Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).Wait(cancellationToken);
                        Console.WriteLine($"{DateTime.UtcNow:G} | Done..");
                        Console.WriteLine($"{DateTime.UtcNow:G} | Releasing lock");
                        redisBlockingLockService.ReleaseLock();
                        Console.Clear();
                    }
                }
            }, cancellationToken);
        }

        private static void RunWithNonBlockingService(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                var lockService = new RedisLockService(10);
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        while (!lockService.TryAcquireTimedLock(ResourceName))
                        {
                            Console.WriteLine($"{DateTime.UtcNow:G} | Attempting to acquire lock..");
                            Task.Delay(TimeSpan.FromSeconds(2), cancellationToken).Wait(cancellationToken);
                        }

                        Console.WriteLine($"{DateTime.UtcNow:G} | Lock acquired Working..");
                        Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).Wait(cancellationToken);
                    }
                    finally
                    {
                        Console.WriteLine($"{DateTime.UtcNow:G} | Releasing lock");
                        lockService.ReleaseLock();
                    }
                }
            }, cancellationToken);
        }
    }
}
