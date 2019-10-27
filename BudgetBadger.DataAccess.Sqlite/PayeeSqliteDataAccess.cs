using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Files;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Models;
using Microsoft.Data.Sqlite;

namespace BudgetBadger.DataAccess.Sqlite
{
    public class PayeeSqliteDataAccess : IPayeeDataAccess
    {
        readonly string _connectionString;
        readonly IResourceContainer _resourceContainer;

        public PayeeSqliteDataAccess(string connectionString,
            IResourceContainer resourceContainer)
        {
            _connectionString = connectionString;
            _resourceContainer = resourceContainer;

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
                        command.CommandText = @"CREATE TABLE IF NOT EXISTS Payee 
                                          ( 
                                             Id               BLOB PRIMARY KEY NOT NULL, 
                                             Description      TEXT NOT NULL, 
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

            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"INSERT OR IGNORE INTO Payee 
                                                    (Id, 
                                                     Description, 
                                                     Notes, 
                                                     CreatedDateTime, 
                                                     ModifiedDateTime, 
                                                     DeletedDateTime) 
                                        VALUES     (@Id, 
                                                    @Description, 
                                                    @Notes, 
                                                    @CreatedDateTime, 
                                                    @ModifiedDateTime, 
                                                    @DeletedDateTime);

                                        UPDATE Payee
                                        SET    Description = @Description
                                        WHERE  Id = @Id;";

                        command.Parameters.AddWithValue("@Id", Constants.StartingBalancePayee.Id.ToByteArray());
                        command.Parameters.AddWithValue("@Description", _resourceContainer.GetResourceString(nameof(Constants.StartingBalancePayee)));
                        command.Parameters.AddWithValue("@Notes", Constants.StartingBalancePayee.Notes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedDateTime", Constants.StartingBalancePayee.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", Constants.StartingBalancePayee.ModifiedDateTime);
                        command.Parameters.AddWithValue("@DeletedDateTime", Constants.StartingBalancePayee.DeletedDateTime ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task CreatePayeeAsync(Payee payee)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"INSERT INTO Payee 
                                                    (Id, 
                                                     Description, 
                                                     Notes, 
                                                     CreatedDateTime, 
                                                     ModifiedDateTime, 
                                                     DeletedDateTime) 
                                        VALUES     (@Id, 
                                                    @Description, 
                                                    @Notes, 
                                                    @CreatedDateTime, 
                                                    @ModifiedDateTime, 
                                                    @DeletedDateTime)";

                        command.Parameters.AddWithValue("@Id", payee.Id.ToByteArray());
                        command.Parameters.AddWithValue("@Description", payee.Description);
                        command.Parameters.AddWithValue("@Notes", payee.Notes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedDateTime", payee.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", payee.ModifiedDateTime);
                        command.Parameters.AddWithValue("@DeletedDateTime", payee.DeletedDateTime ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task DeletePayeeAsync(Guid id)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"DELETE Payee WHERE Id = @Id";

                        command.Parameters.AddWithValue("@Id", id.ToByteArray());

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task<Payee> ReadPayeeAsync(Guid id)
        {
            using(await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var payee = new Payee();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT Id, 
                                               Description,
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   Payee 
                                        WHERE  Id = @Id";

                        command.Parameters.AddWithValue("@Id", id.ToByteArray());

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                payee = new Payee
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Description = reader["Description"].ToString(),
                                    Notes = reader["Notes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture)
                                };
                            }
                        }
                    }

                    return payee;
                });
            }
        }

        public async Task<IReadOnlyList<Payee>> ReadPayeesAsync()
        {
            using(await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var payees = new List<Payee>();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT Id, 
                                               Description, 
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   Payee";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var payee = new Payee
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Description = reader["Description"].ToString(),
                                    Notes = reader["Notes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture)
                                };
                                payees.Add(payee);
                            }
                        }
                    }

                    return payees;
                });
            }
        }

        public async Task UpdatePayeeAsync(Payee payee)
        {
            using(await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"UPDATE Payee 
                                        SET    Description = @Description,
                                               Notes = @Notes, 
                                               CreatedDateTime = @CreatedDateTime, 
                                               ModifiedDateTime = @ModifiedDateTime, 
                                               DeletedDateTime = @DeletedDateTime 
                                        WHERE  Id = @Id ";

                        command.Parameters.AddWithValue("@Id", payee.Id.ToByteArray());
                        command.Parameters.AddWithValue("@Description", payee.Description);
                        command.Parameters.AddWithValue("@Notes", payee.Notes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedDateTime", payee.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", payee.ModifiedDateTime);
                        command.Parameters.AddWithValue("@DeletedDateTime", payee.DeletedDateTime ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task<int> GetPayeesCountAsync()
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
                                    FROM   Payee";

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
