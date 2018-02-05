using System.Text;
using System.Threading.Tasks;
using Autyan.Identity.Model;
using Autyan.Identity.Model.DataProvider;
using Dapper;

namespace Autyan.Identity.DapperDataProvider
{
    public class IdentityUserProvider : BaseDapperDataProvider<IdentityUser, UserQuery>, IIdentityUserProvider
    {
        protected override void AppendWhere(StringBuilder builder, UserQuery query)
        {
            base.AppendWhere(builder, query);
            if (query.LoginName != null)
            {
                builder.Append(" AND LoginName = @LoginName");
            }

            if (query.UserLockoutEnabled != null)
            {
                builder.Append(" AND UserLockoutEnabled = @UserLockoutEnabled");
            }

            if (query.UserLockoutEndAtFrom != null)
            {
                builder.Append(" AND  UserLockoutEndAt > UserLockoutEndAtFrom");
            }

            if (query.UserLockoutEndAtTo != null)
            {
                builder.Append(" AND  UserLockoutEndAt < UserLockoutEndAtTo");
            }
        }
    }
}
