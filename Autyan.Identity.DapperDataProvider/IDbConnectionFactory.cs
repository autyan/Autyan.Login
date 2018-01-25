using System.Data;

namespace Autyan.Identity.DapperDataProvider
{
    public interface IDbConnectionFactory
    {
        IDbConnection GetConnection(string name);
    }
}
