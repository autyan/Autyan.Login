using Autyan.Identity.Model;
using Autyan.Identity.Model.DataProvider;

namespace Autyan.Identity.DapperDataProvider
{
    public class IdentityUserProvider : BaseDapperDataProvider<IdentityUser, UserQuery>, IIdentityUserProvider
    {
    }
}
