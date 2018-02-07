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
            var resultsQueue = new ConcurrentQueue<SignInResult>();
            var random = new Random();
            const int threadCounts = 20;
            const int processTotal = 500000;
            using (var provider = new IdentityUserProvider())
            {
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
            }

            var watch = new Stopwatch();
            var tasks = new List<Task>();
            watch.Start();
            for (var i = 0; i < threadCounts; i++)
            {
                tasks.Add(RunQueryAsync(usersQueue, resultsQueue));
            }

            Task.WaitAll(tasks.ToArray());

            watch.Stop();
            var avg = processTotal / (watch.Elapsed.TotalMilliseconds / 1000);
            Console.WriteLine($"useage : {watch.Elapsed.TotalMilliseconds} ms,  avg : {avg:F3} qps, callCount : {resultsQueue.Count}, successCount : {resultsQueue.Count(r => r.Succeed)}");
        }

        private static async Task RunQueryAsync(ConcurrentQueue<IdentityUser> queue, ConcurrentQueue<SignInResult> resultsQueue)
        {
            while (queue.TryDequeue(out var newUser))
            {
                using (var provider = new IdentityUserProvider())
                {
                    var service = new SignInService(provider);
                    var result = await service.PasswordSignInAsync(newUser.LoginName, newUser.PasswordHash);
                    //resultsQueue.Enqueue(result);
                }
            }
        }

        [TestMethod]
        public void UpdateAsyncTest()
        {
            var passwordHash = "CallMeLazy";
            var provider = new IdentityUserProvider();
            IEnumerable<IdentityUser> queryResult;
            Task.Run(async () =>
            {
                var query = new UserQuery
                {
                    Id = 582739
                };
                queryResult = await provider.QueryAsync(query);
                var user = queryResult.First();
                Assert.IsNotNull(user);
                user.PasswordHash = passwordHash;
                var modifyTimeBeforeUpdate = user.ModifiedAt;
                await provider.UpdateByIdAsync(user);
                queryResult = await provider.QueryAsync(query);
                var userUpdated = queryResult.First();
                Assert.IsNotNull(userUpdated);
                Assert.AreEqual(userUpdated.PasswordHash, passwordHash);
                Assert.AreNotEqual(modifyTimeBeforeUpdate, userUpdated.ModifiedAt);
                provider.Dispose();
            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void InsertAsyncTest()
        {
            Insert();
        }

        private static void Insert()
        {
            var usersQueue = new ConcurrentQueue<IdentityUser>();
            var resultsQueue = new ConcurrentQueue<SignInResult>();
            const int threadCounts = 20;
            const int processTotal = 1000000;
            var random = new Random();
            var loginNames = new Dictionary<string, string>();
            using (var provider = new IdentityUserProvider())
            {
                var index = 0;
                while (index < processTotal)
                {
                    var randomName = RandomString(random.Next(4, 20), random);
                    if (loginNames.ContainsKey(randomName)) continue;
                    //var user = provider.FirstOrDefaultAsync(new UserQuery
                    //{
                    //    LoginName = randomName
                    //}).Result;
                    //if (user != null) continue;

                    loginNames.Add(randomName, randomName);
                    usersQueue.Enqueue(new IdentityUser
                    {
                        LoginName = randomName,
                        PasswordHash = "userlogin"
                    });
                    index++;
                }
            }

            var watch = new Stopwatch();
            var tasks = new List<Task>();
            watch.Start();
            for (var i = 0; i < threadCounts; i++)
            {
                tasks.Add(RunInsertAsync(usersQueue, resultsQueue));
            }

            Task.WaitAll(tasks.ToArray());

            watch.Stop();

            var avg = processTotal / (watch.Elapsed.TotalMilliseconds / 1000);
            Console.WriteLine($"currenttime: {DateTime.Now:HH:mm:ss,fff},useage : {watch.Elapsed.TotalMilliseconds} ms,  avg : {avg:F3} qps, callCount : {resultsQueue.Count}, successCount : {resultsQueue.Count(r => r.Succeed)}");
        }

        public static async Task RunInsertAsync(ConcurrentQueue<IdentityUser> usersQueue, ConcurrentQueue<SignInResult> resultQueue)
        {
            while (usersQueue.TryDequeue(out var user))
            {
                using (var provider = new IdentityUserProvider())
                {
                    var service = new SignInService(provider);
                    var result = await service.RegisterAsync(user);
                    //if (result.Succeed)
                    //{
                    //    resultQueue.Enqueue(result);
                    //}
                }
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
