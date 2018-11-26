﻿using System;
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
        readonly SqliteConnection _connection;

        public PayeeSqliteDataAccess(SqliteConnection connection)
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


                command = _connection.CreateCommand();

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
            finally
            {
                _connection.Close();
            }
        }

        public async Task CreatePayeeAsync(Payee payee)
        {
            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

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
            finally
            {
                _connection.Close();
            }
        }

        public async Task DeletePayeeAsync(Guid id)
        {
            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"DELETE Payee WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task<Payee> ReadPayeeAsync(Guid id)
        {
            var payee = new Payee();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

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
            finally
            {
                _connection.Close();
            }

            return payee;
        }

        public async Task<IReadOnlyList<Payee>> ReadPayeesAsync()
        {
            var payees = new List<Payee>();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

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
            finally
            {
                _connection.Close();
            }

            return payees;
        }

        public async Task UpdatePayeeAsync(Payee payee)
        {
            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

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
            finally
            {
                _connection.Close();
            }
        }
    }
}
