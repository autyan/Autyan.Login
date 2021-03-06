﻿using System;
using System.Threading.Tasks;
using Autyan.Identity.Core.Extension;
using Autyan.Identity.Model;
using Autyan.Identity.Model.DataProvider;

namespace Autyan.Identity.Service.SignIn
{
    public class SignInService : BaseService, ISignInService
    {
        private readonly IIdentityUserProvider _userProvider;

        public SignInService(IIdentityUserProvider userProvider)
        {
            _userProvider = userProvider;
        }

        public virtual SignInResult Register(IdentityUser user)
        {
            if (_userProvider.FirstOrDefault(new
            {
                user.LoginName
            }) != null)
            {
                return SignInResult.Failed("用户已存在", SignInErrors.USER_EXISTS);
            }
            _userProvider.Insert(user);

            return SignInResult.Success();
        }

        public virtual async Task<SignInResult> RegisterAsync(IdentityUser user)
        {
            var result = await _userProvider.FirstOrDefaultAsync(new
            {
                user.LoginName
            });
            if (result != null)
            {
                return SignInResult.Failed("用户已存在", SignInErrors.USER_EXISTS);
            }
            await _userProvider.InsertAsync(user);

            return SignInResult.Success();
        }

        public virtual SignInResult PasswordSignIn(string userName, string password)
        {
            var user = _userProvider.FirstOrDefault(new
            {
                LoginName = userName
            });
            if (user == null)
            {
                return SignInResult.Failed("无效的用户名", SignInErrors.USER_NOT_FOUND);
            }

            if (user.UserLockoutEnabled == true && user.UserLockoutEndAt != null)
            {
                return SignInResult.Failed($"用户登陆已锁定至{user.UserLockoutEndAt.Value:yyyy-MM-dd HH:mm}，请在解锁后尝试登陆。",
                    SignInErrors.USER_IS_LOCKED);
            }

            if (user.UserLockoutEnabled == true)
            {
                return SignInResult.Failed("用户登陆已锁定，请在稍后尝试登陆。", SignInErrors.USER_IS_lOCKED_TO_DATE);
            }

            if (user.PasswordHash != password)
            {
                return SignInResult.Failed("无效的密码", SignInErrors.PASSWORD_NOT_MATCH);
            }

            return SignInResult.Success();
        }

        public virtual async Task<SignInResult> PasswordSignInAsync(string userName, string password)
        {
            var user = await _userProvider.FirstOrDefaultAsync(new
            {
                LoginName = userName
            });
            if (user == null)
            {
                return SignInResult.Failed("无效的用户名", SignInErrors.USER_NOT_FOUND);
            }

            if (user.PasswordHash != password)
            {
                if (user.FailedLoginCount++ > 5)
                {
                    await UserLoginLock(user);
                }

                _userProvider.UpdateById(user);
                return SignInResult.Failed("无效的密码", SignInErrors.PASSWORD_NOT_MATCH);
            }

            return SignInResult.Success();
        }

        public virtual async Task<SignInResult> UserLoginLock(long userId)
        {
            var user = await _userProvider.FirstOrDefaultAsync(new
            {
                Id = userId
            });

            if (user == null)
            {
                return SignInResult.Failed("用户ID不存在", SignInErrors.USER_NOT_FOUND);
            }

            return await UserLoginLock(user);
        }

        private async Task<SignInResult> UserLoginLock(IdentityUser user)
        {
            user.UserLockoutEnabled = true;
            user.UserLockoutEndAt = DateTime.Now.AddHours(12);
            await _userProvider.UpdateByIdAsync(user);

            return SignInResult.Success();
        }
    }
}
