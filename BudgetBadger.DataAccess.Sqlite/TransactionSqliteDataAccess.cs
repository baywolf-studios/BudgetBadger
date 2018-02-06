using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Files;
using BudgetBadger.Models;
using Microsoft.Data.Sqlite;

namespace BudgetBadger.DataAccess.Sqlite
{
    public class TransactionSqliteDataAccess : ITransactionDataAccess
    {
        readonly string ConnectionString;

        public TransactionSqliteDataAccess(IFileInfo databaseFile)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = databaseFile.FullName;
            ConnectionString = connectionStringBuilder.ConnectionString;

            Initialize();
        }

        void Initialize()
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                db.Open();
                var command = db.CreateCommand();

                command.CommandText = @"CREATE TABLE IF NOT EXISTS [Transaction]
                                          ( 
                                             Id                 BLOB PRIMARY KEY NOT NULL, 
                                             Amount             TEXT NOT NULL, 
                                             Posted             INTEGER NOT NULL,
                                             ReconciledDateTime TEXT,
                                             AccountId          BLOB NOT NULL, 
                                             PayeeId            BLOB NOT NULL, 
                                             EnvelopeId         BLOB NOT NULL,
                                             LinkedId           BLOB, 
                                             ServiceDate        TEXT NOT NULL, 
                                             Notes              TEXT, 
                                             CreatedDateTime    TEXT NOT NULL, 
                                             ModifiedDateTime   TEXT NOT NULL, 
                                             DeletedDateTime    TEXT 
                                          );
                                        ";

                command.ExecuteNonQuery();
            }
        }

        public async Task CreateTransactionAsync(Transaction transaction)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"INSERT INTO [Transaction]
                                                    (Id, 
                                                     Amount, 
                                                     Posted,
                                                     ReconciledDateTime, 
                                                     AccountId, 
                                                     PayeeId, 
                                                     EnvelopeId, 
                                                     LinkedId,
                                                     ServiceDate, 
                                                     Notes, 
                                                     CreatedDateTime, 
                                                     ModifiedDateTime, 
                                                     DeletedDateTime) 
                                        VALUES      (@Id, 
                                                     @Amount, 
                                                     @Posted,
                                                     @ReconciledDateTime, 
                                                     @AccountId, 
                                                     @PayeeId, 
                                                     @EnvelopeId, 
                                                     @LinkedId,
                                                     @ServiceDate, 
                                                     @Notes, 
                                                     @CreatedDateTime, 
                                                     @ModifiedDateTime, 
                                                     @DeletedDateTime) ";

                command.Parameters.AddWithValue("@Id", transaction.Id);
                command.Parameters.AddWithValue("@Amount", transaction.Amount);
                command.Parameters.AddWithValue("@Posted", transaction.Posted);
                command.Parameters.AddWithValue("@ReconciledDateTime", transaction.ReconciledDateTime ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AccountId", transaction.Account?.Id);
                command.Parameters.AddWithValue("@PayeeId", transaction.Payee?.Id);
                command.Parameters.AddWithValue("@EnvelopeId", transaction.Envelope?.Id);
                command.Parameters.AddWithValue("@LinkedId", transaction.LinkedId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ServiceDate", transaction.ServiceDate);
                command.Parameters.AddWithValue("@Notes", transaction.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedDateTime", transaction.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", transaction.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", transaction.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<Transaction> ReadTransactionAsync(Guid id)
        {
            var transaction = new Transaction();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT Id, 
                                               Amount, 
                                               Posted,
                                               ReconciledDateTime, 
                                               AccountId, 
                                               PayeeId, 
                                               EnvelopeId, 
                                               LinkedId,
                                               ServiceDate, 
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   [Transaction]
                                        WHERE  Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        transaction = new Transaction
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            Posted = Convert.ToBoolean(reader["Posted"]),
                            ReconciledDateTime = reader["ReconciledDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReconciledDateTime"]),
                            Account = new Account { Id = new Guid(reader["AccountId"] as byte[]) },
                            Payee = new Payee { Id = new Guid(reader["PayeeId"] as byte[]) },
                            Envelope = new Envelope { Id = new Guid(reader["EnvelopeId"] as byte[]) },
                            LinkedId = reader["LinkedId"] == DBNull.Value ? (Guid?)null : new Guid(reader["LinkedId"] as byte[]),
                            ServiceDate = Convert.ToDateTime(reader["ServiceDate"]),
                            Notes = reader["Notes"].ToString(),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                        };
                    }
                }
            }

            return transaction;
        }

        public async Task<IEnumerable<Transaction>> ReadAccountTransactionsAsync(Guid accountId)
        {
            var transactions = new List<Transaction>();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT Id, 
                                               Amount, 
                                               Posted,
                                               ReconciledDateTime, 
                                               AccountId, 
                                               PayeeId, 
                                               EnvelopeId, 
                                               LinkedId,
                                               ServiceDate, 
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   [Transaction]
                                        WHERE  AccountId = @AccountId";

                command.Parameters.AddWithValue("@AccountId", accountId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            Posted = Convert.ToBoolean(reader["Posted"]),
                            ReconciledDateTime = reader["ReconciledDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReconciledDateTime"]),
                            Account = new Account { Id = new Guid(reader["AccountId"] as byte[]) },
                            Payee = new Payee { Id = new Guid(reader["PayeeId"] as byte[]) },
                            Envelope = new Envelope { Id = new Guid(reader["EnvelopeId"] as byte[]) },
                            LinkedId = reader["LinkedId"] == DBNull.Value ? (Guid?)null : new Guid(reader["LinkedId"] as byte[]),
                            ServiceDate = Convert.ToDateTime(reader["ServiceDate"]),
                            Notes = reader["Notes"].ToString(),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                        });
                    }
                }
            }

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> ReadPayeeTransactionsAsync(Guid payeeId)
        {
            var transactions = new List<Transaction>();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT Id, 
                                               Amount, 
                                               Posted,
                                               ReconciledDateTime,  
                                               AccountId, 
                                               PayeeId, 
                                               EnvelopeId, 
                                               LinkedId,
                                               ServiceDate, 
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   [Transaction]
                                        WHERE  PayeeId = @PayeeId";

                command.Parameters.AddWithValue("@PayeeId", payeeId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            Posted = Convert.ToBoolean(reader["Posted"]),
                            ReconciledDateTime = reader["ReconciledDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReconciledDateTime"]),
                            Account = new Account { Id = new Guid(reader["AccountId"] as byte[]) },
                            Payee = new Payee { Id = new Guid(reader["PayeeId"] as byte[]) },
                            Envelope = new Envelope { Id = new Guid(reader["EnvelopeId"] as byte[]) },
                            LinkedId = reader["LinkedId"] == DBNull.Value ? (Guid?)null : new Guid(reader["LinkedId"] as byte[]),
                            ServiceDate = Convert.ToDateTime(reader["ServiceDate"]),
                            Notes = reader["Notes"].ToString(),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                        });
                    }
                }
            }

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> ReadEnvelopeTransactionsAsync(Guid envelopeId)
        {
            var transactions = new List<Transaction>();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT Id, 
                                               Amount, 
                                               Posted,
                                               ReconciledDateTime, 
                                               AccountId, 
                                               PayeeId, 
                                               EnvelopeId, 
                                               LinkedId,
                                               ServiceDate, 
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   [Transaction]
                                        WHERE  EnvelopeId = @EnvelopeId";

                command.Parameters.AddWithValue("@EnvelopeId", envelopeId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            Posted = Convert.ToBoolean(reader["Posted"]),
                            ReconciledDateTime = reader["ReconciledDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReconciledDateTime"]),
                            Account = new Account { Id = new Guid(reader["AccountId"] as byte[]) },
                            Payee = new Payee { Id = new Guid(reader["PayeeId"] as byte[]) },
                            Envelope = new Envelope { Id = new Guid(reader["EnvelopeId"] as byte[]) },
                            LinkedId = reader["LinkedId"] == DBNull.Value ? (Guid?)null : new Guid(reader["LinkedId"] as byte[]),
                            ServiceDate = Convert.ToDateTime(reader["ServiceDate"]),
                            Notes = reader["Notes"].ToString(),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                        });
                    }
                }
            }

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> ReadLinkedTransactionsAsync(Guid linkedId)
        {
            var transactions = new List<Transaction>();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT Id, 
                                               Amount, 
                                               Posted,
                                               ReconciledDateTime, 
                                               AccountId, 
                                               PayeeId, 
                                               EnvelopeId, 
                                               LinkedId,
                                               ServiceDate, 
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   [Transaction]
                                        WHERE  LinkedId = @LinkedId";

                command.Parameters.AddWithValue("@LinkedId", linkedId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            Posted = Convert.ToBoolean(reader["Posted"]),
                            ReconciledDateTime = reader["ReconciledDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReconciledDateTime"]),
                            Account = new Account { Id = new Guid(reader["AccountId"] as byte[]) },
                            Payee = new Payee { Id = new Guid(reader["PayeeId"] as byte[]) },
                            Envelope = new Envelope { Id = new Guid(reader["EnvelopeId"] as byte[]) },
                            LinkedId = reader["LinkedId"] == DBNull.Value ? (Guid?)null : new Guid(reader["LinkedId"] as byte[]),
                            ServiceDate = Convert.ToDateTime(reader["ServiceDate"]),
                            Notes = reader["Notes"].ToString(),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                        });
                    }
                }
            }

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> ReadTransactionsAsync()
        {
            var transactions = new List<Transaction>();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT Id, 
                                               Amount, 
                                               Posted,
                                               ReconciledDateTime,  
                                               AccountId, 
                                               PayeeId, 
                                               EnvelopeId, 
                                               LinkedId,
                                               ServiceDate, 
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   [Transaction]";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            Posted = Convert.ToBoolean(reader["Posted"]),
                            ReconciledDateTime = reader["ReconciledDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReconciledDateTime"]),
                            Account = new Account { Id = new Guid(reader["AccountId"] as byte[]) },
                            Payee = new Payee { Id = new Guid(reader["PayeeId"] as byte[]) },
                            Envelope = new Envelope { Id = new Guid(reader["EnvelopeId"] as byte[]) },
                            LinkedId = reader["LinkedId"] == DBNull.Value ? (Guid?)null : new Guid(reader["LinkedId"] as byte[]),
                            ServiceDate = Convert.ToDateTime(reader["ServiceDate"]),
                            Notes = reader["Notes"].ToString(),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                        });
                    }
                }
            }

            return transactions;
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"UPDATE [Transaction]
                                        SET    Amount = @Amount, 
                                               Posted = @Posted,
                                               ReconciledDateTime = @ReconciledDateTime, 
                                               AccountId = @AccountId, 
                                               PayeeId = @PayeeId, 
                                               EnvelopeId = @EnvelopeId, 
                                               LinkedId = @LinkedId,
                                               ServiceDate = @ServiceDate, 
                                               Notes = @Notes, 
                                               CreatedDateTime = @CreatedDateTime, 
                                               ModifiedDateTime = @ModifiedDateTime, 
                                               DeletedDateTime = @DeletedDateTime 
                                        WHERE  Id = @Id";

                command.Parameters.AddWithValue("@Id", transaction.Id);
                command.Parameters.AddWithValue("@Amount", transaction.Amount);
                command.Parameters.AddWithValue("@Posted", transaction.Posted);
                command.Parameters.AddWithValue("@ReconciledDateTime", transaction.ReconciledDateTime ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AccountId", transaction.Account?.Id);
                command.Parameters.AddWithValue("@PayeeId", transaction.Payee?.Id);
                command.Parameters.AddWithValue("@EnvelopeId", transaction.Envelope?.Id);
                command.Parameters.AddWithValue("@LinkedId", transaction.LinkedId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ServiceDate", transaction.ServiceDate);
                command.Parameters.AddWithValue("@Notes", transaction.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedDateTime", transaction.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", transaction.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", transaction.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteTransaction(Guid id)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"DELETE [Transaction] WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
