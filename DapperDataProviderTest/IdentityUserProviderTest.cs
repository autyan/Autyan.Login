using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autyan.Identity.Core.Component;
using Autyan.Identity.Core.DataConfig;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Autyan.Identity.Model;
using Autyan.Identity.Service.SignIn;

namespace Autyan.Identity.DapperDataProvider.Tests
{
    [TestClass()]
    public class IdentityUserProviderTest
    {
        public IdentityUserProviderTest()
        {
            var wireUp = WireUp.Instance;
            wireUp.WormUpModel();
            MetadataContext.Instance.Initilize(AppDomain.CurrentDomain.GetAssemblies());
        }

        [TestMethod()]
        public void QueryAsyncTest()
        {
            Query();
        }

        private static void Query()
        {
            var usersQueue = new ConcurrentQueue<IdentityUser>();
            var random = new Random();
            const int threadCounts = 20;
            const int processTotal = 500000;
            var provider = new IdentityUserProvider();
            for (var i = 0; i < 25; i++)
            {
                var start = random.Next(0, 9999998);
                var users = provider.QueryAsync(new UserQuery
                {
                    IdFrom = start,
                    IdTo = start + 20000
                }).Result.ToList();
                foreach (var user in users)
                {
                    usersQueue.Enqueue(user);
                }
            }

            var watch = new Stopwatch();
            var tasks = new List<Task>();
            watch.Start();
            for (var i = 0; i < threadCounts; i++)
            {
                tasks.Add(RunQueryAsync(usersQueue));
            }

            Task.WaitAll(tasks.ToArray());

            watch.Stop();
            var avg = processTotal / (watch.Elapsed.TotalMilliseconds / 1000);
            Console.WriteLine($"useage : {watch.Elapsed.TotalMilliseconds} ms,  avg : {avg:F3} qps");
        }

        private static async Task RunQueryAsync(ConcurrentQueue<IdentityUser> queue)
        {
            while (queue.TryDequeue(out var newUser))
            {
                var provider = new IdentityUserProvider();
                var service = new SignInService(provider);
                await service.PasswordSignInAsync(newUser.LoginName, newUser.PasswordHash);
            }
        }

        [TestMethod]
        public void UpdateAsyncTest()
        {
            var provider = new IdentityUserProvider();
            var result = Task.Factory.StartNew(() => provider.UpdateByConditionAsync(new { PasswordHash = "userlogin" }, new { IdFrom = 500, IdTo = 5000 })).Result;
            Console.WriteLine(result.Result);
        }

        [TestMethod]
        public void InsertAsyncTest()
        {
            Insert();
        }

        private static void Insert()
        {
            var usersQueue = new ConcurrentQueue<IdentityUser>();
            const int threadCounts = 20;
            const int processTotal = 1000000;
            var random = new Random();
            var loginNames = new Dictionary<string, string>();
            var index = 0;
            while (index < processTotal)
            {
                var randomName = RandomString(random.Next(4, 20), random);
                if (loginNames.ContainsKey(randomName)) continue;

                loginNames.Add(randomName, randomName);
                usersQueue.Enqueue(new IdentityUser
                {
                    LoginName = randomName,
                    PasswordHash = "userlogin"
                });
                index++;
            }

            var watch = new Stopwatch();
            var tasks = new List<Task>();
            watch.Start();
            for (var i = 0; i < threadCounts; i++)
            {
                tasks.Add(RunInsertAsync(usersQueue));
            }

            Task.WaitAll(tasks.ToArray());

            watch.Stop();

            var avg = processTotal / (watch.Elapsed.TotalMilliseconds / 1000);
            Console.WriteLine($"currenttime: {DateTime.Now:HH:mm:ss,fff},useage : {watch.Elapsed.TotalMilliseconds} ms,  avg : {avg:F3} qps");
        }

        public static async Task RunInsertAsync(ConcurrentQueue<IdentityUser> usersQueue)
        {
            while (usersQueue.TryDequeue(out var user))
            {
                var provider = new IdentityUserProvider();
                var service = new SignInService(provider);
                await service.RegisterAsync(user);
            }
        }

        public static string RandomString(int length, Random random)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
