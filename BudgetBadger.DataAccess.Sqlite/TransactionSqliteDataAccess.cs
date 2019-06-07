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
    public class TransactionSqliteDataAccess : ITransactionDataAccess
    {
        readonly string _connectionString;

        public TransactionSqliteDataAccess(string connectionString)
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

                        command.CommandText = @"CREATE TABLE IF NOT EXISTS [Transaction]
                                          ( 
                                             Id                 BLOB PRIMARY KEY NOT NULL, 
                                             Amount             TEXT NOT NULL, 
                                             Posted             INTEGER NOT NULL,
                                             ReconciledDateTime TEXT,
                                             AccountId          BLOB NOT NULL, 
                                             PayeeId            BLOB NOT NULL, 
                                             EnvelopeId         BLOB NOT NULL,
                                             SplitId            BLOB, 
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
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"INSERT INTO [Transaction]
                                                    (Id, 
                                                     Amount, 
                                                     Posted,
                                                     ReconciledDateTime, 
                                                     AccountId, 
                                                     PayeeId, 
                                                     EnvelopeId, 
                                                     SplitId,
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
                                                     @SplitId,
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
                        command.Parameters.AddWithValue("@SplitId", transaction.SplitId ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ServiceDate", transaction.ServiceDate);
                        command.Parameters.AddWithValue("@Notes", transaction.Notes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedDateTime", transaction.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", transaction.ModifiedDateTime);
                        command.Parameters.AddWithValue("@DeletedDateTime", transaction.DeletedDateTime ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task<Transaction> ReadTransactionAsync(Guid id)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var transaction = new Transaction();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT Id, 
                                               Amount, 
                                               Posted,
                                               ReconciledDateTime, 
                                               AccountId, 
                                               PayeeId, 
                                               EnvelopeId, 
                                               SplitId,
                                               ServiceDate, 
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   [Transaction]
                                        WHERE  Id = @Id";

                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                transaction = new Transaction
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Amount = Convert.ToDecimal(reader["Amount"], CultureInfo.InvariantCulture),
                                    Posted = Convert.ToBoolean(reader["Posted"], CultureInfo.InvariantCulture),
                                    ReconciledDateTime = reader["ReconciledDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReconciledDateTime"], CultureInfo.InvariantCulture),
                                    Account = new Account { Id = new Guid(reader["AccountId"] as byte[]) },
                                    Payee = new Payee { Id = new Guid(reader["PayeeId"] as byte[]) },
                                    Envelope = new Envelope { Id = new Guid(reader["EnvelopeId"] as byte[]) },
                                    SplitId = reader["SplitId"] == DBNull.Value ? (Guid?)null : new Guid(reader["SplitId"] as byte[]),
                                    ServiceDate = Convert.ToDateTime(reader["ServiceDate"], CultureInfo.InvariantCulture),
                                    Notes = reader["Notes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture)
                                };
                            }
                        }
                    }
                    return transaction;
                });
            }
        }

        public async Task<IReadOnlyList<Transaction>> ReadAccountTransactionsAsync(Guid accountId)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                var transactions = new List<Transaction>();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT Id, 
                                               Amount, 
                                               Posted,
                                               ReconciledDateTime, 
                                               AccountId, 
                                               PayeeId, 
                                               EnvelopeId, 
                                               SplitId,
                                               ServiceDate, 
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   [Transaction]
                                        WHERE  AccountId = @AccountId";

                        command.Parameters.AddWithValue("@AccountId", accountId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(new Transaction
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Amount = Convert.ToDecimal(reader["Amount"], CultureInfo.InvariantCulture),
                                    Posted = Convert.ToBoolean(reader["Posted"], CultureInfo.InvariantCulture),
                                    ReconciledDateTime = reader["ReconciledDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReconciledDateTime"], CultureInfo.InvariantCulture),
                                    Account = new Account { Id = new Guid(reader["AccountId"] as byte[]) },
                                    Payee = new Payee { Id = new Guid(reader["PayeeId"] as byte[]) },
                                    Envelope = new Envelope { Id = new Guid(reader["EnvelopeId"] as byte[]) },
                                    SplitId = reader["SplitId"] == DBNull.Value ? (Guid?)null : new Guid(reader["SplitId"] as byte[]),
                                    ServiceDate = Convert.ToDateTime(reader["ServiceDate"], CultureInfo.InvariantCulture),
                                    Notes = reader["Notes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture)
                                });
                            }
                        }
                    }

                    return transactions;
                });
            }
        }

        public async Task<IReadOnlyList<Transaction>> ReadPayeeTransactionsAsync(Guid payeeId)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                var transactions = new List<Transaction>();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT Id, 
                                               Amount, 
                                               Posted,
                                               ReconciledDateTime,  
                                               AccountId, 
                                               PayeeId, 
                                               EnvelopeId, 
                                               SplitId,
                                               ServiceDate, 
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   [Transaction]
                                        WHERE  PayeeId = @PayeeId";

                        command.Parameters.AddWithValue("@PayeeId", payeeId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(new Transaction
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Amount = Convert.ToDecimal(reader["Amount"], CultureInfo.InvariantCulture),
                                    Posted = Convert.ToBoolean(reader["Posted"], CultureInfo.InvariantCulture),
                                    ReconciledDateTime = reader["ReconciledDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReconciledDateTime"], CultureInfo.InvariantCulture),
                                    Account = new Account { Id = new Guid(reader["AccountId"] as byte[]) },
                                    Payee = new Payee { Id = new Guid(reader["PayeeId"] as byte[]) },
                                    Envelope = new Envelope { Id = new Guid(reader["EnvelopeId"] as byte[]) },
                                    SplitId = reader["SplitId"] == DBNull.Value ? (Guid?)null : new Guid(reader["SplitId"] as byte[]),
                                    ServiceDate = Convert.ToDateTime(reader["ServiceDate"], CultureInfo.InvariantCulture),
                                    Notes = reader["Notes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture)
                                });
                            }
                        }
                    }

                    return transactions;
                });
            }
        }

        public async Task<IReadOnlyList<Transaction>> ReadEnvelopeTransactionsAsync(Guid envelopeId)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                var transactions = new List<Transaction>();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT Id, 
                                               Amount, 
                                               Posted,
                                               ReconciledDateTime, 
                                               AccountId, 
                                               PayeeId, 
                                               EnvelopeId, 
                                               SplitId,
                                               ServiceDate, 
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   [Transaction]
                                        WHERE  EnvelopeId = @EnvelopeId";

                        command.Parameters.AddWithValue("@EnvelopeId", envelopeId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(new Transaction
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Amount = Convert.ToDecimal(reader["Amount"], CultureInfo.InvariantCulture),
                                    Posted = Convert.ToBoolean(reader["Posted"], CultureInfo.InvariantCulture),
                                    ReconciledDateTime = reader["ReconciledDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReconciledDateTime"], CultureInfo.InvariantCulture),
                                    Account = new Account { Id = new Guid(reader["AccountId"] as byte[]) },
                                    Payee = new Payee { Id = new Guid(reader["PayeeId"] as byte[]) },
                                    Envelope = new Envelope { Id = new Guid(reader["EnvelopeId"] as byte[]) },
                                    SplitId = reader["SplitId"] == DBNull.Value ? (Guid?)null : new Guid(reader["SplitId"] as byte[]),
                                    ServiceDate = Convert.ToDateTime(reader["ServiceDate"], CultureInfo.InvariantCulture),
                                    Notes = reader["Notes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture)
                                });
                            }
                        }
                    }

                    return transactions;
                });
            }
        }

        public async Task<IReadOnlyList<Transaction>> ReadSplitTransactionsAsync(Guid splitId)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                var transactions = new List<Transaction>();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT Id, 
                                               Amount, 
                                               Posted,
                                               ReconciledDateTime, 
                                               AccountId, 
                                               PayeeId, 
                                               EnvelopeId, 
                                               SplitId,
                                               ServiceDate, 
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   [Transaction]
                                        WHERE  SplitId = @SplitId";

                        command.Parameters.AddWithValue("@SplitId", splitId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(new Transaction
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Amount = Convert.ToDecimal(reader["Amount"], CultureInfo.InvariantCulture),
                                    Posted = Convert.ToBoolean(reader["Posted"], CultureInfo.InvariantCulture),
                                    ReconciledDateTime = reader["ReconciledDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReconciledDateTime"], CultureInfo.InvariantCulture),
                                    Account = new Account { Id = new Guid(reader["AccountId"] as byte[]) },
                                    Payee = new Payee { Id = new Guid(reader["PayeeId"] as byte[]) },
                                    Envelope = new Envelope { Id = new Guid(reader["EnvelopeId"] as byte[]) },
                                    SplitId = reader["SplitId"] == DBNull.Value ? (Guid?)null : new Guid(reader["SplitId"] as byte[]),
                                    ServiceDate = Convert.ToDateTime(reader["ServiceDate"], CultureInfo.InvariantCulture),
                                    Notes = reader["Notes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture)
                                });
                            }
                        }
                    }

                    return transactions;
                });
            }
        }

        public async Task<IReadOnlyList<Transaction>> ReadTransactionsAsync()
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                var transactions = new List<Transaction>();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT Id, 
                                               Amount, 
                                               Posted,
                                               ReconciledDateTime,  
                                               AccountId, 
                                               PayeeId, 
                                               EnvelopeId, 
                                               SplitId,
                                               ServiceDate, 
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   [Transaction]";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(new Transaction
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Amount = Convert.ToDecimal(reader["Amount"], CultureInfo.InvariantCulture),
                                    Posted = Convert.ToBoolean(reader["Posted"], CultureInfo.InvariantCulture),
                                    ReconciledDateTime = reader["ReconciledDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReconciledDateTime"], CultureInfo.InvariantCulture),
                                    Account = new Account { Id = new Guid(reader["AccountId"] as byte[]) },
                                    Payee = new Payee { Id = new Guid(reader["PayeeId"] as byte[]) },
                                    Envelope = new Envelope { Id = new Guid(reader["EnvelopeId"] as byte[]) },
                                    SplitId = reader["SplitId"] == DBNull.Value ? (Guid?)null : new Guid(reader["SplitId"] as byte[]),
                                    ServiceDate = Convert.ToDateTime(reader["ServiceDate"], CultureInfo.InvariantCulture),
                                    Notes = reader["Notes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture)
                                });
                            }
                        }
                    }

                    return transactions;
                });
            }
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"UPDATE [Transaction]
                                        SET    Amount = @Amount, 
                                               Posted = @Posted,
                                               ReconciledDateTime = @ReconciledDateTime, 
                                               AccountId = @AccountId, 
                                               PayeeId = @PayeeId, 
                                               EnvelopeId = @EnvelopeId, 
                                               SplitId = @SplitId,
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
                        command.Parameters.AddWithValue("@SplitId", transaction.SplitId ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ServiceDate", transaction.ServiceDate);
                        command.Parameters.AddWithValue("@Notes", transaction.Notes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedDateTime", transaction.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", transaction.ModifiedDateTime);
                        command.Parameters.AddWithValue("@DeletedDateTime", transaction.DeletedDateTime ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task DeleteTransaction(Guid id)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"DELETE [Transaction] WHERE Id = @Id";

                        command.Parameters.AddWithValue("@Id", id);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task<int> GetTransactionsCountAsync()
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
                                    FROM   [Transaction]";

                        object result = command.ExecuteScalar();
                        result = (result == DBNull.Value) ? null : result;
                        count = Convert.ToInt32(result);
                    }

                    return count;
                });
            }
        }
    }
}
