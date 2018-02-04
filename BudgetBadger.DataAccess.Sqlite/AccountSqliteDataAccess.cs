using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Models;
using Microsoft.Data.Sqlite;

namespace BudgetBadger.DataAccess.Sqlite
{
    public class AccountSqliteDataAccess : IAccountDataAccess
    {
        readonly string ConnectionString;

        public AccountSqliteDataAccess(string databaseFilePath)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = databaseFilePath;
            ConnectionString = connectionStringBuilder.ConnectionString;

            Initialize();
        }

        void Initialize()
        {
            using(var db = new SqliteConnection(ConnectionString))
            {
                db.Open();
                var command = db.CreateCommand();

                command.CommandText = @"CREATE TABLE IF NOT EXISTS AccountType 
                                          ( 
                                             Id          BLOB PRIMARY KEY NOT NULL, 
                                             Description TEXT NOT NULL,
                                             CreatedDateTime  TEXT NOT NULL, 
                                             ModifiedDateTime TEXT NOT NULL, 
                                             DeletedDateTime  TEXT
                                          );
                

                                        CREATE TABLE IF NOT EXISTS Account 
                                          ( 
                                             Id               BLOB PRIMARY KEY NOT NULL, 
                                             Description      TEXT NOT NULL, 
                                             AccountTypeId    BLOB NOT NULL, 
                                             OnBudget         INTEGER NOT NULL, 
                                             Notes            TEXT, 
                                             CreatedDateTime  TEXT NOT NULL, 
                                             ModifiedDateTime TEXT NOT NULL, 
                                             DeletedDateTime  TEXT,
                                             FOREIGN KEY(AccountTypeId) REFERENCES AccountType(Id)
                                          );
                                        ";
                
                command.ExecuteNonQuery();
            }

            var dateTimeNow = DateTime.Now;

            var accountTypes = new List<AccountType>
            {
                new AccountType
                {
                    Id = new Guid("d4ec0d4e-e8c1-40ec-80d0-efb2071d56bb"),
                    Description = "Checking",
                    CreatedDateTime = dateTimeNow,
                    ModifiedDateTime = dateTimeNow
                },
                new AccountType
                {
                    Id = new Guid("d6eb0a4f-bba5-491a-976b-504a3e15dcce"),
                    Description = "Savings",
                    CreatedDateTime = dateTimeNow,
                    ModifiedDateTime = dateTimeNow
                },
                new AccountType
                {
                    Id = new Guid("c31201e4-b02a-4221-8ab0-03625641a622"),
                    Description = "Credit Card",
                    CreatedDateTime = dateTimeNow,
                    ModifiedDateTime = dateTimeNow
                }
            };
            foreach (var accountType in accountTypes)
            {
                using (var db = new SqliteConnection(ConnectionString))
                {
                    db.Open();
                    var command = db.CreateCommand();

                    command.CommandText = @"INSERT OR IGNORE INTO AccountType 
                                                    (Id, 
                                                     Description,
                                                     CreatedDateTime,
                                                     ModifiedDateTime) 
                                            VALUES  (@Id, 
                                                    @Description,
                                                    @CreatedDateTime,
                                                    @ModifiedDateTime)";

                    command.Parameters.AddWithValue("@Id", accountType.Id);
                    command.Parameters.AddWithValue("@Description", accountType.Description);
                    command.Parameters.AddWithValue("@CreatedDateTime", accountType.CreatedDateTime);
                    command.Parameters.AddWithValue("@ModifiedDateTime", accountType.ModifiedDateTime);

                    command.ExecuteNonQuery();
                }
            }
        }

        public async Task CreateAccountAsync(Account account)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"INSERT INTO Account 
                                                    (Id, 
                                                     Description, 
                                                     AccountTypeId, 
                                                     OnBudget, 
                                                     Notes, 
                                                     CreatedDateTime, 
                                                     ModifiedDateTime, 
                                                     DeletedDateTime) 
                                        VALUES     (@Id, 
                                                    @Description, 
                                                    @AccountTypeId, 
                                                    @OnBudget, 
                                                    @Notes, 
                                                    @CreatedDateTime, 
                                                    @ModifiedDateTime, 
                                                    @DeletedDateTime)";
                
                command.Parameters.AddWithValue("@Id", account.Id);
                command.Parameters.AddWithValue("@Description", account.Description);
                command.Parameters.AddWithValue("@AccountTypeId", account.Type?.Id);
                command.Parameters.AddWithValue("@OnBudget", account.OnBudget);
                command.Parameters.AddWithValue("@Notes", account.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedDateTime", account.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", account.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", account.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAccountAsync(Guid id)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"DELETE Account WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<Account> ReadAccountAsync(Guid id)
        {
            var account = new Account();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT AC.Id, 
                                               AC.Description, 
                                               AC.AccountTypeId, 
                                               AC.OnBudget, 
                                               AC.Notes, 
                                               AC.CreatedDateTime, 
                                               AC.ModifiedDateTime, 
                                               AC.DeletedDateTime, 
                                               ACT.Id               AS AccountTypeId, 
                                               ACT.Description      AS AccountTypeDescription, 
                                               ACT.CreatedDateTime  AS AccountTypeCreatedDateTime, 
                                               ACT.ModifiedDateTime AS AccountTypeModifiedDateTime, 
                                               ACT.DeletedDateTime  AS AccountTypeDeletedDateTime
                                        FROM   Account AS AC 
                                        JOIN   AccountType AS ACT ON AC.AccountTypeId = ACT.Id
                                        WHERE  AC.Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        account = new Account
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Description = reader["Description"].ToString(),
                            Type = new AccountType
                            {
                                Id = new Guid(reader["AccountTypeId"] as byte[]),
                                Description = reader["AccountTypeDescription"].ToString(),
                                CreatedDateTime = Convert.ToDateTime(reader["AccountTypeCreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["AccountTypeModifiedDateTime"]),
                                DeletedDateTime = reader["AccountTypeDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["AccountTypeDeletedDateTime"])
                            },
                            OnBudget = Convert.ToBoolean(reader["OnBudget"]),
                            Notes = reader["Notes"].ToString(),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                        };
                    }
                }
            }

            return account;
        }

        public async Task<IEnumerable<Account>> ReadAccountsAsync()
        {
            var accounts = new List<Account>();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT A.Id, 
                                               A.Description, 
                                               A.AccountTypeId, 
                                               A.OnBudget, 
                                               A.Notes, 
                                               A.CreatedDateTime, 
                                               A.ModifiedDateTime, 
                                               A.DeletedDateTime, 
                                               ACT.Id               AS AccountTypeId, 
                                               ACT.Description      AS AccountTypeDescription, 
                                               ACT.CreatedDateTime  AS AccountTypeCreatedDateTime, 
                                               ACT.ModifiedDateTime AS AccountTypeModifiedDateTime, 
                                               ACT.DeletedDateTime  AS AccountTypeDeletedDateTime
                                        FROM   Account AS A 
                                        JOIN   AccountType AS ACT ON A.AccountTypeId = ACT.Id";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        accounts.Add(new Account
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Description = reader["Description"].ToString(),
                            Type = new AccountType
                            {
                                Id = new Guid(reader["AccountTypeId"] as byte[]),
                                Description = reader["AccountTypeDescription"].ToString(),
                                CreatedDateTime = Convert.ToDateTime(reader["AccountTypeCreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["AccountTypeModifiedDateTime"]),
                                DeletedDateTime = reader["AccountTypeDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["AccountTypeDeletedDateTime"])
                            },
                            OnBudget = Convert.ToBoolean(reader["OnBudget"]),
                            Notes = reader["Notes"].ToString(),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                        });
                    }
                }
            }

            return accounts;
        }

        public async Task<IEnumerable<AccountType>> ReadAccountTypesAsync()
        {
            var accountTypes = new List<AccountType>();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT Id, 
                                               Description, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   AccountType";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        var account = new AccountType
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Description = reader["Description"].ToString(),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                        };
                        accountTypes.Add(account);
                    }
                }
            }

            return accountTypes;
        }

        public async Task UpdateAccountAsync(Account account)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"UPDATE Account 
                                        SET    Description = @Description, 
                                               AccountTypeId = @AccountTypeId, 
                                               OnBudget = @OnBudget, 
                                               Notes = @Notes, 
                                               CreatedDateTime = @CreatedDateTime, 
                                               ModifiedDateTime = @ModifiedDateTime, 
                                               DeletedDateTime = @DeletedDateTime 
                                        WHERE  Id = @Id ";

                command.Parameters.AddWithValue("@Id", account.Id);
                command.Parameters.AddWithValue("@Description", account.Description);
                command.Parameters.AddWithValue("@AccountTypeId", account.Type.Id);
                command.Parameters.AddWithValue("@OnBudget", account.OnBudget);
                command.Parameters.AddWithValue("@Notes", account.Notes);
                command.Parameters.AddWithValue("@CreatedDateTime", account.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", account.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", account.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task CreateAccountTypeAsync(AccountType accountType)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"INSERT INTO AccountType 
                                                    (Id, 
                                                     Description, 
                                                     CreatedDateTime, 
                                                     ModifiedDateTime, 
                                                     DeletedDateTime)  
                                        VALUES     (@Id, 
                                                    @Description, 
                                                    @CreatedDateTime, 
                                                    @ModifiedDateTime, 
                                                    @DeletedDateTime)";

                command.Parameters.AddWithValue("@Id", accountType.Id);
                command.Parameters.AddWithValue("@Description", accountType.Description);
                command.Parameters.AddWithValue("@CreatedDateTime", accountType.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", accountType.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", accountType.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAccountTypeAsync(AccountType accountType)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"UPDATE Account 
                                        SET    Description = @Description, 
                                               CreatedDateTime = @CreatedDateTime, 
                                               ModifiedDateTime = @ModifiedDateTime, 
                                               DeletedDateTime = @DeletedDateTime 
                                        WHERE  Id = @Id ";

                command.Parameters.AddWithValue("@Id", accountType.Id);
                command.Parameters.AddWithValue("@Description", accountType.Description);
                command.Parameters.AddWithValue("@CreatedDateTime", accountType.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", accountType.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", accountType.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAccountTypeAsync(Guid id)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"DELETE AccountType WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
