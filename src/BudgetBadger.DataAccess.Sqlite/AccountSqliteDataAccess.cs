using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BudgetBadger.Core.Utilities;
using BudgetBadger.Core.Models;
using Microsoft.Data.Sqlite;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Dtos;
using System.Linq;

namespace BudgetBadger.DataAccess.Sqlite
{
    public partial class SqliteDataAccess
    {
        public async Task CreateAccountAsync(Account account)
        {
            using (await MultiThreadLock.UseWaitAsync())
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
                                                 DeletedDateTime,
                                                 HiddenDateTime) 
                                    VALUES     (@Id, 
                                                @Description, 
                                                @OnBudget, 
                                                @Notes, 
                                                @CreatedDateTime, 
                                                @ModifiedDateTime, 
                                                @DeletedDateTime,
                                                @HiddenDateTime)";

                        command.Parameters.AddWithValue("@Id", account.Id.ToByteArray());
                        command.Parameters.AddWithValue("@Description", account.Description);
                        command.Parameters.AddWithValue("@OnBudget", account.OnBudget);
                        command.Parameters.AddWithValue("@Notes", account.Notes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedDateTime", account.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", account.ModifiedDateTime);
                        command.Parameters.AddWithValue("@DeletedDateTime", account.DeletedDateTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@HiddenDateTime", account.HiddenDateTime ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                });
            }
            
        }

        public async Task<Account> ReadAccountAsync(Guid id)
        {
            using (await MultiThreadLock.UseWaitAsync())
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
                                           AC.DeletedDateTime,
                                           AC.HiddenDateTime
                                    FROM   Account AS AC 
                                    WHERE  AC.Id = @Id";

                        command.Parameters.AddWithValue("@Id", id.ToByteArray());

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
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture),
                                    HiddenDateTime = reader["HiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["HiddenDateTime"], CultureInfo.InvariantCulture)
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
            using (await MultiThreadLock.UseWaitAsync())
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
                                           A.DeletedDateTime,
                                           A.HiddenDateTime
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
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture),
                                    HiddenDateTime = reader["HiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["HiddenDateTime"], CultureInfo.InvariantCulture)
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
            using (await MultiThreadLock.UseWaitAsync())
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
                                           DeletedDateTime = @DeletedDateTime,
                                           HiddenDateTime = @HiddenDateTime
                                    WHERE  Id = @Id ";

                        command.Parameters.AddWithValue("@Id", account.Id.ToByteArray());
                        command.Parameters.AddWithValue("@Description", account.Description);
                        command.Parameters.AddWithValue("@OnBudget", account.OnBudget);
                        command.Parameters.AddWithValue("@Notes", account.Notes);
                        command.Parameters.AddWithValue("@CreatedDateTime", account.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", account.ModifiedDateTime);
                        command.Parameters.AddWithValue("@DeletedDateTime", account.DeletedDateTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@HiddenDateTime", account.HiddenDateTime ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task CreateAccountDtoAsync(AccountDto account)
        {
            using (await MultiThreadLock.UseWaitAsync())
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
                                                             DeletedDateTime,
                                                             HiddenDateTime) 
                                                VALUES     (@Id, 
                                                            @Description,
                                                            @OnBudget,
                                                            @Notes, 
                                                            @CreatedDateTime, 
                                                            @ModifiedDateTime, 
                                                            @DeletedDateTime,
                                                            @HiddenDateTime)";
                        command.AddParameter("@Id", account.Id.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@Description", account.Description, SqliteType.Text);
                        command.AddParameter("@OnBudget", account.OnBudget, SqliteType.Integer);
                        command.AddParameter("@Notes", account.Notes, SqliteType.Text);
                        command.AddParameter("@CreatedDateTime", DateTime.Now, SqliteType.Text);
                        command.AddParameter("@ModifiedDateTime", account.ModifiedDateTime, SqliteType.Text);
                        command.AddParameter("@DeletedDateTime", account.Deleted ? DateTime.Now : null, SqliteType.Text);
                        command.AddParameter("@HiddenDateTime", account.Hidden ? DateTime.Now : null, SqliteType.Text);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task<IReadOnlyList<AccountDto>> ReadAccountDtosAsync(IEnumerable<Guid> accountIds)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var accounts = new List<AccountDto>();

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
                                                A.DeletedDateTime,
                                                A.HiddenDateTime
                                         FROM   Account AS A";

                        if (accountIds != null)
                        {
                            var ids = accountIds.Select(p => p.ToByteArray()).ToList();
                            if (ids.Any())
                            {
                                var parameters = command.AddParameters("@Id", ids, SqliteType.Blob);
                                command.CommandText += $" WHERE Id IN ({parameters})";
                            }
                            else
                            {
                                return accounts.AsReadOnly();
                            }
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var account = new AccountDto
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Description = reader["Description"].ToString(),
                                    OnBudget = Convert.ToBoolean(reader["OnBudget"], CultureInfo.InvariantCulture),
                                    Notes = reader["Notes"] == DBNull.Value ? (string)null : reader["Notes"].ToString(),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    Deleted = reader["DeletedDateTime"] != DBNull.Value,
                                    Hidden = reader["HiddenDateTime"] != DBNull.Value
                                };
                                accounts.Add(account);
                            }
                        }
                    }

                    return accounts.AsReadOnly();
                });
            }
        }

        public async Task UpdateAccountDtoAsync(AccountDto account)
        {
            using (await MultiThreadLock.UseWaitAsync())
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
                                                       ModifiedDateTime = @ModifiedDateTime, 
                                                       DeletedDateTime = @DeletedDateTime,
                                                       HiddenDateTime = @HiddenDateTime
                                                WHERE  Id = @Id ";
                        command.AddParameter("@Id", account.Id.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@Description", account.Description, SqliteType.Text);
                        command.AddParameter("@OnBudget", account.OnBudget, SqliteType.Integer);
                        command.AddParameter("@Notes", account.Notes, SqliteType.Text);
                        command.AddParameter("@ModifiedDateTime", account.ModifiedDateTime, SqliteType.Text);
                        command.AddParameter("@DeletedDateTime", account.Deleted ? DateTime.Now : null, SqliteType.Text);
                        command.AddParameter("@HiddenDateTime", account.Hidden ? DateTime.Now : null, SqliteType.Text);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }
    }
}
