using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Files;
using BudgetBadger.Models;
using Microsoft.Data.Sqlite;

namespace BudgetBadger.DataAccess.Sqlite
{
    public class EnvelopeSqliteDataAccess : IEnvelopeDataAccess
    {
        readonly SqliteConnection _connection;

        public EnvelopeSqliteDataAccess(SqliteConnection connection)
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

                command.CommandText = @"CREATE TABLE IF NOT EXISTS EnvelopeGroup 
                                      ( 
                                         Id               BLOB PRIMARY KEY NOT NULL, 
                                         Description      TEXT NOT NULL, 
                                         Notes            TEXT, 
                                         CreatedDateTime  TEXT NOT NULL, 
                                         ModifiedDateTime TEXT NOT NULL, 
                                         DeletedDateTime  TEXT 
                                      ); 

                                    CREATE TABLE IF NOT EXISTS Envelope 
                                      ( 
                                         Id               BLOB PRIMARY KEY NOT NULL, 
                                         Description      TEXT NOT NULL, 
                                         EnvelopeGroupId  BLOB NOT NULL, 
                                         Notes            TEXT, 
                                         IgnoreOverspend  INTEGER NOT NULL,
                                         CreatedDateTime  TEXT NOT NULL, 
                                         ModifiedDateTime TEXT NOT NULL, 
                                         DeletedDateTime  TEXT,
                                         FOREIGN KEY(EnvelopeGroupId) REFERENCES EnvelopeGroup(Id)
                                      ); 
            
                                    CREATE TABLE IF NOT EXISTS BudgetSchedule 
                                      ( 
                                         Id          BLOB PRIMARY KEY NOT NULL, 
                                         BeginDate   TEXT NOT NULL,
                                         EndDate     TEXT NOT NULL,
                                         CreatedDateTime  TEXT NOT NULL, 
                                         ModifiedDateTime TEXT NOT NULL, 
                                         DeletedDateTime  TEXT
                                      );
            
                                    CREATE TABLE IF NOT EXISTS Budget 
                                      ( 
                                         Id               BLOB PRIMARY KEY NOT NULL, 
                                         Amount           TEXT NOT NULL,
                                         IgnoreOverspend  INTEGER NOT NULL, 
                                         BudgetScheduleId BLOB NOT NULL, 
                                         EnvelopeId       BLOB NOT NULL,
                                         CreatedDateTime  TEXT NOT NULL, 
                                         ModifiedDateTime TEXT NOT NULL, 
                                         DeletedDateTime  TEXT,
                                         FOREIGN KEY(BudgetScheduleId) REFERENCES BudgetSchedule(Id),
                                         FOREIGN KEY(EnvelopeId) REFERENCES Envelope(Id)
                                      );
                                    ";

                command.ExecuteNonQuery();


                // envelope groups
                command = _connection.CreateCommand();

                command.CommandText = @"INSERT OR IGNORE INTO EnvelopeGroup
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

                command.Parameters.AddWithValue("@Id", Constants.DebtEnvelopeGroup.Id);
                command.Parameters.AddWithValue("@Description", Constants.DebtEnvelopeGroup.Description);
                command.Parameters.AddWithValue("@Notes", Constants.DebtEnvelopeGroup.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedDateTime", Constants.DebtEnvelopeGroup.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", Constants.DebtEnvelopeGroup.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", Constants.DebtEnvelopeGroup.DeletedDateTime ?? (object)DBNull.Value);

                command.ExecuteNonQuery();


                command = _connection.CreateCommand();

                command.CommandText = @"INSERT OR IGNORE INTO EnvelopeGroup
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

                command.Parameters.AddWithValue("@Id", Constants.IncomeEnvelopeGroup.Id);
                command.Parameters.AddWithValue("@Description", Constants.IncomeEnvelopeGroup.Description);
                command.Parameters.AddWithValue("@Notes", Constants.IncomeEnvelopeGroup.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedDateTime", Constants.IncomeEnvelopeGroup.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", Constants.IncomeEnvelopeGroup.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", Constants.IncomeEnvelopeGroup.DeletedDateTime ?? (object)DBNull.Value);

                command.ExecuteNonQuery();


                command = _connection.CreateCommand();

                command.CommandText = @"INSERT OR IGNORE INTO EnvelopeGroup
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

                command.Parameters.AddWithValue("@Id", Constants.SystemEnvelopeGroup.Id);
                command.Parameters.AddWithValue("@Description", Constants.SystemEnvelopeGroup.Description);
                command.Parameters.AddWithValue("@Notes", Constants.SystemEnvelopeGroup.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedDateTime", Constants.SystemEnvelopeGroup.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", Constants.SystemEnvelopeGroup.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", Constants.SystemEnvelopeGroup.DeletedDateTime ?? (object)DBNull.Value);

                command.ExecuteNonQuery();


                command = _connection.CreateCommand();

                command.CommandText = @"INSERT OR IGNORE INTO Envelope 
                                                (Id, 
                                                 Description, 
                                                 EnvelopeGroupId, 
                                                 Notes, 
                                                 IgnoreOverspend,
                                                 CreatedDateTime, 
                                                 ModifiedDateTime, 
                                                 DeletedDateTime) 
                                    VALUES     (@Id, 
                                                @Description, 
                                                @EnvelopeGroupId,
                                                @Notes, 
                                                @IgnoreOverspend,
                                                @CreatedDateTime, 
                                                @ModifiedDateTime, 
                                                @DeletedDateTime)";

                command.Parameters.AddWithValue("@Id", Constants.BufferEnvelope.Id);
                command.Parameters.AddWithValue("@Description", Constants.BufferEnvelope.Description);
                command.Parameters.AddWithValue("@EnvelopeGroupId", Constants.BufferEnvelope.Group?.Id);
                command.Parameters.AddWithValue("@Notes", Constants.BufferEnvelope.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IgnoreOverspend", Constants.BufferEnvelope.IgnoreOverspend);
                command.Parameters.AddWithValue("@CreatedDateTime", Constants.BufferEnvelope.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", Constants.BufferEnvelope.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", Constants.BufferEnvelope.DeletedDateTime ?? (object)DBNull.Value);

                command.ExecuteNonQuery();


                command = _connection.CreateCommand();

                command.CommandText = @"INSERT OR IGNORE INTO Envelope 
                                                (Id, 
                                                 Description, 
                                                 EnvelopeGroupId, 
                                                 Notes, 
                                                 IgnoreOverspend,
                                                 CreatedDateTime, 
                                                 ModifiedDateTime, 
                                                 DeletedDateTime) 
                                    VALUES     (@Id, 
                                                @Description, 
                                                @EnvelopeGroupId,
                                                @Notes, 
                                                @IgnoreOverspend,
                                                @CreatedDateTime, 
                                                @ModifiedDateTime, 
                                                @DeletedDateTime)";

                command.Parameters.AddWithValue("@Id", Constants.IgnoredEnvelope.Id);
                command.Parameters.AddWithValue("@Description", Constants.IgnoredEnvelope.Description);
                command.Parameters.AddWithValue("@EnvelopeGroupId", Constants.IgnoredEnvelope.Group?.Id);
                command.Parameters.AddWithValue("@Notes", Constants.IgnoredEnvelope.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IgnoreOverspend", Constants.IgnoredEnvelope.IgnoreOverspend);
                command.Parameters.AddWithValue("@CreatedDateTime", Constants.IgnoredEnvelope.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", Constants.IgnoredEnvelope.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", Constants.IgnoredEnvelope.DeletedDateTime ?? (object)DBNull.Value);

                command.ExecuteNonQuery();


                command = _connection.CreateCommand();

                command.CommandText = @"INSERT OR IGNORE INTO Envelope 
                                                (Id, 
                                                 Description, 
                                                 EnvelopeGroupId, 
                                                 Notes, 
                                                 IgnoreOverspend,
                                                 CreatedDateTime, 
                                                 ModifiedDateTime, 
                                                 DeletedDateTime) 
                                    VALUES     (@Id, 
                                                @Description, 
                                                @EnvelopeGroupId,
                                                @Notes, 
                                                @IgnoreOverspend,
                                                @CreatedDateTime, 
                                                @ModifiedDateTime, 
                                                @DeletedDateTime)";

                command.Parameters.AddWithValue("@Id", Constants.IncomeEnvelope.Id);
                command.Parameters.AddWithValue("@Description", Constants.IncomeEnvelope.Description);
                command.Parameters.AddWithValue("@EnvelopeGroupId", Constants.IncomeEnvelope.Group?.Id);
                command.Parameters.AddWithValue("@Notes", Constants.IncomeEnvelope.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IgnoreOverspend", Constants.IncomeEnvelope.IgnoreOverspend);
                command.Parameters.AddWithValue("@CreatedDateTime", Constants.IncomeEnvelope.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", Constants.IncomeEnvelope.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", Constants.IncomeEnvelope.DeletedDateTime ?? (object)DBNull.Value);

                command.ExecuteNonQuery();
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task CreateBudgetAsync(Budget budget)
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"INSERT INTO Budget
                                                (Id, 
                                                 Amount, 
                                                 IgnoreOverspend,
                                                 BudgetScheduleId, 
                                                 EnvelopeId, 
                                                 CreatedDateTime, 
                                                 ModifiedDateTime)  
                                    VALUES     (@Id, 
                                                @Amount,
                                                @IgnoreOverspend, 
                                                @BudgetScheduleId, 
                                                @EnvelopeId, 
                                                @CreatedDateTime, 
                                                @ModifiedDateTime)";

                command.Parameters.AddWithValue("@Id", budget.Id);
                command.Parameters.AddWithValue("@Amount", budget.Amount);
                command.Parameters.AddWithValue("@IgnoreOverspend", budget.IgnoreOverspend);
                command.Parameters.AddWithValue("@BudgetScheduleId", budget.Schedule?.Id);
                command.Parameters.AddWithValue("@EnvelopeId", budget.Envelope?.Id);
                command.Parameters.AddWithValue("@CreatedDateTime", budget.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", budget.ModifiedDateTime);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }
        }

        public async Task CreateBudgetScheduleAsync(BudgetSchedule budgetSchedule)
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"INSERT INTO BudgetSchedule
                                                (Id, 
                                                 BeginDate,
                                                 EndDate, 
                                                 CreatedDateTime, 
                                                 ModifiedDateTime) 
                                    VALUES     (@Id, 
                                                @BeginDate, 
                                                @EndDate, 
                                                @CreatedDateTime, 
                                                @ModifiedDateTime)";

                command.Parameters.AddWithValue("@Id", budgetSchedule.Id);
                command.Parameters.AddWithValue("@BeginDate", budgetSchedule.BeginDate);
                command.Parameters.AddWithValue("@EndDate", budgetSchedule.EndDate);
                command.Parameters.AddWithValue("@CreatedDateTime", budgetSchedule.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", budgetSchedule.ModifiedDateTime);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }
        }

        public async Task CreateEnvelopeAsync(Envelope envelope)
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"INSERT INTO Envelope 
                                                (Id, 
                                                 Description, 
                                                 EnvelopeGroupId, 
                                                 Notes, 
                                                 IgnoreOverspend,
                                                 CreatedDateTime, 
                                                 ModifiedDateTime, 
                                                 DeletedDateTime) 
                                    VALUES     (@Id, 
                                                @Description, 
                                                @EnvelopeGroupId,
                                                @Notes, 
                                                @IgnoreOverspend,
                                                @CreatedDateTime, 
                                                @ModifiedDateTime, 
                                                @DeletedDateTime)";

                command.Parameters.AddWithValue("@Id", envelope.Id);
                command.Parameters.AddWithValue("@Description", envelope.Description);
                command.Parameters.AddWithValue("@EnvelopeGroupId", envelope.Group?.Id);
                command.Parameters.AddWithValue("@Notes", envelope.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IgnoreOverspend", envelope.IgnoreOverspend);
                command.Parameters.AddWithValue("@CreatedDateTime", envelope.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", envelope.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", envelope.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }
        }

        public async Task CreateEnvelopeGroupAsync(EnvelopeGroup envelopeGroup)
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"INSERT INTO EnvelopeGroup
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

                command.Parameters.AddWithValue("@Id", envelopeGroup.Id);
                command.Parameters.AddWithValue("@Description", envelopeGroup.Description);
                command.Parameters.AddWithValue("@Notes", envelopeGroup.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedDateTime", envelopeGroup.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", envelopeGroup.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", envelopeGroup.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }
        }

        public async Task DeleteBudgetAsync(Guid id)
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"DELETE Budget WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }
        }

        public async Task DeleteBudgetScheduleAsync(Guid id)
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"DELETE BudgetSchedule WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }
        }

        public async Task DeleteEnvelopeAsync(Guid id)
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"DELETE Envelope WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }
        }

        public async Task DeleteEnvelopeGroupAsync(Guid id)
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"DELETE EnvelopeGroup WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }
        }

        public async Task<Budget> ReadBudgetAsync(Guid id)
        {
            var budget = new Budget();

            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"SELECT B.Id, 
                                           B.Amount, 
                                           B.IgnoreOverspend,
                                           B.CreatedDateTime, 
                                           B.ModifiedDateTime, 
                                           B.BudgetScheduleId, 
                                           BS.BeginDate        AS BudgetScheduleBeginDate, 
                                           BS.EndDate          AS BudgetScheduleEndDate,
                                           BS.CreatedDateTime  AS BudgetScheduleCreatedDateTime, 
                                           BS.ModifiedDateTime AS BudgetScheduleModifiedDateTime, 
                                           B.EnvelopeId, 
                                           E.Description       AS EnvelopeDescription, 
                                           E.Notes             AS EnvelopeNotes, 
                                           E.IgnoreOverspend   AS EnvelopeIgnoreOverspend,
                                           E.CreatedDateTime   AS EnvelopeCreatedDateTime, 
                                           E.ModifiedDateTime  AS EnvelopeModifiedDateTime, 
                                           E.DeletedDateTime   AS EnvelopeDeletedDateTime, 
                                           EG.Id               AS EnvelopeGroupId, 
                                           EG.Description      AS EnvelopeGroupDescription,
                                           EG.Notes            AS EnvelopeGroupNotes, 
                                           EG.CreatedDateTime  AS EnvelopeGroupCreatedDateTime, 
                                           EG.ModifiedDateTime AS EnvelopeGroupModifiedDateTime, 
                                           EG.DeletedDateTime  AS EnvelopeGroupDeletedDateTime 
                                    FROM   Budget AS B 
                                    JOIN   BudgetSchedule AS BS ON B.BudgetScheduleId = BS.Id
                                    JOIN   Envelope AS E ON B.EnvelopeId = E.Id
                                    JOIN   EnvelopeGroup EG ON E.EnvelopeGroupId = EG.Id
                                    WHERE  B.Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (reader.Read())
                    {
                        budget = new Budget()
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            IgnoreOverspend = Convert.ToBoolean(reader["IgnoreOverspend"]),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            Schedule = new BudgetSchedule
                            {
                                Id = new Guid(reader["BudgetScheduleId"] as byte[]),
                                BeginDate = Convert.ToDateTime(reader["BudgetScheduleBeginDate"]),
                                EndDate = Convert.ToDateTime(reader["BudgetScheduleEndDate"]),
                                CreatedDateTime = Convert.ToDateTime(reader["BudgetScheduleCreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["BudgetScheduleModifiedDateTime"]),
                            },
                            Envelope = new Envelope
                            {
                                Id = new Guid(reader["EnvelopeId"] as byte[]),
                                Description = reader["EnvelopeDescription"].ToString(),
                                Notes = reader["EnvelopeNotes"].ToString(),
                                IgnoreOverspend = Convert.ToBoolean(reader["EnvelopeIgnoreOverspend"]),
                                CreatedDateTime = Convert.ToDateTime(reader["EnvelopeCreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeModifiedDateTime"]),
                                DeletedDateTime = reader["EnvelopeDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeDeletedDateTime"]),
                                Group = new EnvelopeGroup
                                {
                                    Id = new Guid(reader["EnvelopeGroupId"] as byte[]),
                                    Description = reader["EnvelopeGroupDescription"].ToString(),
                                    Notes = reader["EnvelopeGroupNotes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["EnvelopeGroupCreatedDateTime"]),
                                    ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeGroupModifiedDateTime"]),
                                    DeletedDateTime = reader["EnvelopeGroupDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupDeletedDateTime"])
                                }
                            }
                        };
                    }
                }
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }

            return budget;
        }

        public async Task<IReadOnlyList<Budget>> ReadBudgetsAsync()
        {
            var budgets = new List<Budget>();

            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"SELECT B.Id, 
                                           B.Amount, 
                                           B.IgnoreOverspend,
                                           B.CreatedDateTime, 
                                           B.ModifiedDateTime, 
                                           B.BudgetScheduleId, 
                                           BS.BeginDate        AS BudgetScheduleBeginDate, 
                                           BS.EndDate          AS BudgetScheduleEndDate,
                                           BS.CreatedDateTime  AS BudgetScheduleCreatedDateTime, 
                                           BS.ModifiedDateTime AS BudgetScheduleModifiedDateTime, 
                                           B.EnvelopeId, 
                                           E.Description       AS EnvelopeDescription, 
                                           E.Notes             AS EnvelopeNotes, 
                                           E.IgnoreOverspend   AS EnvelopeIgnoreOverspend,
                                           E.CreatedDateTime   AS EnvelopeCreatedDateTime, 
                                           E.ModifiedDateTime  AS EnvelopeModifiedDateTime, 
                                           E.DeletedDateTime   AS EnvelopeDeletedDateTime, 
                                           EG.Id               AS EnvelopeGroupId, 
                                           EG.Description      AS EnvelopeGroupDescription,
                                           EG.Notes            AS EnvelopeGroupNotes, 
                                           EG.CreatedDateTime  AS EnvelopeGroupCreatedDateTime, 
                                           EG.ModifiedDateTime AS EnvelopeGroupModifiedDateTime, 
                                           EG.DeletedDateTime  AS EnvelopeGroupDeletedDateTime
                                    FROM   Budget AS B 
                                    JOIN   BudgetSchedule AS BS ON B.BudgetScheduleId = BS.Id
                                    JOIN   Envelope AS E ON B.EnvelopeId = E.Id
                                    JOIN   EnvelopeGroup EG ON E.EnvelopeGroupId = EG.Id";

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (reader.Read())
                    {
                        budgets.Add(new Budget()
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            IgnoreOverspend = Convert.ToBoolean(reader["IgnoreOverspend"]),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            Schedule = new BudgetSchedule
                            {
                                Id = new Guid(reader["BudgetScheduleId"] as byte[]),
                                BeginDate = Convert.ToDateTime(reader["BudgetScheduleBeginDate"]),
                                EndDate = Convert.ToDateTime(reader["BudgetScheduleEndDate"]),
                                CreatedDateTime = Convert.ToDateTime(reader["BudgetScheduleCreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["BudgetScheduleModifiedDateTime"]),
                            },
                            Envelope = new Envelope
                            {
                                Id = new Guid(reader["EnvelopeId"] as byte[]),
                                Description = reader["EnvelopeDescription"].ToString(),
                                Notes = reader["EnvelopeNotes"].ToString(),
                                IgnoreOverspend = Convert.ToBoolean(reader["EnvelopeIgnoreOverspend"]),
                                CreatedDateTime = Convert.ToDateTime(reader["EnvelopeCreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeModifiedDateTime"]),
                                DeletedDateTime = reader["EnvelopeDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeDeletedDateTime"]),
                                Group = new EnvelopeGroup
                                {
                                    Id = new Guid(reader["EnvelopeGroupId"] as byte[]),
                                    Description = reader["EnvelopeGroupDescription"].ToString(),
                                    Notes = reader["EnvelopeGroupNotes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["EnvelopeGroupCreatedDateTime"]),
                                    ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeGroupModifiedDateTime"]),
                                    DeletedDateTime = reader["EnvelopeGroupDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupDeletedDateTime"])
                                }
                            }
                        });
                    }
                }
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }

            return budgets;
        }

        public async Task<IReadOnlyList<Budget>> ReadBudgetsFromScheduleAsync(Guid scheduleId)
        {
            var budgets = new List<Budget>();

            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"SELECT B.Id, 
                                           B.Amount, 
                                           B.IgnoreOverspend,
                                           B.CreatedDateTime, 
                                           B.ModifiedDateTime, 
                                           B.BudgetScheduleId, 
                                           BS.BeginDate        AS BudgetScheduleBeginDate, 
                                           BS.EndDate          AS BudgetScheduleEndDate,
                                           BS.CreatedDateTime  AS BudgetScheduleCreatedDateTime, 
                                           BS.ModifiedDateTime AS BudgetScheduleModifiedDateTime,  
                                           B.EnvelopeId, 
                                           E.Description       AS EnvelopeDescription, 
                                           E.Notes             AS EnvelopeNotes, 
                                           E.IgnoreOverspend   AS EnvelopeIgnoreOverspend,
                                           E.CreatedDateTime   AS EnvelopeCreatedDateTime, 
                                           E.ModifiedDateTime  AS EnvelopeModifiedDateTime, 
                                           E.DeletedDateTime   AS EnvelopeDeletedDateTime, 
                                           EG.Id               AS EnvelopeGroupId, 
                                           EG.Description      AS EnvelopeGroupDescription,
                                           EG.Notes            AS EnvelopeGroupNotes, 
                                           EG.CreatedDateTime  AS EnvelopeGroupCreatedDateTime, 
                                           EG.ModifiedDateTime AS EnvelopeGroupModifiedDateTime, 
                                           EG.DeletedDateTime  AS EnvelopeGroupDeletedDateTime
                                    FROM   Budget AS B 
                                    JOIN   BudgetSchedule BS ON B.BudgetScheduleId = BS.Id
                                    JOIN   Envelope E ON B.EnvelopeId = E.Id
                                    JOIN   EnvelopeGroup EG ON E.EnvelopeGroupId = EG.Id
                                    WHERE  BS.Id = @ScheduleId";

                command.Parameters.AddWithValue("@ScheduleId", scheduleId);

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (reader.Read())
                    {
                        budgets.Add(new Budget()
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            IgnoreOverspend = Convert.ToBoolean(reader["IgnoreOverspend"]),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            Schedule = new BudgetSchedule
                            {
                                Id = new Guid(reader["BudgetScheduleId"] as byte[]),
                                BeginDate = Convert.ToDateTime(reader["BudgetScheduleBeginDate"]),
                                EndDate = Convert.ToDateTime(reader["BudgetScheduleEndDate"]),
                                CreatedDateTime = Convert.ToDateTime(reader["BudgetScheduleCreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["BudgetScheduleModifiedDateTime"]),
                            },
                            Envelope = new Envelope
                            {
                                Id = new Guid(reader["EnvelopeId"] as byte[]),
                                Description = reader["EnvelopeDescription"].ToString(),
                                Notes = reader["EnvelopeNotes"].ToString(),
                                IgnoreOverspend = Convert.ToBoolean(reader["EnvelopeIgnoreOverspend"]),
                                CreatedDateTime = Convert.ToDateTime(reader["EnvelopeCreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeModifiedDateTime"]),
                                DeletedDateTime = reader["EnvelopeDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeDeletedDateTime"]),
                                Group = new EnvelopeGroup
                                {
                                    Id = new Guid(reader["EnvelopeGroupId"] as byte[]),
                                    Description = reader["EnvelopeGroupDescription"].ToString(),
                                    Notes = reader["EnvelopeGroupNotes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["EnvelopeGroupCreatedDateTime"]),
                                    ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeGroupModifiedDateTime"]),
                                    DeletedDateTime = reader["EnvelopeGroupDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupDeletedDateTime"])
                                }
                            }
                        });
                    }
                }
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }

            return budgets;
        }

        public async Task<IReadOnlyList<Budget>> ReadBudgetsFromEnvelopeAsync(Guid envelopeId)
        {
            var budgets = new List<Budget>();

            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"SELECT B.Id, 
                                           B.Amount, 
                                           B.IgnoreOverspend,
                                           B.CreatedDateTime, 
                                           B.ModifiedDateTime, 
                                           B.BudgetScheduleId, 
                                           BS.BeginDate        AS BudgetScheduleBeginDate, 
                                           BS.EndDate          AS BudgetScheduleEndDate,
                                           BS.CreatedDateTime  AS BudgetScheduleCreatedDateTime, 
                                           BS.ModifiedDateTime AS BudgetScheduleModifiedDateTime, 
                                           B.EnvelopeId, 
                                           E.Description       AS EnvelopeDescription, 
                                           E.Notes             AS EnvelopeNotes, 
                                           E.IgnoreOverspend   AS EnvelopeIgnoreOverspend,
                                           E.CreatedDateTime   AS EnvelopeCreatedDateTime, 
                                           E.ModifiedDateTime  AS EnvelopeModifiedDateTime, 
                                           E.DeletedDateTime   AS EnvelopeDeletedDateTime, 
                                           EG.Id               AS EnvelopeGroupId, 
                                           EG.Description      AS EnvelopeGroupDescription,
                                           EG.Notes            AS EnvelopeGroupNotes, 
                                           EG.CreatedDateTime  AS EnvelopeGroupCreatedDateTime, 
                                           EG.ModifiedDateTime AS EnvelopeGroupModifiedDateTime, 
                                           EG.DeletedDateTime  AS EnvelopeGroupDeletedDateTime
                                    FROM   Budget AS B 
                                    JOIN   BudgetSchedule BS ON B.BudgetScheduleId = BS.Id
                                    JOIN   Envelope E ON B.EnvelopeId = E.Id
                                    JOIN   EnvelopeGroup EG ON E.EnvelopeGroupId = EG.Id
                                    WHERE  E.Id = @EnvelopeId";

                command.Parameters.AddWithValue("@EnvelopeId", envelopeId);

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (reader.Read())
                    {
                        budgets.Add(new Budget()
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            IgnoreOverspend = Convert.ToBoolean(reader["IgnoreOverspend"]),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            Schedule = new BudgetSchedule
                            {
                                Id = new Guid(reader["BudgetScheduleId"] as byte[]),
                                BeginDate = Convert.ToDateTime(reader["BudgetScheduleBeginDate"]),
                                EndDate = Convert.ToDateTime(reader["BudgetScheduleEndDate"]),
                                CreatedDateTime = Convert.ToDateTime(reader["BudgetScheduleCreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["BudgetScheduleModifiedDateTime"]),
                            },
                            Envelope = new Envelope
                            {
                                Id = new Guid(reader["EnvelopeId"] as byte[]),
                                Description = reader["EnvelopeDescription"].ToString(),
                                Notes = reader["EnvelopeNotes"].ToString(),
                                IgnoreOverspend = Convert.ToBoolean(reader["EnvelopeIgnoreOverspend"]),
                                CreatedDateTime = Convert.ToDateTime(reader["EnvelopeCreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeModifiedDateTime"]),
                                DeletedDateTime = reader["EnvelopeDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeDeletedDateTime"]),
                                Group = new EnvelopeGroup
                                {
                                    Id = new Guid(reader["EnvelopeGroupId"] as byte[]),
                                    Description = reader["EnvelopeGroupDescription"].ToString(),
                                    Notes = reader["EnvelopeGroupNotes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["EnvelopeGroupCreatedDateTime"]),
                                    ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeGroupModifiedDateTime"]),
                                    DeletedDateTime = reader["EnvelopeGroupDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupDeletedDateTime"])
                                }
                            }
                        });
                    }
                }
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }

            return budgets;
        }

        public async Task<BudgetSchedule> ReadBudgetScheduleAsync(Guid id)
        {
            var budgetSchedule = new BudgetSchedule();

            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"SELECT Id,
                                           BeginDate, 
                                           EndDate,
                                           CreatedDateTime,
                                           ModifiedDateTime
                                    FROM   BudgetSchedule
                                    WHERE  Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (reader.Read())
                    {
                        budgetSchedule = new BudgetSchedule
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            BeginDate = Convert.ToDateTime(reader["BeginDate"]),
                            EndDate = Convert.ToDateTime(reader["EndDate"]),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                        };
                    }
                }
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }

            return budgetSchedule;
        }

        public async Task<IReadOnlyList<BudgetSchedule>> ReadBudgetSchedulesAsync()
        {
            var budgetSchedules = new List<BudgetSchedule>();

            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"SELECT Id,
                                           BeginDate, 
                                           EndDate,
                                           CreatedDateTime,
                                           ModifiedDateTime
                                    FROM   BudgetSchedule";

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (reader.Read())
                    {
                        budgetSchedules.Add(new BudgetSchedule
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            BeginDate = Convert.ToDateTime(reader["BeginDate"]),
                            EndDate = Convert.ToDateTime(reader["EndDate"]),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                        });
                    }
                }
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }

            return budgetSchedules;
        }

        public async Task<Envelope> ReadEnvelopeAsync(Guid id)
        {
            var envelope = new Envelope();

            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"SELECT E.Id, 
                                           E.Description, 
                                           E.Notes, 
                                           E.IgnoreOverspend,
                                           E.CreatedDateTime, 
                                           E.ModifiedDateTime, 
                                           E.DeletedDateTime, 
                                           E.EnvelopeGroupId, 
                                           EG.Description      AS EnvelopeGroupDescription, 
                                           EG.Notes            AS EnvelopeGroupNotes, 
                                           EG.CreatedDateTime  AS EnvelopeGroupCreatedDateTime, 
                                           EG.ModifiedDateTime AS EnvelopeGroupModifiedDateTime, 
                                           EG.DeletedDateTime  AS EnvelopeGroupDeletedDateTime 
                                    FROM   Envelope AS E
                                    JOIN   EnvelopeGroup AS EG ON E.EnvelopeGroupId = EG.Id
                                    WHERE  E.Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (reader.Read())
                    {
                        envelope = new Envelope
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Description = reader["Description"].ToString(),
                            Notes = reader["Notes"].ToString(),
                            IgnoreOverspend = Convert.ToBoolean(reader["IgnoreOverspend"]),
                            Group = new EnvelopeGroup
                            {
                                Id = new Guid(reader["EnvelopeGroupId"] as byte[]),
                                Description = reader["EnvelopeGroupDescription"].ToString(),
                                Notes = reader["EnvelopeGroupNotes"].ToString(),
                                CreatedDateTime = Convert.ToDateTime(reader["EnvelopeGroupCreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeGroupModifiedDateTime"]),
                                DeletedDateTime = reader["EnvelopeGroupDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupDeletedDateTime"])
                            },
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
                MultiThreadLock.SemaphoreSlim.Release();
            }

            return envelope;
        }

        public async Task<EnvelopeGroup> ReadEnvelopeGroupAsync(Guid id)
        {
            var envelopeGroup = new EnvelopeGroup();

            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"SELECT Id,
                                               Description,
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime, 
                                        FROM   EnvelopeGroup
                                        WHERE  Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (reader.Read())
                    {
                        envelopeGroup = new EnvelopeGroup
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
                MultiThreadLock.SemaphoreSlim.Release();
            }

            return envelopeGroup;
        }

        public async Task<IReadOnlyList<EnvelopeGroup>> ReadEnvelopeGroupsAsync()
        {
            var envelopeGroups = new List<EnvelopeGroup>();

            await MultiThreadLock.SemaphoreSlim.WaitAsync();

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
                                        FROM   EnvelopeGroup";

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (reader.Read())
                    {
                        envelopeGroups.Add(new EnvelopeGroup
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Description = reader["Description"].ToString(),
                            Notes = reader["Notes"].ToString(),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                        });
                    }
                }
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }

            return envelopeGroups;
        }

        public async Task<IReadOnlyList<Envelope>> ReadEnvelopesAsync()
        {
            var envelopes = new List<Envelope>();

            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"SELECT E.Id, 
                                               E.Description, 
                                               E.Notes, 
                                               E.IgnoreOverspend,
                                               E.CreatedDateTime, 
                                               E.ModifiedDateTime, 
                                               E.DeletedDateTime, 
                                               E.EnvelopeGroupId, 
                                               EG.Description      AS EnvelopeGroupDescription, 
                                               EG.Notes            AS EnvelopeGroupNotes, 
                                               EG.CreatedDateTime  AS EnvelopeGroupCreatedDateTime, 
                                               EG.ModifiedDateTime AS EnvelopeGroupModifiedDateTime, 
                                               EG.DeletedDateTime  AS EnvelopeGroupDeletedDateTime 
                                        FROM   Envelope AS E
                                        JOIN   EnvelopeGroup AS EG ON E.EnvelopeGroupId = EG.Id";

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (reader.Read())
                    {
                        envelopes.Add(new Envelope
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Description = reader["Description"].ToString(),
                            Notes = reader["Notes"].ToString(),
                            IgnoreOverspend = Convert.ToBoolean(reader["IgnoreOverspend"]),
                            Group = new EnvelopeGroup
                            {
                                Id = new Guid(reader["EnvelopeGroupId"] as byte[]),
                                Description = reader["EnvelopeGroupDescription"].ToString(),
                                Notes = reader["EnvelopeGroupNotes"].ToString(),
                                CreatedDateTime = Convert.ToDateTime(reader["EnvelopeGroupCreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeGroupModifiedDateTime"]),
                                DeletedDateTime = reader["EnvelopeGroupDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupDeletedDateTime"])
                            },
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                        });
                    }
                }
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }

            return envelopes;
        }

        public async Task UpdateBudgetAsync(Budget budget)
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"UPDATE Budget 
                                        SET    Amount = @Amount, 
                                               IgnoreOverspend = @IgnoreOverspend,
                                               EnvelopeId = @EnvelopeId, 
                                               BudgetScheduleId = @BudgetScheduleId,
                                               CreatedDateTime = @CreatedDateTime,
                                               ModifiedDateTime = @ModifiedDateTime
                                        WHERE  Id = @Id";

                command.Parameters.AddWithValue("@Id", budget.Id);
                command.Parameters.AddWithValue("@Amount", budget.Amount);
                command.Parameters.AddWithValue("@IgnoreOverspend", budget.IgnoreOverspend);
                command.Parameters.AddWithValue("@EnvelopeId", budget.Envelope?.Id);
                command.Parameters.AddWithValue("@BudgetScheduleId", budget.Schedule?.Id);
                command.Parameters.AddWithValue("@CreatedDateTime", budget.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", budget.ModifiedDateTime);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }
        }

        public async Task UpdateBudgetScheduleAsync(BudgetSchedule budgetSchedule)
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"UPDATE BudgetSchedule
                                        SET    BeginDate = @BeginDate, 
                                               EndDate = @EndDate,
                                               CreatedDateTime = @CreatedDateTime,
                                               ModifiedDateTime = @ModifiedDateTime
                                        WHERE  Id = @Id";

                command.Parameters.AddWithValue("@Id", budgetSchedule.Id);
                command.Parameters.AddWithValue("@BeginDate", budgetSchedule.BeginDate);
                command.Parameters.AddWithValue("@EndDate", budgetSchedule.EndDate);
                command.Parameters.AddWithValue("@CreatedDateTime", budgetSchedule.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", budgetSchedule.ModifiedDateTime);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }
        }

        public async Task UpdateEnvelopeAsync(Envelope envelope)
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"UPDATE Envelope 
                                        SET    Description = @Description,
                                               EnvelopeGroupId = @EnvelopeGroupId,
                                               Notes = @Notes,
                                               IgnoreOverspend = @IgnoreOverspend,
                                               CreatedDateTime = @CreatedDateTime, 
                                               ModifiedDateTime = @ModifiedDateTime, 
                                               DeletedDateTime = @DeletedDateTime 
                                        WHERE  Id = @Id ";

                command.Parameters.AddWithValue("@Id", envelope.Id);
                command.Parameters.AddWithValue("@Description", envelope.Description);
                command.Parameters.AddWithValue("@EnvelopeGroupId", envelope.Group?.Id);
                command.Parameters.AddWithValue("@Notes", envelope.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IgnoreOverspend", envelope.IgnoreOverspend);
                command.Parameters.AddWithValue("@CreatedDateTime", envelope.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", envelope.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", envelope.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }
        }

        public async Task UpdateEnvelopeGroupAsync(EnvelopeGroup envelopeGroup)
        {
            await MultiThreadLock.SemaphoreSlim.WaitAsync();

            try
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                var command = _connection.CreateCommand();

                command.CommandText = @"UPDATE EnvelopeGroup
                                        SET    Description = @Description,
                                               Notes = @Notes,
                                               CreatedDateTime = @CreatedDateTime, 
                                               ModifiedDateTime = @ModifiedDateTime, 
                                               DeletedDateTime = @DeletedDateTime 
                                        WHERE  Id = @Id ";

                command.Parameters.AddWithValue("@Id", envelopeGroup.Id);
                command.Parameters.AddWithValue("@Description", envelopeGroup.Description);
                command.Parameters.AddWithValue("@Notes", envelopeGroup.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedDateTime", envelopeGroup.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", envelopeGroup.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", envelopeGroup.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            finally
            {
                _connection.Close();
                MultiThreadLock.SemaphoreSlim.Release();
            }
        }
    }
}
