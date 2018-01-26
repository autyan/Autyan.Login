using System;
using Autyan.Identity.Core.Data;

namespace Autyan.Identity.Model
{
    /// <summary>
    /// basic user model
    /// </summary>
    public class IdentityUser : BaseEntity
    {
        public virtual string LoginName { get; set; }

        public virtual string PasswordHash { get; set; }

        //public virtual string PasswordHashSalt { get; set; }

        public virtual bool? UserLockoutEnabled { get; set; }

        public virtual DateTime? UserLockoutEndAt { get; set; }

        public virtual int? FailedLoginCount { get; set; }
    }

    public class UserQuery : BaseQuery<IdentityUser>
    {
        public string LoginName { get; set; }

        public bool? UserLockoutEnabled { get; set; }

        public DateTime? UserLockoutEndAtFrom { get; set; }

        public DateTime? UserLockoutEndAtTo { get; set; }
    }
}
