using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autyan.Identity.Core.Component;
using Autyan.Identity.Core.DataConfig;
using Autyan.Identity.DapperDataProvider;
using Autyan.Identity.Model;
using Autyan.Identity.Service.SignIn;

namespace DataGenerater
{
    internal static class Program
    {
        private static readonly Random Random = new Random();

        static void Main()
        {
            QueryTest();
            //GenerateData();
            Console.ReadKey();
        }

        private static void QueryTest()
        {
            var wireUp = WireUp.Instance;
            wireUp.WormUpModel();
            MetadataContext.Instance.Initilize(AppDomain.CurrentDomain.GetAssemblies());
            var infoQueue = new ConcurrentQueue<ProcessInfomatin>();
            var userQueue = new Queue<Queue<IdentityUser>>();
            const int threadCounts = 10;
            const int threadProcss = 2000;
            var random = new Random();
            using (var provider = new IdentityUserProvider())
            {
                for (var i = 0; i < threadCounts; i++)
                {
                    var threadUserQueue = new Queue<IdentityUser>();
                    for (var j = 0; j < threadProcss; j++)
                    {
                        var user = provider.FirstOrDefaultAsync(new UserQuery
                        {
                            Id = random.Next(0, 9999998)
                        }).GetAwaiter().GetResult();
                        threadUserQueue.Enqueue(user);
                    }
                    userQueue.Enqueue(threadUserQueue);
                }
            }

            for (var i = 0; i < threadCounts; i++)
            {
                Task.Factory.StartNew(async () => await RunQueryAsync(userQueue.Dequeue(), infoQueue, threadProcss));
            }

            while (infoQueue.Count < threadCounts)
            {

            }

            var infoList = infoQueue.ToList();
            var avg = infoList.Sum(i => i.AvgTps);
            Console.WriteLine($"tps : {avg:F3} ");
        }

        private static async Task RunQueryAsync(Queue<IdentityUser> userQueue, ConcurrentQueue<ProcessInfomatin> queue, int processCount)
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            var usage = TimeSpan.Zero;
            using (var provider = new IdentityUserProvider())
            {
                var service = new SignInService(provider);
                var start = DateTime.Now;
                while (userQueue.Count > 0)
                {
                    var queryUser = userQueue.Dequeue();
                    var result = await service.PasswordSignInAsync(queryUser.LoginName, queryUser.PasswordHash);
                }

                usage = DateTime.Now - start;
                var tps = processCount / usage.TotalSeconds;
                queue.Enqueue(new ProcessInfomatin
                {
                    AvgTps = tps
                });
                Console.WriteLine($"thread:{id} end, useage: {usage.TotalMilliseconds} ms, TPS : {tps:F3} ");
            }
        }

        private static void GenerateData()
        {
            var loginNames = new Dictionary<string, string>();
            using (var file = new StreamWriter("c:\\generate.txt"))
            {
                var count = 1;
                while (count < 10000000)
                {
                    string loginName;
                    do
                    {
                        var length = Random.Next(8, 25);
                        loginName = RandomString(length);
                    } while (loginNames.ContainsKey(loginName));
                    loginNames.Add(loginName, null);
                    file.WriteLine($"{count},{loginName},userlogin,0,,,{DateTime.Now:yyyy-MM-dd HH:mm:ss},,");
                    count++;
                }
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }

    internal class ProcessInfomatin
    {
        public double AvgTps { get; set; }
    }
}
