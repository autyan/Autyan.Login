using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autyan.Identity.Core.Component;
using Autyan.Identity.Core.DataConfig;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Autyan.Identity.Model;

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
            var provider = new IdentityUserProvider();
            IEnumerable<IdentityUser> queryResult;
            Task.Run(async () =>
            {
                var query = new UserQuery
                {
                    IdFrom = 5000,
                    IdTo = 50000
                };
                queryResult = await provider.QueryAsync(query);
                Assert.AreEqual(50000 - 5001, queryResult.Count());

                query = new UserQuery
                {
                    Ids = new []{1L,3,5,7,9,2,4,6,8,10}
                };
                queryResult = await provider.QueryAsync(query);
                Assert.AreEqual(10, queryResult.Count());
                provider.Dispose();
            }).GetAwaiter().GetResult();
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
            var provider = new IdentityUserProvider();
            Task.Run(async () =>
            {
                var user = new IdentityUser
                {
                    LoginName = "TempUser",
                    PasswordHash = "LoginUser",
                    UserLockoutEnabled = false
                };
                await provider.InsertAsync(user);
                var queryResult = await provider.QueryAsync(new UserQuery
                {
                    LoginName = "TempUser"
                });
                var insertedUser = queryResult.FirstOrDefault();
                Assert.IsNotNull(insertedUser);
                Assert.AreEqual(user.Id, insertedUser.Id);
                await provider.DeleteByIdAsync(insertedUser.Id);
                provider.Dispose();
            }).GetAwaiter().GetResult();
        }
    }
}
