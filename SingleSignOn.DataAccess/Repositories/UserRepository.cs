using Dapper;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SingleSignOn.DataAccess.Repositories
{
    public class UserRepository<T>
    {
        public IConfiguration Configuration { get; }

        public UserRepository(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<IEnumerable<T>> GetUsers()
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("SingleSignOn")))
            {
                var users = await db.QueryAsync<T>("SELECT * FROM AspNetUsers");

                return users;
            }
        }

        public async Task<IEnumerable<T>> Get(int id)
        {
            using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("SingleSignOn")))
            {
                var user = await db.QueryAsync<T>("SELECT * FROM Users WHERE Id = @id", new { id });

                return user;
            }
        }

        //public async Task<T> Create(ApplicationUser user)
        //{
        //    using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("SingleSignOn")))
        //    {
        //        var sqlQuery = "INSERT INTO Users (Name, Age) VALUES(@Name, @Age); SELECT CAST(SCOPE_IDENTITY() as int)";
        //        var userId = db.Query<int>(sqlQuery, user).FirstOrDefault();
        //        user.Id = userId;
        //    }
        //    return user;
        //}

        //public void Update(ApplicationUser user)
        //{
        //    using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("SingleSignOn")))
        //    {
        //        var sqlQuery = "UPDATE Users SET Name = @Name, Age = @Age WHERE Id = @Id";
        //        db.Execute(sqlQuery, user);
        //    }
        //}

        //public void Delete(int id)
        //{
        //    using (IDbConnection db = new SqlConnection(Configuration.GetConnectionString("SingleSignOn")))
        //    {
        //        var sqlQuery = "DELETE FROM Users WHERE Id = @id";
        //        db.Execute(sqlQuery, new { id });
        //    }
        //}
    }
}
