using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Dapper.Contrib.Extensions;

namespace mikevh.sqrl.Repos
{
    public class User
    {
        [ExplicitKey]
        public string idk { get; set; }
        public string suk { get; set; }
        public string vuk { get; set; }

        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime LastLoggedIn { get; set; }
        public int LoginCount { get; set; }
        public int UpdateCount { get; set; }
        
        /*
CREATE TABLE Users (
	idk varchar(43) not null primary key,
	suk varchar(43) not null,
	vuk varchar(43) not null,
	Name nvarchar(255) null,
	CreatedOn DATETIME2 not null,
	UpdatedOn DATETIME2 not null,
	LastLoggedIn DATETIME2 not null,
	LoginCount INT not null,
	UpdateCount INT not null
)
         */
    }

    public interface IUserRepo
    {
        User Get(string idk);
        bool Update(User user);
        void Add(User user);
        bool Remove(User user);
    }

    public class UserRepo : IUserRepo, IDisposable
    {
        private readonly string _connectionString;

        public UserRepo(string connectionString)
        {
            _connectionString = connectionString;
            SqlMapper.AddTypeMap(typeof(DateTime), DbType.DateTime2);
        }

        private SqlConnection _connection;
        private SqlConnection Connection => _connection = _connection ?? new SqlConnection(_connectionString);

        public void Add(User user) => Connection.Insert(user);

        public bool Update(User user) => Connection.Update(user);

        public User Get(string idk) => Connection.Get<User>(idk);

        public bool Remove(User user) => Connection.Delete(user);

        public void Dispose() => _connection?.Dispose();
    }
}