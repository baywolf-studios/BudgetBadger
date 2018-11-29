using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Files;
using BudgetBadger.Models;
using Microsoft.Data.Sqlite;

namespace BudgetBadger.DataAccess.Sqlite
{
    public class AccountSqliteDataAccess : IAccountDataAccess
    {
        readonly SqliteConnection _connection;

        public AccountSqliteDataAccess(SqliteConnection connection)
        {
            _connection = connection;

            Initialize();
        }

        void Initialize()
        {
            try
            {
                _connection.Open();
                var command = _connection.CreateCommand();

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
            finally
            {
                _connection.Close();
            }
        }

        public async Task CreateAccountAsync(Account account)
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await Task.Run(() =>
                {
                    _connection.Open();
                    var command = _connection.CreateCommand();

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
                });
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }
        }

        public async Task DeleteAccountAsync(Guid id)
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await Task.Run(() =>
                {
                    _connection.Open();
                    var command = _connection.CreateCommand();

                    command.CommandText = @"DELETE Account WHERE Id = @Id";

                    command.Parameters.AddWithValue("@Id", id);

                    command.ExecuteNonQuery();
                });
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task<Account> ReadAccountAsync(Guid id)
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                return await Task.Run(() =>
                {
                    var account = new Account();
                    _connection.Open();
                    var command = _connection.CreateCommand();

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
                                OnBudget = Convert.ToBoolean(reader["OnBudget"]),
                                Notes = reader["Notes"].ToString(),
                                CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                                DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                            };
                        }
                    }

                    return account;
                });
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }
        }

        public async Task<IReadOnlyList<Account>> ReadAccountsAsync()
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                return await Task.Run(() =>
                {
                    var accounts = new List<Account>();
                    _connection.Open();
                    var command = _connection.CreateCommand();

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
                                OnBudget = Convert.ToBoolean(reader["OnBudget"]),
                                Notes = reader["Notes"].ToString(),
                                CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                                DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                            });
                        }
                    }

                    return accounts;
                });
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }
        }

        public async Task UpdateAccountAsync(Account account)
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await Task.Run(() =>
                {
                    _connection.Open();
                    var command = _connection.CreateCommand();

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
                });
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }
        }
    }
}
