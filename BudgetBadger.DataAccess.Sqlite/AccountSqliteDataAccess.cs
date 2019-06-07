using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Files;
using BudgetBadger.Models;
using Microsoft.Data.Sqlite;

namespace BudgetBadger.DataAccess.Sqlite
{
    public class AccountSqliteDataAccess : IAccountDataAccess
    {
        readonly string _connectionString;

        public AccountSqliteDataAccess(string connectionString)
        {
            _connectionString = connectionString;

            Initialize();
        }

        async void Initialize()
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"CREATE TABLE IF NOT EXISTS Account 
                                      ( 
                                         Id               BLOB PRIMARY KEY NOT NULL, 
                                         Description      TEXT NOT NULL, 
                                         OnBudget         INTEGER NOT NULL, 
                                         Notes            TEXT, 
                                         CreatedDateTime  TEXT NOT NULL, 
                                         ModifiedDateTime TEXT NOT NULL, 
                                         DeletedDateTime  TEXT
                                      );
                                    ";

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task CreateAccountAsync(Account account)
        {
            using(await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"INSERT INTO Account 
                                                (Id, 
                                                 Description, 
                                                 OnBudget, 
                                                 Notes, 
                                                 CreatedDateTime, 
                                                 ModifiedDateTime, 
                                                 DeletedDateTime) 
                                    VALUES     (@Id, 
                                                @Description, 
                                                @OnBudget, 
                                                @Notes, 
                                                @CreatedDateTime, 
                                                @ModifiedDateTime, 
                                                @DeletedDateTime)";

                        command.Parameters.AddWithValue("@Id", account.Id);
                        command.Parameters.AddWithValue("@Description", account.Description);
                        command.Parameters.AddWithValue("@OnBudget", account.OnBudget);
                        command.Parameters.AddWithValue("@Notes", account.Notes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedDateTime", account.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", account.ModifiedDateTime);
                        command.Parameters.AddWithValue("@DeletedDateTime", account.DeletedDateTime ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                });
            }
            
        }

        public async Task DeleteAccountAsync(Guid id)
        {
            using(await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"DELETE Account WHERE Id = @Id";

                        command.Parameters.AddWithValue("@Id", id);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task<Account> ReadAccountAsync(Guid id)
        {
            using(await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var account = new Account();
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT AC.Id, 
                                           AC.Description, 
                                           AC.OnBudget, 
                                           AC.Notes, 
                                           AC.CreatedDateTime, 
                                           AC.ModifiedDateTime, 
                                           AC.DeletedDateTime
                                    FROM   Account AS AC 
                                    WHERE  AC.Id = @Id";

                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                account = new Account
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Description = reader["Description"].ToString(),
                                    OnBudget = Convert.ToBoolean(reader["OnBudget"], CultureInfo.InvariantCulture),
                                    Notes = reader["Notes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture)
                                };
                            }
                        }
                    }

                    return account;
                });
            }
            
        }

        public async Task<IReadOnlyList<Account>> ReadAccountsAsync()
        {
            using(await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var accounts = new List<Account>();
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT A.Id, 
                                           A.Description, 
                                           A.OnBudget, 
                                           A.Notes, 
                                           A.CreatedDateTime, 
                                           A.ModifiedDateTime, 
                                           A.DeletedDateTime
                                    FROM   Account AS A";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                accounts.Add(new Account
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Description = reader["Description"].ToString(),
                                    OnBudget = Convert.ToBoolean(reader["OnBudget"], CultureInfo.InvariantCulture),
                                    Notes = reader["Notes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture)
                                });
                            }
                        }
                    }

                    return accounts;
                });
            }
            
        }

        public async Task UpdateAccountAsync(Account account)
        {
            using(await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"UPDATE Account 
                                    SET    Description = @Description, 
                                           OnBudget = @OnBudget, 
                                           Notes = @Notes, 
                                           CreatedDateTime = @CreatedDateTime, 
                                           ModifiedDateTime = @ModifiedDateTime, 
                                           DeletedDateTime = @DeletedDateTime 
                                    WHERE  Id = @Id ";

                        command.Parameters.AddWithValue("@Id", account.Id);
                        command.Parameters.AddWithValue("@Description", account.Description);
                        command.Parameters.AddWithValue("@OnBudget", account.OnBudget);
                        command.Parameters.AddWithValue("@Notes", account.Notes);
                        command.Parameters.AddWithValue("@CreatedDateTime", account.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", account.ModifiedDateTime);
                        command.Parameters.AddWithValue("@DeletedDateTime", account.DeletedDateTime ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task<int> GetAccountsCountAsync()
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var count = 0;
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT COUNT(*)
                                    FROM   Account";

                        object result = command.ExecuteScalar();
                        result = (result == DBNull.Value) ? null : result;
                        count = Convert.ToInt32(result, CultureInfo.InvariantCulture);
                    }

                    return count;
                });
            }
        }
    }
}
