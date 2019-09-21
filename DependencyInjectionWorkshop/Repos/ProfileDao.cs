using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Repos
{
    public interface IProfile
    {
        string GetPasswordFromDb(string accountId);
    }

    public class ProfileDao : IProfile
    {
        public string GetPasswordFromDb(string accountId)
        {
            string passwordFromDb;

            // Get Password
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return passwordFromDb;
        }
    }
}