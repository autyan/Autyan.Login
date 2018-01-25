using Autyan.Identity.Core.DataProvider;

namespace Autyan.Identity.Model.DataProvider
{
    public interface IIdentityUserProvider : IDataProvider<IdentityUser, UserQuery>
    {
    }
}
