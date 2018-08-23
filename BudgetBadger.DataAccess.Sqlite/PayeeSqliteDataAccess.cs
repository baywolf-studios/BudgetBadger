using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Files;
using BudgetBadger.Models;
using Microsoft.Data.Sqlite;

namespace BudgetBadger.DataAccess.Sqlite
{
    public class PayeeSqliteDataAccess : IPayeeDataAccess
    {
        readonly string _connectionString;

        public PayeeSqliteDataAccess(string connectionString)
        {
            _connectionString = connectionString;

            Initialize();
        }

        void Initialize()
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
                                                    @DeletedDateTime)";

                command.Parameters.AddWithValue("@Id", Constants.StartingBalancePayee.Id);
                command.Parameters.AddWithValue("@Description", Constants.StartingBalancePayee.Description);
                command.Parameters.AddWithValue("@Notes", Constants.StartingBalancePayee.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedDateTime", Constants.StartingBalancePayee.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", Constants.StartingBalancePayee.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", Constants.StartingBalancePayee.DeletedDateTime ?? (object)DBNull.Value);

                command.ExecuteNonQuery();
            }
        }

        public async Task CreatePayeeAsync(Payee payee)
        {
            using (var db = new SqliteConnection(_connectionString))
            {
                await db.OpenAsync().ConfigureAwait(false);
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

                command.Parameters.AddWithValue("@Id", payee.Id);
                command.Parameters.AddWithValue("@Description", payee.Description);
                command.Parameters.AddWithValue("@Notes", payee.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedDateTime", payee.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", payee.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", payee.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task DeletePayeeAsync(Guid id)
        {
            using (var db = new SqliteConnection(_connectionString))
            {
                await db.OpenAsync().ConfigureAwait(false);
                var command = db.CreateCommand();

                command.CommandText = @"DELETE Payee WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task<Payee> ReadPayeeAsync(Guid id)
        {
            var payee = new Payee();

            using (var db = new SqliteConnection(_connectionString))
            {
                await db.OpenAsync().ConfigureAwait(false);
                var command = db.CreateCommand();

                command.CommandText = @"SELECT Id, 
                                               Description,
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   Payee 
                                        WHERE  Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (reader.Read())
                    {
                        payee = new Payee
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Description = reader["Description"].ToString(),
                            Notes = reader["Notes"].ToString(),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                        };
                    }
                }
            }

            return payee;
        }

        public async Task<IReadOnlyList<Payee>> ReadPayeesAsync()
        {
            var payees = new List<Payee>();

            using (var db = new SqliteConnection(_connectionString))
            {
                await db.OpenAsync().ConfigureAwait(false);
                var command = db.CreateCommand();

                command.CommandText = @"SELECT Id, 
                                               Description, 
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   Payee";

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (reader.Read())
                    {
                        var payee = new Payee
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Description = reader["Description"].ToString(),
                            Notes = reader["Notes"].ToString(),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                        };
                        payees.Add(payee);
                    }
                }
            }

            return payees;
        }

        public async Task UpdatePayeeAsync(Payee payee)
        {
            using (var db = new SqliteConnection(_connectionString))
            {
                await db.OpenAsync().ConfigureAwait(false);
                var command = db.CreateCommand();

                command.CommandText = @"UPDATE Payee 
                                        SET    Description = @Description,
                                               Notes = @Notes, 
                                               CreatedDateTime = @CreatedDateTime, 
                                               ModifiedDateTime = @ModifiedDateTime, 
                                               DeletedDateTime = @DeletedDateTime 
                                        WHERE  Id = @Id ";

                command.Parameters.AddWithValue("@Id", payee.Id);
                command.Parameters.AddWithValue("@Description", payee.Description);
                command.Parameters.AddWithValue("@Notes", payee.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedDateTime", payee.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", payee.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", payee.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }
    }
}
