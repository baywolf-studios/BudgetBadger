using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Models;
using Microsoft.Data.Sqlite;

namespace BudgetBadger.DataAccess.Sqlite
{
    public class EnvelopeSqliteDataAccess : IEnvelopeDataAccess
    {
        readonly string ConnectionString;

        public EnvelopeSqliteDataAccess(string connectionString)
        {
            ConnectionString = connectionString;
            Initialize();
        }

        void Initialize()
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                db.Open();
                var command = db.CreateCommand();

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
            }
        }

        public async Task CreateBudgetAsync(Budget budget)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"INSERT INTO Budget
                                                    (Id, 
                                                     Amount, 
                                                     BudgetScheduleId, 
                                                     EnvelopeId, 
                                                     CreatedDateTime, 
                                                     ModifiedDateTime, 
                                                     DeletedDateTime)  
                                        VALUES     (@Id, 
                                                    @Amount, 
                                                    @BudgetScheduleId, 
                                                    @EnvelopeId, 
                                                    @CreatedDateTime, 
                                                    @ModifiedDateTime, 
                                                    @DeletedDateTime)";

                command.Parameters.AddWithValue("@Id", budget.Id);
                command.Parameters.AddWithValue("@Amount", budget.Amount);
                command.Parameters.AddWithValue("@BudgetScheduleId", budget.Schedule?.Id);
                command.Parameters.AddWithValue("@EnvelopeId", budget.Envelope?.Id);
                command.Parameters.AddWithValue("@CreatedDateTime", budget.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", budget.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", budget.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task CreateBudgetScheduleAsync(BudgetSchedule budgetSchedule)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"INSERT INTO BudgetSchedule
                                                    (Id, 
                                                     BeginDate,
                                                     EndDate, 
                                                     CreatedDateTime, 
                                                     ModifiedDateTime, 
                                                     DeletedDateTime) 
                                        VALUES     (@Id, 
                                                    @BeginDate, 
                                                    @EndDate, 
                                                    @CreatedDateTime, 
                                                    @ModifiedDateTime, 
                                                    @DeletedDateTime)";

                command.Parameters.AddWithValue("@Id", budgetSchedule.Id);
                command.Parameters.AddWithValue("@BeginDate", budgetSchedule.BeginDate);
                command.Parameters.AddWithValue("@EndDate", budgetSchedule.EndDate);
                command.Parameters.AddWithValue("@CreatedDateTime", budgetSchedule.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", budgetSchedule.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", budgetSchedule.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task CreateEnvelopeAsync(Envelope envelope)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"INSERT INTO Envelope 
                                                    (Id, 
                                                     Description, 
                                                     EnvelopeGroupId, 
                                                     Notes, 
                                                     CreatedDateTime, 
                                                     ModifiedDateTime, 
                                                     DeletedDateTime) 
                                        VALUES     (@Id, 
                                                    @Description, 
                                                    @EnvelopeGroupId,
                                                    @Notes, 
                                                    @CreatedDateTime, 
                                                    @ModifiedDateTime, 
                                                    @DeletedDateTime)";

                command.Parameters.AddWithValue("@Id", envelope.Id);
                command.Parameters.AddWithValue("@Description", envelope.Description);
                command.Parameters.AddWithValue("@EnvelopeGroupId", envelope.Group?.Id);
                command.Parameters.AddWithValue("@Notes", envelope.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedDateTime", envelope.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", envelope.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", envelope.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task CreateEnvelopeGroupAsync(EnvelopeGroup envelopeGroup)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

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

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteBudgetAsync(Guid id)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"DELETE Budget WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteBudgetScheduleAsync(Guid id)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"DELETE BudgetSchedule WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteEnvelopeAsync(Guid id)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"DELETE Envelope WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteEnvelopeGroupAsync(Guid id)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"DELETE EnvelopeGroup WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<Budget> ReadBudgetAsync(Guid id)
        {
            var budget = new Budget();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT B.Id, 
                                               B.Amount, 
                                               B.CreatedDateTime, 
                                               B.ModifiedDateTime, 
                                               B.DeletedDateTime,  
                                               B.BudgetScheduleId, 
                                               BS.BeginDate        AS BudgetScheduleBeginDate, 
                                               BS.EndDate          AS BudgetScheduleEndDate,
                                               BS.CreatedDateTime  AS BudgetScheduleCreatedDateTime, 
                                               BS.ModifiedDateTime AS BudgetScheduleModifiedDateTime, 
                                               BS.DeletedDateTime  AS BudgetScheduleDeletedDateTime,  
                                               B.EnvelopeId, 
                                               E.Description       AS EnvelopeDescription, 
                                               E.Notes             AS EnvelopeNotes, 
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

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        budget = new Budget()
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"]),
                            Schedule = new BudgetSchedule
                            {
                                Id = new Guid(reader["BudgetScheduleId"] as byte[]),
                                BeginDate = Convert.ToDateTime(reader["BudgetScheduleBeginDate"]),
                                EndDate = Convert.ToDateTime(reader["BudgetScheduleEndDate"]),
                                CreatedDateTime = Convert.ToDateTime(reader["BudgetScheduleCreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["BudgetScheduleModifiedDateTime"]),
                                DeletedDateTime = reader["BudgetScheduleDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["BudgetScheduleDeletedDateTime"])
                            },
                            Envelope = new Envelope
                            {
                                Id = new Guid(reader["EnvelopeId"] as byte[]),
                                Description = reader["EnvelopeDescription"].ToString(),
                                Notes = reader["EnvelopeNotes"].ToString(),
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

            return budget;
        }

        public async Task<IEnumerable<Budget>> ReadBudgetsAsync()
        {
            var budgets = new List<Budget>();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT B.Id, 
                                               B.Amount, 
                                               B.CreatedDateTime, 
                                               B.ModifiedDateTime, 
                                               B.DeletedDateTime,  
                                               B.BudgetScheduleId, 
                                               BS.BeginDate        AS BudgetScheduleBeginDate, 
                                               BS.EndDate          AS BudgetScheduleEndDate,
                                               BS.CreatedDateTime  AS BudgetScheduleCreatedDateTime, 
                                               BS.ModifiedDateTime AS BudgetScheduleModifiedDateTime, 
                                               BS.DeletedDateTime  AS BudgetScheduleDeletedDateTime,  
                                               B.EnvelopeId, 
                                               E.Description       AS EnvelopeDescription, 
                                               E.Notes             AS EnvelopeNotes, 
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

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        budgets.Add(new Budget()
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"]),
                            Schedule = new BudgetSchedule
                            {
                                Id = new Guid(reader["BudgetScheduleId"] as byte[]),
                                BeginDate = Convert.ToDateTime(reader["BudgetScheduleBeginDate"]),
                                EndDate = Convert.ToDateTime(reader["BudgetScheduleEndDate"]),
                                CreatedDateTime = Convert.ToDateTime(reader["BudgetScheduleCreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["BudgetScheduleModifiedDateTime"]),
                                DeletedDateTime = reader["BudgetScheduleDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["BudgetScheduleDeletedDateTime"])
                            },
                            Envelope = new Envelope
                            {
                                Id = new Guid(reader["EnvelopeId"] as byte[]),
                                Description = reader["EnvelopeDescription"].ToString(),
                                Notes = reader["EnvelopeNotes"].ToString(),
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

            return budgets;
        }

        public async Task<IEnumerable<Budget>> ReadBudgetsFromScheduleAsync(Guid scheduleId)

        {
            var budgets = new List<Budget>();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT B.Id, 
                                               B.Amount, 
                                               B.CreatedDateTime, 
                                               B.ModifiedDateTime, 
                                               B.DeletedDateTime,  
                                               B.BudgetScheduleId, 
                                               BS.BeginDate        AS BudgetScheduleBeginDate, 
                                               BS.EndDate          AS BudgetScheduleEndDate,
                                               BS.CreatedDateTime  AS BudgetScheduleCreatedDateTime, 
                                               BS.ModifiedDateTime AS BudgetScheduleModifiedDateTime, 
                                               BS.DeletedDateTime  AS BudgetScheduleDeletedDateTime,  
                                               B.EnvelopeId, 
                                               E.Description       AS EnvelopeDescription, 
                                               E.Notes             AS EnvelopeNotes, 
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

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        budgets.Add(new Budget()
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                            ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"]),
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"]),
                            Schedule = new BudgetSchedule
                            {
                                Id = new Guid(reader["BudgetScheduleId"] as byte[]),
                                BeginDate = Convert.ToDateTime(reader["BudgetScheduleBeginDate"]),
                                EndDate = Convert.ToDateTime(reader["BudgetScheduleEndDate"]),
                                CreatedDateTime = Convert.ToDateTime(reader["BudgetScheduleCreatedDateTime"]),
                                ModifiedDateTime = Convert.ToDateTime(reader["BudgetScheduleModifiedDateTime"]),
                                DeletedDateTime = reader["BudgetScheduleDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["BudgetScheduleDeletedDateTime"])
                            },
                            Envelope = new Envelope
                            {
                                Id = new Guid(reader["EnvelopeId"] as byte[]),
                                Description = reader["EnvelopeDescription"].ToString(),
                                Notes = reader["EnvelopeNotes"].ToString(),
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

            return budgets;
        }

        public async Task<BudgetSchedule> ReadBudgetScheduleAsync(Guid id)
        {
            var budgetSchedule = new BudgetSchedule();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT Id,
                                               BeginDate, 
                                               EndDate,
                                               CreatedDateTime,
                                               ModifiedDateTime,
                                               DeletedDateTime
                                        FROM   BudgetSchedule
                                        WHERE  Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync())
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
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                        };
                    }
                }
            }

            return budgetSchedule;
        }

        public async Task<IEnumerable<BudgetSchedule>> ReadBudgetSchedulesAsync()
        {
            var budgetSchedules = new List<BudgetSchedule>();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT Id,
                                               BeginDate, 
                                               EndDate,
                                               CreatedDateTime,
                                               ModifiedDateTime,
                                               DeletedDateTime
                                        FROM   BudgetSchedule";

                using (var reader = await command.ExecuteReaderAsync())
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
                            DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"])
                        });
                    }
                }
            }

            return budgetSchedules;
        }

        public async Task<Envelope> ReadEnvelopeAsync(Guid id)
        {
            var envelope = new Envelope();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT E.Id, 
                                               E.Description, 
                                               E.Notes, 
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

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        envelope = new Envelope
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Description = reader["Description"].ToString(),
                            Notes = reader["Notes"].ToString(),
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

            return envelope;
        }

        public async Task<EnvelopeGroup> ReadEnvelopeGroupAsync(Guid id)
        {
            var envelopeGroup = new EnvelopeGroup();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT Id,
                                               Description,
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime, 
                                        FROM   EnvelopeGroup
                                        WHERE  Id = @Id";

                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync())
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

            return envelopeGroup;
        }

        public async Task<IEnumerable<EnvelopeGroup>> ReadEnvelopeGroupsAsync()
        {
            var envelopeGroups = new List<EnvelopeGroup>();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT Id,
                                               Description,
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime
                                        FROM   EnvelopeGroup";

                using (var reader = await command.ExecuteReaderAsync())
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

            return envelopeGroups;
        }

        public async Task<IEnumerable<Envelope>> ReadEnvelopesAsync()
        {
            var envelopes = new List<Envelope>();

            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"SELECT E.Id, 
                                               E.Description, 
                                               E.Notes, 
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

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        envelopes.Add(new Envelope
                        {
                            Id = new Guid(reader["Id"] as byte[]),
                            Description = reader["Description"].ToString(),
                            Notes = reader["Notes"].ToString(),
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

            return envelopes;
        }

        public async Task UpdateBudgetAsync(Budget budget)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"UPDATE Budget 
                                        SET    Amount = @Amount, 
                                               EnvelopeId = @EnvelopeId, 
                                               BudgetScheduleId = @BudgetScheduleId,
                                               CreatedDateTime = @CreatedDateTime,
                                               ModifiedDateTime = @ModifiedDateTime,
                                               DeletedDateTime = @DeletedDateTime
                                        WHERE  Id = @Id";

                command.Parameters.AddWithValue("@Id", budget.Id);
                command.Parameters.AddWithValue("@Amount", budget.Amount);
                command.Parameters.AddWithValue("@EnvelopeId", budget.Envelope?.Id);
                command.Parameters.AddWithValue("@BudgetScheduleId", budget.Schedule?.Id);
                command.Parameters.AddWithValue("@CreatedDateTime", budget.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", budget.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", budget.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateBudgetScheduleAsync(BudgetSchedule budgetSchedule)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"UPDATE BudgetSchedule
                                        SET    BeginDate = @BeginDate, 
                                               EndDate = @EndDate,
                                               CreatedDateTime = @CreatedDateTime,
                                               ModifiedDateTime = @ModifiedDateTime,
                                               DeletedDateTime = @DeletedDateTime
                                        WHERE  Id = @Id";

                command.Parameters.AddWithValue("@Id", budgetSchedule.Id);
                command.Parameters.AddWithValue("@BeginDate", budgetSchedule.BeginDate);
                command.Parameters.AddWithValue("@EndDate", budgetSchedule.EndDate);
                command.Parameters.AddWithValue("@CreatedDateTime", budgetSchedule.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", budgetSchedule.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", budgetSchedule.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateEnvelopeAsync(Envelope envelope)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

                command.CommandText = @"UPDATE Envelope 
                                        SET    Description = @Description,
                                               EnvelopeGroupId = @EnvelopeGroupId,
                                               Notes = @Notes,
                                               CreatedDateTime = @CreatedDateTime, 
                                               ModifiedDateTime = @ModifiedDateTime, 
                                               DeletedDateTime = @DeletedDateTime 
                                        WHERE  Id = @Id ";

                command.Parameters.AddWithValue("@Id", envelope.Id);
                command.Parameters.AddWithValue("@Description", envelope.Description);
                command.Parameters.AddWithValue("@EnvelopeGroupId", envelope.Group?.Id);
                command.Parameters.AddWithValue("@Notes", envelope.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedDateTime", envelope.CreatedDateTime);
                command.Parameters.AddWithValue("@ModifiedDateTime", envelope.ModifiedDateTime);
                command.Parameters.AddWithValue("@DeletedDateTime", envelope.DeletedDateTime ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateEnvelopeGroupAsync(EnvelopeGroup envelopeGroup)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();

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

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
