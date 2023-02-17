using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BudgetBadger.Core.Utilities;
using BudgetBadger.Core.Models;
using Microsoft.Data.Sqlite;
using BudgetBadger.Core.Dtos;
using System.Linq;

namespace BudgetBadger.DataAccess.Sqlite
{
    public partial class SqliteDataAccess
    {
        public async Task CreateBudgetAsync(Budget budget)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

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

                        command.Parameters.AddWithValue("@Id", budget.Id.ToByteArray());
                        command.Parameters.AddWithValue("@Amount", budget.Amount);
                        command.Parameters.AddWithValue("@IgnoreOverspend", budget.IgnoreOverspend);
                        command.Parameters.AddWithValue("@BudgetScheduleId", budget.Schedule?.Id.ToByteArray());
                        command.Parameters.AddWithValue("@EnvelopeId", budget.Envelope?.Id.ToByteArray());
                        command.Parameters.AddWithValue("@CreatedDateTime", budget.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", budget.ModifiedDateTime);

                        command.ExecuteNonQuery();
                    }
                });
            }
            
        }

        public async Task CreateBudgetScheduleAsync(BudgetSchedule budgetSchedule)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

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

                        command.Parameters.AddWithValue("@Id", budgetSchedule.Id.ToByteArray());
                        command.Parameters.AddWithValue("@BeginDate", budgetSchedule.BeginDate);
                        command.Parameters.AddWithValue("@EndDate", budgetSchedule.EndDate);
                        command.Parameters.AddWithValue("@CreatedDateTime", budgetSchedule.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", budgetSchedule.ModifiedDateTime);

                        command.ExecuteNonQuery();
                    }
                });
            }
            
        }

        public async Task CreateEnvelopeAsync(Envelope envelope)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"INSERT INTO Envelope 
                                                (Id, 
                                                 Description, 
                                                 EnvelopeGroupId, 
                                                 Notes, 
                                                 IgnoreOverspend,
                                                 CreatedDateTime, 
                                                 ModifiedDateTime, 
                                                 DeletedDateTime,
                                                 HiddenDateTime) 
                                    VALUES     (@Id, 
                                                @Description, 
                                                @EnvelopeGroupId,
                                                @Notes, 
                                                @IgnoreOverspend,
                                                @CreatedDateTime, 
                                                @ModifiedDateTime, 
                                                @DeletedDateTime,
                                                @HiddenDateTime)";

                        command.Parameters.AddWithValue("@Id", envelope.Id.ToByteArray());
                        command.Parameters.AddWithValue("@Description", envelope.Description);
                        command.Parameters.AddWithValue("@EnvelopeGroupId", envelope.Group?.Id.ToByteArray());
                        command.Parameters.AddWithValue("@Notes", envelope.Notes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@IgnoreOverspend", envelope.IgnoreOverspend);
                        command.Parameters.AddWithValue("@CreatedDateTime", envelope.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", envelope.ModifiedDateTime);
                        command.Parameters.AddWithValue("@DeletedDateTime", envelope.DeletedDateTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@HiddenDateTime", envelope.HiddenDateTime ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                });
            }
            
        }

        public async Task CreateEnvelopeGroupAsync(EnvelopeGroup envelopeGroup)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"INSERT INTO EnvelopeGroup
                                                (Id, 
                                                 Description, 
                                                 Notes, 
                                                 CreatedDateTime, 
                                                 ModifiedDateTime, 
                                                 DeletedDateTime,
                                                 HiddenDateTime) 
                                    VALUES     (@Id, 
                                                @Description, 
                                                @Notes, 
                                                @CreatedDateTime, 
                                                @ModifiedDateTime, 
                                                @DeletedDateTime,
                                                @HiddenDateTime)";

                        command.Parameters.AddWithValue("@Id", envelopeGroup.Id.ToByteArray());
                        command.Parameters.AddWithValue("@Description", envelopeGroup.Description);
                        command.Parameters.AddWithValue("@Notes", envelopeGroup.Notes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedDateTime", envelopeGroup.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", envelopeGroup.ModifiedDateTime);
                        command.Parameters.AddWithValue("@DeletedDateTime", envelopeGroup.DeletedDateTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@HiddenDateTime", envelopeGroup.HiddenDateTime ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task<Budget> ReadBudgetAsync(Guid id)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var budget = new Budget();
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

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
                                           E.HiddenDateTime    AS EnvelopeHiddenDateTime,
                                           EG.Id               AS EnvelopeGroupId, 
                                           EG.Description      AS EnvelopeGroupDescription,
                                           EG.Notes            AS EnvelopeGroupNotes, 
                                           EG.CreatedDateTime  AS EnvelopeGroupCreatedDateTime, 
                                           EG.ModifiedDateTime AS EnvelopeGroupModifiedDateTime, 
                                           EG.DeletedDateTime  AS EnvelopeGroupDeletedDateTime,
                                           EG.HiddenDateTime   AS EnvelopeGroupHiddenDateTime
                                    FROM   Budget AS B 
                                    JOIN   BudgetSchedule AS BS ON B.BudgetScheduleId = BS.Id
                                    JOIN   Envelope AS E ON B.EnvelopeId = E.Id
                                    JOIN   EnvelopeGroup EG ON E.EnvelopeGroupId = EG.Id
                                    WHERE  B.Id = @Id";

                        command.Parameters.AddWithValue("@Id", id.ToByteArray());

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                budget = new Budget()
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Amount = Convert.ToDecimal(reader["Amount"], CultureInfo.InvariantCulture),
                                    IgnoreOverspend = Convert.ToBoolean(reader["IgnoreOverspend"], CultureInfo.InvariantCulture),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    Schedule = new BudgetSchedule
                                    {
                                        Id = new Guid(reader["BudgetScheduleId"] as byte[]),
                                        BeginDate = Convert.ToDateTime(reader["BudgetScheduleBeginDate"], CultureInfo.InvariantCulture),
                                        EndDate = Convert.ToDateTime(reader["BudgetScheduleEndDate"], CultureInfo.InvariantCulture),
                                        CreatedDateTime = Convert.ToDateTime(reader["BudgetScheduleCreatedDateTime"], CultureInfo.InvariantCulture),
                                        ModifiedDateTime = Convert.ToDateTime(reader["BudgetScheduleModifiedDateTime"], CultureInfo.InvariantCulture),
                                    },
                                    Envelope = new Envelope
                                    {
                                        Id = new Guid(reader["EnvelopeId"] as byte[]),
                                        Description = reader["EnvelopeDescription"].ToString(),
                                        Notes = reader["EnvelopeNotes"].ToString(),
                                        IgnoreOverspend = Convert.ToBoolean(reader["EnvelopeIgnoreOverspend"], CultureInfo.InvariantCulture),
                                        CreatedDateTime = Convert.ToDateTime(reader["EnvelopeCreatedDateTime"], CultureInfo.InvariantCulture),
                                        ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeModifiedDateTime"], CultureInfo.InvariantCulture),
                                        DeletedDateTime = reader["EnvelopeDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeDeletedDateTime"], CultureInfo.InvariantCulture),
                                        HiddenDateTime = reader["EnvelopeHiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeHiddenDateTime"], CultureInfo.InvariantCulture),
                                        Group = new EnvelopeGroup
                                        {
                                            Id = new Guid(reader["EnvelopeGroupId"] as byte[]),
                                            Description = reader["EnvelopeGroupDescription"].ToString(),
                                            Notes = reader["EnvelopeGroupNotes"].ToString(),
                                            CreatedDateTime = Convert.ToDateTime(reader["EnvelopeGroupCreatedDateTime"], CultureInfo.InvariantCulture),
                                            ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeGroupModifiedDateTime"], CultureInfo.InvariantCulture),
                                            DeletedDateTime = reader["EnvelopeGroupDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupDeletedDateTime"], CultureInfo.InvariantCulture),
                                            HiddenDateTime = reader["EnvelopeGroupHiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupHiddenDateTime"], CultureInfo.InvariantCulture)
                                        }
                                    }
                                };
                            }
                        }
                    }

                    return budget;
                });
            }
            
        }

        public async Task<IReadOnlyList<Budget>> ReadBudgetsAsync()
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var budgets = new List<Budget>();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

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
                                           E.HiddenDateTime    AS EnvelopeHiddenDateTime,
                                           EG.Id               AS EnvelopeGroupId, 
                                           EG.Description      AS EnvelopeGroupDescription,
                                           EG.Notes            AS EnvelopeGroupNotes, 
                                           EG.CreatedDateTime  AS EnvelopeGroupCreatedDateTime, 
                                           EG.ModifiedDateTime AS EnvelopeGroupModifiedDateTime, 
                                           EG.DeletedDateTime  AS EnvelopeGroupDeletedDateTime,
                                           EG.HiddenDateTime   AS EnvelopeGroupHiddenDateTime
                                    FROM   Budget AS B 
                                    JOIN   BudgetSchedule AS BS ON B.BudgetScheduleId = BS.Id
                                    JOIN   Envelope AS E ON B.EnvelopeId = E.Id
                                    JOIN   EnvelopeGroup EG ON E.EnvelopeGroupId = EG.Id";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                budgets.Add(new Budget()
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Amount = Convert.ToDecimal(reader["Amount"], CultureInfo.InvariantCulture),
                                    IgnoreOverspend = Convert.ToBoolean(reader["IgnoreOverspend"], CultureInfo.InvariantCulture),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    Schedule = new BudgetSchedule
                                    {
                                        Id = new Guid(reader["BudgetScheduleId"] as byte[]),
                                        BeginDate = Convert.ToDateTime(reader["BudgetScheduleBeginDate"], CultureInfo.InvariantCulture),
                                        EndDate = Convert.ToDateTime(reader["BudgetScheduleEndDate"], CultureInfo.InvariantCulture),
                                        CreatedDateTime = Convert.ToDateTime(reader["BudgetScheduleCreatedDateTime"], CultureInfo.InvariantCulture),
                                        ModifiedDateTime = Convert.ToDateTime(reader["BudgetScheduleModifiedDateTime"], CultureInfo.InvariantCulture),
                                    },
                                    Envelope = new Envelope
                                    {
                                        Id = new Guid(reader["EnvelopeId"] as byte[]),
                                        Description = reader["EnvelopeDescription"].ToString(),
                                        Notes = reader["EnvelopeNotes"].ToString(),
                                        IgnoreOverspend = Convert.ToBoolean(reader["EnvelopeIgnoreOverspend"], CultureInfo.InvariantCulture),
                                        CreatedDateTime = Convert.ToDateTime(reader["EnvelopeCreatedDateTime"], CultureInfo.InvariantCulture),
                                        ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeModifiedDateTime"], CultureInfo.InvariantCulture),
                                        DeletedDateTime = reader["EnvelopeDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeDeletedDateTime"], CultureInfo.InvariantCulture),
                                        HiddenDateTime = reader["EnvelopeHiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeHiddenDateTime"], CultureInfo.InvariantCulture),
                                        Group = new EnvelopeGroup
                                        {
                                            Id = new Guid(reader["EnvelopeGroupId"] as byte[]),
                                            Description = reader["EnvelopeGroupDescription"].ToString(),
                                            Notes = reader["EnvelopeGroupNotes"].ToString(),
                                            CreatedDateTime = Convert.ToDateTime(reader["EnvelopeGroupCreatedDateTime"], CultureInfo.InvariantCulture),
                                            ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeGroupModifiedDateTime"], CultureInfo.InvariantCulture),
                                            DeletedDateTime = reader["EnvelopeGroupDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupDeletedDateTime"], CultureInfo.InvariantCulture),
                                            HiddenDateTime = reader["EnvelopeGroupHiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupHiddenDateTime"], CultureInfo.InvariantCulture)
                                        }
                                    }
                                });
                            }
                        }
                    }

                    return budgets;
                });
            }
            
        }

        public async Task<IReadOnlyList<Budget>> ReadBudgetsFromScheduleAsync(Guid scheduleId)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var budgets = new List<Budget>();
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

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
                                           E.HiddenDateTime    AS EnvelopeHiddenDateTime,
                                           EG.Id               AS EnvelopeGroupId, 
                                           EG.Description      AS EnvelopeGroupDescription,
                                           EG.Notes            AS EnvelopeGroupNotes, 
                                           EG.CreatedDateTime  AS EnvelopeGroupCreatedDateTime, 
                                           EG.ModifiedDateTime AS EnvelopeGroupModifiedDateTime, 
                                           EG.DeletedDateTime  AS EnvelopeGroupDeletedDateTime,
                                           EG.HiddenDateTime   AS EnvelopeGroupHiddenDateTime
                                    FROM   Budget AS B 
                                    JOIN   BudgetSchedule BS ON B.BudgetScheduleId = BS.Id
                                    JOIN   Envelope E ON B.EnvelopeId = E.Id
                                    JOIN   EnvelopeGroup EG ON E.EnvelopeGroupId = EG.Id
                                    WHERE  BS.Id = @ScheduleId";

                        command.Parameters.AddWithValue("@ScheduleId", scheduleId.ToByteArray());

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                budgets.Add(new Budget()
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Amount = Convert.ToDecimal(reader["Amount"], CultureInfo.InvariantCulture),
                                    IgnoreOverspend = Convert.ToBoolean(reader["IgnoreOverspend"], CultureInfo.InvariantCulture),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    Schedule = new BudgetSchedule
                                    {
                                        Id = new Guid(reader["BudgetScheduleId"] as byte[]),
                                        BeginDate = Convert.ToDateTime(reader["BudgetScheduleBeginDate"], CultureInfo.InvariantCulture),
                                        EndDate = Convert.ToDateTime(reader["BudgetScheduleEndDate"], CultureInfo.InvariantCulture),
                                        CreatedDateTime = Convert.ToDateTime(reader["BudgetScheduleCreatedDateTime"], CultureInfo.InvariantCulture),
                                        ModifiedDateTime = Convert.ToDateTime(reader["BudgetScheduleModifiedDateTime"], CultureInfo.InvariantCulture),
                                    },
                                    Envelope = new Envelope
                                    {
                                        Id = new Guid(reader["EnvelopeId"] as byte[]),
                                        Description = reader["EnvelopeDescription"].ToString(),
                                        Notes = reader["EnvelopeNotes"].ToString(),
                                        IgnoreOverspend = Convert.ToBoolean(reader["EnvelopeIgnoreOverspend"], CultureInfo.InvariantCulture),
                                        CreatedDateTime = Convert.ToDateTime(reader["EnvelopeCreatedDateTime"], CultureInfo.InvariantCulture),
                                        ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeModifiedDateTime"], CultureInfo.InvariantCulture),
                                        DeletedDateTime = reader["EnvelopeDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeDeletedDateTime"], CultureInfo.InvariantCulture),
                                        HiddenDateTime = reader["EnvelopeHiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeHiddenDateTime"], CultureInfo.InvariantCulture),
                                        Group = new EnvelopeGroup
                                        {
                                            Id = new Guid(reader["EnvelopeGroupId"] as byte[]),
                                            Description = reader["EnvelopeGroupDescription"].ToString(),
                                            Notes = reader["EnvelopeGroupNotes"].ToString(),
                                            CreatedDateTime = Convert.ToDateTime(reader["EnvelopeGroupCreatedDateTime"], CultureInfo.InvariantCulture),
                                            ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeGroupModifiedDateTime"], CultureInfo.InvariantCulture),
                                            DeletedDateTime = reader["EnvelopeGroupDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupDeletedDateTime"], CultureInfo.InvariantCulture),
                                            HiddenDateTime = reader["EnvelopeGroupHiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupHiddenDateTime"], CultureInfo.InvariantCulture)
                                        }
                                    }
                                });
                            }
                        }
                    }
                    return budgets;
                });
            }
            
        }

        public async Task<IReadOnlyList<Budget>> ReadBudgetsFromEnvelopeAsync(Guid envelopeId)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var budgets = new List<Budget>();
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

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
                                           E.HiddenDateTime    AS EnvelopeHiddenDateTime,
                                           EG.Id               AS EnvelopeGroupId, 
                                           EG.Description      AS EnvelopeGroupDescription,
                                           EG.Notes            AS EnvelopeGroupNotes, 
                                           EG.CreatedDateTime  AS EnvelopeGroupCreatedDateTime, 
                                           EG.ModifiedDateTime AS EnvelopeGroupModifiedDateTime, 
                                           EG.DeletedDateTime  AS EnvelopeGroupDeletedDateTime,
                                           EG.HiddenDateTime   AS EnvelopeGroupHiddenDateTime
                                    FROM   Budget AS B 
                                    JOIN   BudgetSchedule BS ON B.BudgetScheduleId = BS.Id
                                    JOIN   Envelope E ON B.EnvelopeId = E.Id
                                    JOIN   EnvelopeGroup EG ON E.EnvelopeGroupId = EG.Id
                                    WHERE  E.Id = @EnvelopeId";

                        command.Parameters.AddWithValue("@EnvelopeId", envelopeId.ToByteArray());

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                budgets.Add(new Budget()
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Amount = Convert.ToDecimal(reader["Amount"], CultureInfo.InvariantCulture),
                                    IgnoreOverspend = Convert.ToBoolean(reader["IgnoreOverspend"], CultureInfo.InvariantCulture),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    Schedule = new BudgetSchedule
                                    {
                                        Id = new Guid(reader["BudgetScheduleId"] as byte[]),
                                        BeginDate = Convert.ToDateTime(reader["BudgetScheduleBeginDate"], CultureInfo.InvariantCulture),
                                        EndDate = Convert.ToDateTime(reader["BudgetScheduleEndDate"], CultureInfo.InvariantCulture),
                                        CreatedDateTime = Convert.ToDateTime(reader["BudgetScheduleCreatedDateTime"], CultureInfo.InvariantCulture),
                                        ModifiedDateTime = Convert.ToDateTime(reader["BudgetScheduleModifiedDateTime"], CultureInfo.InvariantCulture),
                                    },
                                    Envelope = new Envelope
                                    {
                                        Id = new Guid(reader["EnvelopeId"] as byte[]),
                                        Description = reader["EnvelopeDescription"].ToString(),
                                        Notes = reader["EnvelopeNotes"].ToString(),
                                        IgnoreOverspend = Convert.ToBoolean(reader["EnvelopeIgnoreOverspend"], CultureInfo.InvariantCulture),
                                        CreatedDateTime = Convert.ToDateTime(reader["EnvelopeCreatedDateTime"], CultureInfo.InvariantCulture),
                                        ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeModifiedDateTime"], CultureInfo.InvariantCulture),
                                        DeletedDateTime = reader["EnvelopeDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeDeletedDateTime"], CultureInfo.InvariantCulture),
                                        HiddenDateTime = reader["EnvelopeHiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeHiddenDateTime"], CultureInfo.InvariantCulture),
                                        Group = new EnvelopeGroup
                                        {
                                            Id = new Guid(reader["EnvelopeGroupId"] as byte[]),
                                            Description = reader["EnvelopeGroupDescription"].ToString(),
                                            Notes = reader["EnvelopeGroupNotes"].ToString(),
                                            CreatedDateTime = Convert.ToDateTime(reader["EnvelopeGroupCreatedDateTime"], CultureInfo.InvariantCulture),
                                            ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeGroupModifiedDateTime"], CultureInfo.InvariantCulture),
                                            DeletedDateTime = reader["EnvelopeGroupDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupDeletedDateTime"], CultureInfo.InvariantCulture),
                                            HiddenDateTime = reader["EnvelopeGroupHiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupHiddenDateTime"], CultureInfo.InvariantCulture)
                                        }
                                    }
                                });
                            }
                        }
                    }

                    return budgets;
                });
            }
            
        }

        public async Task<Budget> ReadBudgetFromScheduleAndEnvelopeAsync(Guid scheduleId, Guid envelopeId)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var budget = new Budget();
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

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
                                           E.HiddenDateTime    AS EnvelopeHiddenDateTime,
                                           EG.Id               AS EnvelopeGroupId, 
                                           EG.Description      AS EnvelopeGroupDescription,
                                           EG.Notes            AS EnvelopeGroupNotes, 
                                           EG.CreatedDateTime  AS EnvelopeGroupCreatedDateTime, 
                                           EG.ModifiedDateTime AS EnvelopeGroupModifiedDateTime, 
                                           EG.DeletedDateTime  AS EnvelopeGroupDeletedDateTime,
                                           EG.HiddenDateTime   AS EnvelopeGroupHiddenDateTime
                                    FROM   Budget AS B 
                                    JOIN   BudgetSchedule BS ON B.BudgetScheduleId = BS.Id
                                    JOIN   Envelope E ON B.EnvelopeId = E.Id
                                    JOIN   EnvelopeGroup EG ON E.EnvelopeGroupId = EG.Id
                                    WHERE  E.Id = @EnvelopeId AND BS.Id = @ScheduleId";

                        command.Parameters.AddWithValue("@EnvelopeId", envelopeId.ToByteArray());
                        command.Parameters.AddWithValue("@ScheduleId", scheduleId.ToByteArray());

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                budget = new Budget()
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Amount = Convert.ToDecimal(reader["Amount"], CultureInfo.InvariantCulture),
                                    IgnoreOverspend = Convert.ToBoolean(reader["IgnoreOverspend"], CultureInfo.InvariantCulture),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    Schedule = new BudgetSchedule
                                    {
                                        Id = new Guid(reader["BudgetScheduleId"] as byte[]),
                                        BeginDate = Convert.ToDateTime(reader["BudgetScheduleBeginDate"], CultureInfo.InvariantCulture),
                                        EndDate = Convert.ToDateTime(reader["BudgetScheduleEndDate"], CultureInfo.InvariantCulture),
                                        CreatedDateTime = Convert.ToDateTime(reader["BudgetScheduleCreatedDateTime"], CultureInfo.InvariantCulture),
                                        ModifiedDateTime = Convert.ToDateTime(reader["BudgetScheduleModifiedDateTime"], CultureInfo.InvariantCulture),
                                    },
                                    Envelope = new Envelope
                                    {
                                        Id = new Guid(reader["EnvelopeId"] as byte[]),
                                        Description = reader["EnvelopeDescription"].ToString(),
                                        Notes = reader["EnvelopeNotes"].ToString(),
                                        IgnoreOverspend = Convert.ToBoolean(reader["EnvelopeIgnoreOverspend"], CultureInfo.InvariantCulture),
                                        CreatedDateTime = Convert.ToDateTime(reader["EnvelopeCreatedDateTime"], CultureInfo.InvariantCulture),
                                        ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeModifiedDateTime"], CultureInfo.InvariantCulture),
                                        DeletedDateTime = reader["EnvelopeDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeDeletedDateTime"], CultureInfo.InvariantCulture),
                                        HiddenDateTime = reader["EnvelopeHiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeHiddenDateTime"], CultureInfo.InvariantCulture),
                                        Group = new EnvelopeGroup
                                        {
                                            Id = new Guid(reader["EnvelopeGroupId"] as byte[]),
                                            Description = reader["EnvelopeGroupDescription"].ToString(),
                                            Notes = reader["EnvelopeGroupNotes"].ToString(),
                                            CreatedDateTime = Convert.ToDateTime(reader["EnvelopeGroupCreatedDateTime"], CultureInfo.InvariantCulture),
                                            ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeGroupModifiedDateTime"], CultureInfo.InvariantCulture),
                                            DeletedDateTime = reader["EnvelopeGroupDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupDeletedDateTime"], CultureInfo.InvariantCulture),
                                            HiddenDateTime = reader["EnvelopeGroupHiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupHiddenDateTime"], CultureInfo.InvariantCulture)
                                        }
                                    }
                                };
                            }
                        }
                    }

                    return budget;
                });
            }
        }

        public async Task<BudgetSchedule> ReadBudgetScheduleAsync(Guid id)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var budgetSchedule = new BudgetSchedule();
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT Id,
                                           BeginDate, 
                                           EndDate,
                                           CreatedDateTime,
                                           ModifiedDateTime
                                    FROM   BudgetSchedule
                                    WHERE  Id = @Id";

                        command.Parameters.AddWithValue("@Id", id.ToByteArray());

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                budgetSchedule = new BudgetSchedule
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    BeginDate = Convert.ToDateTime(reader["BeginDate"], CultureInfo.InvariantCulture),
                                    EndDate = Convert.ToDateTime(reader["EndDate"], CultureInfo.InvariantCulture),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                };
                            }
                        }
                    }

                    return budgetSchedule;
                });
            }
            
        }

        public async Task<IReadOnlyList<BudgetSchedule>> ReadBudgetSchedulesAsync()
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var budgetSchedules = new List<BudgetSchedule>();
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT Id,
                                           BeginDate, 
                                           EndDate,
                                           CreatedDateTime,
                                           ModifiedDateTime
                                    FROM   BudgetSchedule";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                budgetSchedules.Add(new BudgetSchedule
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    BeginDate = Convert.ToDateTime(reader["BeginDate"], CultureInfo.InvariantCulture),
                                    EndDate = Convert.ToDateTime(reader["EndDate"], CultureInfo.InvariantCulture),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                });
                            }
                        }
                    }

                    return budgetSchedules;
                });
            }
            
        }

        public async Task<Envelope> ReadEnvelopeAsync(Guid id)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var envelope = new Envelope();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT E.Id, 
                                           E.Description, 
                                           E.Notes, 
                                           E.IgnoreOverspend,
                                           E.CreatedDateTime, 
                                           E.ModifiedDateTime, 
                                           E.DeletedDateTime, 
                                           E.EnvelopeGroupId,
                                           E.HiddenDateTime,
                                           EG.Description      AS EnvelopeGroupDescription, 
                                           EG.Notes            AS EnvelopeGroupNotes, 
                                           EG.CreatedDateTime  AS EnvelopeGroupCreatedDateTime, 
                                           EG.ModifiedDateTime AS EnvelopeGroupModifiedDateTime, 
                                           EG.DeletedDateTime  AS EnvelopeGroupDeletedDateTime,
                                           EG.HiddenDateTime   AS EnvelopeGroupHiddenDateTime
                                    FROM   Envelope AS E
                                    JOIN   EnvelopeGroup AS EG ON E.EnvelopeGroupId = EG.Id
                                    WHERE  E.Id = @Id";

                        command.Parameters.AddWithValue("@Id", id.ToByteArray());

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                envelope = new Envelope
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Description = reader["Description"].ToString(),
                                    Notes = reader["Notes"].ToString(),
                                    IgnoreOverspend = Convert.ToBoolean(reader["IgnoreOverspend"], CultureInfo.InvariantCulture),
                                    Group = new EnvelopeGroup
                                    {
                                        Id = new Guid(reader["EnvelopeGroupId"] as byte[]),
                                        Description = reader["EnvelopeGroupDescription"].ToString(),
                                        Notes = reader["EnvelopeGroupNotes"].ToString(),
                                        CreatedDateTime = Convert.ToDateTime(reader["EnvelopeGroupCreatedDateTime"], CultureInfo.InvariantCulture),
                                        ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeGroupModifiedDateTime"], CultureInfo.InvariantCulture),
                                        DeletedDateTime = reader["EnvelopeGroupDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupDeletedDateTime"], CultureInfo.InvariantCulture),
                                        HiddenDateTime = reader["EnvelopeGroupHiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupHiddenDateTime"], CultureInfo.InvariantCulture)
                                    },
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture),
                                    HiddenDateTime = reader["HiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["HiddenDateTime"], CultureInfo.InvariantCulture)
                                };
                            }
                        }
                    }

                    return envelope;
                });
            }
            
        }

        public async Task<EnvelopeGroup> ReadEnvelopeGroupAsync(Guid id)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var envelopeGroup = new EnvelopeGroup();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT Id,
                                               Description,
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime,
                                               HiddenDateTime
                                        FROM   EnvelopeGroup
                                        WHERE  Id = @Id";

                        command.Parameters.AddWithValue("@Id", id.ToByteArray());

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                envelopeGroup = new EnvelopeGroup
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Description = reader["Description"].ToString(),
                                    Notes = reader["Notes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture),
                                    HiddenDateTime = reader["HiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["HiddenDateTime"], CultureInfo.InvariantCulture)
                                };
                            }
                        }
                    }

                    return envelopeGroup;
                });
            }
            
        }

        public async Task<IReadOnlyList<EnvelopeGroup>> ReadEnvelopeGroupsAsync()
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var envelopeGroups = new List<EnvelopeGroup>();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT Id,
                                               Description,
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime,
                                               HiddenDateTime
                                        FROM   EnvelopeGroup";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                envelopeGroups.Add(new EnvelopeGroup
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Description = reader["Description"].ToString(),
                                    Notes = reader["Notes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture),
                                    HiddenDateTime = reader["HiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["HiddenDateTime"], CultureInfo.InvariantCulture)
                                });
                            }
                        }
                    }

                    return envelopeGroups;
                });
            }
            
        }

        public async Task<IReadOnlyList<Envelope>> ReadEnvelopesAsync()
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var envelopes = new List<Envelope>();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT E.Id, 
                                               E.Description, 
                                               E.Notes, 
                                               E.IgnoreOverspend,
                                               E.CreatedDateTime, 
                                               E.ModifiedDateTime, 
                                               E.DeletedDateTime,
                                               E.HiddenDateTime,
                                               E.EnvelopeGroupId, 
                                               EG.Description      AS EnvelopeGroupDescription, 
                                               EG.Notes            AS EnvelopeGroupNotes, 
                                               EG.CreatedDateTime  AS EnvelopeGroupCreatedDateTime, 
                                               EG.ModifiedDateTime AS EnvelopeGroupModifiedDateTime, 
                                               EG.DeletedDateTime  AS EnvelopeGroupDeletedDateTime,
                                               EG.HiddenDateTime   AS EnvelopeGroupHiddenDateTime
                                        FROM   Envelope AS E
                                        JOIN   EnvelopeGroup AS EG ON E.EnvelopeGroupId = EG.Id";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                envelopes.Add(new Envelope
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Description = reader["Description"].ToString(),
                                    Notes = reader["Notes"].ToString(),
                                    IgnoreOverspend = Convert.ToBoolean(reader["IgnoreOverspend"], CultureInfo.InvariantCulture),
                                    Group = new EnvelopeGroup
                                    {
                                        Id = new Guid(reader["EnvelopeGroupId"] as byte[]),
                                        Description = reader["EnvelopeGroupDescription"].ToString(),
                                        Notes = reader["EnvelopeGroupNotes"].ToString(),
                                        CreatedDateTime = Convert.ToDateTime(reader["EnvelopeGroupCreatedDateTime"], CultureInfo.InvariantCulture),
                                        ModifiedDateTime = Convert.ToDateTime(reader["EnvelopeGroupModifiedDateTime"], CultureInfo.InvariantCulture),
                                        DeletedDateTime = reader["EnvelopeGroupDeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupDeletedDateTime"], CultureInfo.InvariantCulture),
                                        HiddenDateTime = reader["EnvelopeGroupHiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EnvelopeGroupHiddenDateTime"], CultureInfo.InvariantCulture)
                                    },
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture),
                                    HiddenDateTime = reader["HiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["HiddenDateTime"], CultureInfo.InvariantCulture)
                                });
                            }
                        }
                    }

                    return envelopes;
                });
            }
            
        }

        public async Task UpdateBudgetAsync(Budget budget)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"UPDATE Budget 
                                        SET    Amount = @Amount, 
                                               IgnoreOverspend = @IgnoreOverspend,
                                               EnvelopeId = @EnvelopeId, 
                                               BudgetScheduleId = @BudgetScheduleId,
                                               CreatedDateTime = @CreatedDateTime,
                                               ModifiedDateTime = @ModifiedDateTime
                                        WHERE  Id = @Id";

                        command.Parameters.AddWithValue("@Id", budget.Id.ToByteArray());
                        command.Parameters.AddWithValue("@Amount", budget.Amount);
                        command.Parameters.AddWithValue("@IgnoreOverspend", budget.IgnoreOverspend);
                        command.Parameters.AddWithValue("@EnvelopeId", budget.Envelope?.Id.ToByteArray());
                        command.Parameters.AddWithValue("@BudgetScheduleId", budget.Schedule?.Id.ToByteArray());
                        command.Parameters.AddWithValue("@CreatedDateTime", budget.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", budget.ModifiedDateTime);

                        command.ExecuteNonQuery();
                    }
                });
            }
            
        }

        public async Task UpdateBudgetScheduleAsync(BudgetSchedule budgetSchedule)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"UPDATE BudgetSchedule
                                        SET    BeginDate = @BeginDate, 
                                               EndDate = @EndDate,
                                               CreatedDateTime = @CreatedDateTime,
                                               ModifiedDateTime = @ModifiedDateTime
                                        WHERE  Id = @Id";

                        command.Parameters.AddWithValue("@Id", budgetSchedule.Id.ToByteArray());
                        command.Parameters.AddWithValue("@BeginDate", budgetSchedule.BeginDate);
                        command.Parameters.AddWithValue("@EndDate", budgetSchedule.EndDate);
                        command.Parameters.AddWithValue("@CreatedDateTime", budgetSchedule.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", budgetSchedule.ModifiedDateTime);

                        command.ExecuteNonQuery();
                    }
                });
            }
            
        }

        public async Task UpdateEnvelopeAsync(Envelope envelope)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"UPDATE Envelope 
                                        SET    Description = @Description,
                                               EnvelopeGroupId = @EnvelopeGroupId,
                                               Notes = @Notes,
                                               IgnoreOverspend = @IgnoreOverspend,
                                               CreatedDateTime = @CreatedDateTime, 
                                               ModifiedDateTime = @ModifiedDateTime, 
                                               DeletedDateTime = @DeletedDateTime,
                                               HiddenDateTime = @HiddenDateTime
                                        WHERE  Id = @Id ";

                        command.Parameters.AddWithValue("@Id", envelope.Id.ToByteArray());
                        command.Parameters.AddWithValue("@Description", envelope.Description);
                        command.Parameters.AddWithValue("@EnvelopeGroupId", envelope.Group?.Id.ToByteArray());
                        command.Parameters.AddWithValue("@Notes", envelope.Notes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@IgnoreOverspend", envelope.IgnoreOverspend);
                        command.Parameters.AddWithValue("@CreatedDateTime", envelope.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", envelope.ModifiedDateTime);
                        command.Parameters.AddWithValue("@DeletedDateTime", envelope.DeletedDateTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@HiddenDateTime", envelope.HiddenDateTime ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                });
            }
            
        }

        public async Task UpdateEnvelopeGroupAsync(EnvelopeGroup envelopeGroup)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"UPDATE EnvelopeGroup
                                        SET    Description = @Description,
                                               Notes = @Notes,
                                               CreatedDateTime = @CreatedDateTime, 
                                               ModifiedDateTime = @ModifiedDateTime, 
                                               DeletedDateTime = @DeletedDateTime,
                                               HiddenDateTime = @HiddenDateTime
                                        WHERE  Id = @Id ";

                        command.Parameters.AddWithValue("@Id", envelopeGroup.Id.ToByteArray());
                        command.Parameters.AddWithValue("@Description", envelopeGroup.Description);
                        command.Parameters.AddWithValue("@Notes", envelopeGroup.Notes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedDateTime", envelopeGroup.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", envelopeGroup.ModifiedDateTime);
                        command.Parameters.AddWithValue("@DeletedDateTime", envelopeGroup.DeletedDateTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@HiddenDateTime", envelopeGroup.HiddenDateTime ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                });
            }
            
        }

        public async Task CreateEnvelopeGroupDtoAsync(EnvelopeGroupDto envelopeGroup)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"INSERT INTO EnvelopeGroup
                                                            (Id, 
                                                             Description, 
                                                             Notes, 
                                                             CreatedDateTime, 
                                                             ModifiedDateTime, 
                                                             DeletedDateTime,
                                                             HiddenDateTime) 
                                                VALUES     (@Id, 
                                                            @Description, 
                                                            @Notes, 
                                                            @CreatedDateTime, 
                                                            @ModifiedDateTime, 
                                                            @DeletedDateTime,
                                                            @HiddenDateTime)";

                        command.AddParameter("@Id", envelopeGroup.Id.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@Description", envelopeGroup.Description, SqliteType.Text);
                        command.AddParameter("@Notes", envelopeGroup.Notes, SqliteType.Text);
                        command.AddParameter("@CreatedDateTime", DateTime.Now, SqliteType.Text);
                        command.AddParameter("@ModifiedDateTime", envelopeGroup.ModifiedDateTime, SqliteType.Text);
                        command.AddParameter("@DeletedDateTime", envelopeGroup.Deleted ? DateTime.Now : null, SqliteType.Text);
                        command.AddParameter("@HiddenDateTime", envelopeGroup.Hidden ? DateTime.Now : null, SqliteType.Text);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task<IReadOnlyList<EnvelopeGroupDto>> ReadEnvelopeGroupDtosAsync(IEnumerable<Guid> envelopeGroupIds)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var envelopeGroups = new List<EnvelopeGroupDto>();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT Id,
                                               Description,
                                               Notes, 
                                               CreatedDateTime, 
                                               ModifiedDateTime, 
                                               DeletedDateTime,
                                               HiddenDateTime
                                        FROM   EnvelopeGroup";

                        if (envelopeGroupIds != null)
                        {
                            var ids = envelopeGroupIds.Select(p => p.ToByteArray()).ToList();
                            if (ids.Any())
                            {
                                var parameters = command.AddParameters("@Id", ids, SqliteType.Blob);
                                command.CommandText += $" WHERE Id IN ({parameters})";
                            }
                            else
                            {
                                return envelopeGroups.AsReadOnly();
                            }
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                envelopeGroups.Add(new EnvelopeGroupDto
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Description = reader["Description"].ToString(),
                                    Notes = reader["Notes"] == DBNull.Value ? (string)null : reader["Notes"].ToString(),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    Deleted = reader["DeletedDateTime"] != DBNull.Value,
                                    Hidden = reader["HiddenDateTime"] != DBNull.Value
                                });
                            }
                        }
                    }

                    return envelopeGroups.AsReadOnly();
                });
            }
        }

        public async Task UpdateEnvelopeGroupDtoAsync(EnvelopeGroupDto envelopeGroup)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"UPDATE EnvelopeGroup
                                                SET    Description = @Description,
                                                       Notes = @Notes,
                                                       ModifiedDateTime = @ModifiedDateTime, 
                                                       DeletedDateTime = @DeletedDateTime,
                                                       HiddenDateTime = @HiddenDateTime
                                                WHERE  Id = @Id ";

                        command.AddParameter("@Id", envelopeGroup.Id.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@Description", envelopeGroup.Description, SqliteType.Text);
                        command.AddParameter("@Notes", envelopeGroup.Notes, SqliteType.Text);
                        command.AddParameter("@ModifiedDateTime", envelopeGroup.ModifiedDateTime, SqliteType.Text);
                        command.AddParameter("@DeletedDateTime", envelopeGroup.Deleted ? DateTime.Now : null, SqliteType.Text);
                        command.AddParameter("@HiddenDateTime", envelopeGroup.Hidden ? DateTime.Now : null, SqliteType.Text);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task CreateEnvelopeDtoAsync(EnvelopeDto envelope)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"INSERT INTO Envelope 
                                                            (Id, 
                                                             Description, 
                                                             EnvelopeGroupId, 
                                                             Notes, 
                                                             IgnoreOverspend,
                                                             CreatedDateTime, 
                                                             ModifiedDateTime, 
                                                             DeletedDateTime,
                                                             HiddenDateTime) 
                                                VALUES     (@Id, 
                                                            @Description, 
                                                            @EnvelopeGroupId,
                                                            @Notes, 
                                                            @IgnoreOverspend,
                                                            @CreatedDateTime, 
                                                            @ModifiedDateTime, 
                                                            @DeletedDateTime,
                                                            @HiddenDateTime)";

                        command.AddParameter("@Id", envelope.Id.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@Description", envelope.Description, SqliteType.Text);
                        command.AddParameter("@Notes", envelope.Notes, SqliteType.Text);
                        command.AddParameter("@EnvelopeGroupId", envelope.EnvelopGroupId.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@IgnoreOverspend", envelope.IgnoreOverspend, SqliteType.Integer);
                        command.AddParameter("@CreatedDateTime", DateTime.Now, SqliteType.Text);
                        command.AddParameter("@ModifiedDateTime", envelope.ModifiedDateTime, SqliteType.Text);
                        command.AddParameter("@DeletedDateTime", envelope.Deleted ? DateTime.Now : null, SqliteType.Text);
                        command.AddParameter("@HiddenDateTime", envelope.Hidden ? DateTime.Now : null, SqliteType.Text);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task<IReadOnlyList<EnvelopeDto>> ReadEnvelopeDtosAsync(IEnumerable<Guid> envelopeIds, IEnumerable<Guid> envelopeGroupIds)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var envelopes = new List<EnvelopeDto>();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT Id, 
                                                       Description, 
                                                       Notes, 
                                                       IgnoreOverspend,
                                                       CreatedDateTime, 
                                                       ModifiedDateTime, 
                                                       DeletedDateTime,
                                                       HiddenDateTime,
                                                       EnvelopeGroupId
                                                FROM   Envelope
                                                WHERE  1=1";

                        if (envelopeIds != null)
                        {
                            var ids = envelopeIds.Select(p => p.ToByteArray()).ToList();
                            if (ids.Any())
                            {
                                var parameters = command.AddParameters("@Id", ids, SqliteType.Blob);
                                command.CommandText += $" AND Id IN ({parameters})";
                            }
                            else
                            {
                                return envelopes.AsReadOnly();
                            }
                        }

                        if (envelopeGroupIds != null)
                        {
                            var ids = envelopeGroupIds.Select(p => p.ToByteArray()).ToList();
                            if (ids.Any())
                            {
                                var parameters = command.AddParameters("@EnvelopeGroupId", ids, SqliteType.Blob);
                                command.CommandText += $" AND EnvelopeGroupId IN ({parameters})";
                            }
                            else
                            {
                                return envelopes.AsReadOnly();
                            }
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                envelopes.Add(new EnvelopeDto
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Description = reader["Description"].ToString(),
                                    Notes = reader["Notes"] == DBNull.Value ? (string)null : reader["Notes"].ToString(),
                                    EnvelopGroupId = new Guid(reader["EnvelopeGroupId"] as byte[]),
                                    IgnoreOverspend = Convert.ToBoolean(reader["IgnoreOverspend"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    Deleted = reader["DeletedDateTime"] != DBNull.Value,
                                    Hidden = reader["HiddenDateTime"] != DBNull.Value
                                });
                            }
                        }
                    }

                    return envelopes.AsReadOnly();
                });
            }
        }

        public async Task UpdateEnvelopeDtoAsync(EnvelopeDto envelope)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"UPDATE Envelope 
                                                SET    Description = @Description,
                                                       EnvelopeGroupId = @EnvelopeGroupId,
                                                       Notes = @Notes,
                                                       IgnoreOverspend = @IgnoreOverspend,
                                                       CreatedDateTime = @CreatedDateTime, 
                                                       ModifiedDateTime = @ModifiedDateTime, 
                                                       DeletedDateTime = @DeletedDateTime,
                                                       HiddenDateTime = @HiddenDateTime
                                                WHERE  Id = @Id ";

                        command.AddParameter("@Id", envelope.Id.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@Description", envelope.Description, SqliteType.Text);
                        command.AddParameter("@Notes", envelope.Notes, SqliteType.Text);
                        command.AddParameter("@EnvelopeGroupId", envelope.EnvelopGroupId.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@IgnoreOverspend", envelope.IgnoreOverspend, SqliteType.Integer);
                        command.AddParameter("@CreatedDateTime", DateTime.Now, SqliteType.Text);
                        command.AddParameter("@ModifiedDateTime", envelope.ModifiedDateTime, SqliteType.Text);
                        command.AddParameter("@DeletedDateTime", envelope.Deleted ? DateTime.Now : null, SqliteType.Text);
                        command.AddParameter("@HiddenDateTime", envelope.Hidden ? DateTime.Now : null, SqliteType.Text);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task CreateBudgetPeriodDtoAsync(BudgetPeriodDto budgetPeriod)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

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

                        command.AddParameter("@Id", budgetPeriod.Id.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@BeginDate", budgetPeriod.BeginDate, SqliteType.Text);
                        command.AddParameter("@EndDate", budgetPeriod.EndDate, SqliteType.Text);
                        command.AddParameter("@CreatedDateTime", DateTime.Now, SqliteType.Text);
                        command.AddParameter("@ModifiedDateTime", budgetPeriod.ModifiedDateTime, SqliteType.Text);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task<IReadOnlyList<BudgetPeriodDto>> ReadBudgetPeriodDtosAsync(IEnumerable<Guid> budgetPeriodIds = null)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var budgetPeriods = new List<BudgetPeriodDto>();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT Id,
                                                       BeginDate, 
                                                       EndDate,
                                                       CreatedDateTime,
                                                       ModifiedDateTime
                                                FROM   BudgetSchedule";

                        if (budgetPeriodIds != null)
                        {
                            var ids = budgetPeriodIds.Select(p => p.ToByteArray()).ToList();
                            if (ids.Any())
                            {
                                var parameters = command.AddParameters("@Id", ids, SqliteType.Blob);
                                command.CommandText += $" WHERE Id IN ({parameters})";
                            }
                            else
                            {
                                return budgetPeriods.AsReadOnly();
                            }
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                budgetPeriods.Add(new BudgetPeriodDto
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    BeginDate = Convert.ToDateTime(reader["BeginDate"], CultureInfo.InvariantCulture),
                                    EndDate = Convert.ToDateTime(reader["EndDate"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture)
                                });
                            }
                        }
                    }

                    return budgetPeriods.AsReadOnly();
                });
            }
        }

        public async Task UpdateBudgetPeriodDtoAsync(BudgetPeriodDto budgetPeriod)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"UPDATE BudgetSchedule
                                                SET    BeginDate = @BeginDate, 
                                                       EndDate = @EndDate,
                                                       ModifiedDateTime = @ModifiedDateTime
                                                WHERE  Id = @Id";

                        command.AddParameter("@Id", budgetPeriod.Id.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@BeginDate", budgetPeriod.BeginDate, SqliteType.Text);
                        command.AddParameter("@EndDate", budgetPeriod.EndDate, SqliteType.Text);
                        command.AddParameter("@ModifiedDateTime", budgetPeriod.ModifiedDateTime, SqliteType.Text);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task CreateBudgetDtoAsync(BudgetDto budget)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

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

                        command.AddParameter("@Id", budget.Id.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@Amount", budget.Amount, SqliteType.Text);
                        command.AddParameter("@IgnoreOverspend", budget.IgnoreOverspend, SqliteType.Integer);
                        command.AddParameter("@BudgetScheduleId", budget.BudgetPeriodId.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@EnvelopeId", budget.EnvelopeId.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@CreatedDateTime", DateTime.Now, SqliteType.Text);
                        command.AddParameter("@ModifiedDateTime", budget.ModifiedDateTime, SqliteType.Text);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task<IReadOnlyList<BudgetDto>> ReadBudgetDtosAsync(IEnumerable<Guid> budgetIds, IEnumerable<Guid> budgetPeriodIds, IEnumerable<Guid> envelopeIds)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var budgets = new List<BudgetDto>();

                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"SELECT Id, 
                                                       Amount, 
                                                       IgnoreOverspend,
                                                       CreatedDateTime, 
                                                       ModifiedDateTime, 
                                                       BudgetScheduleId,
                                                       EnvelopeId
                                                FROM   Budget
                                                WHERE  1=1";

                        if (budgetIds != null)
                        {
                            var ids = budgetIds.Select(p => p.ToByteArray()).ToList();
                            if (ids.Any())
                            {
                                var parameters = command.AddParameters("@Id", ids, SqliteType.Blob);
                                command.CommandText += $" AND Id IN ({parameters})";
                            }
                            else
                            {
                                return budgets.AsReadOnly();
                            }
                        }

                        if (envelopeIds != null)
                        {
                            var ids = envelopeIds.Select(p => p.ToByteArray()).ToList();
                            if (ids.Any())
                            {
                                var parameters = command.AddParameters("@EnvelopeId", ids, SqliteType.Blob);
                                command.CommandText += $" AND EnvelopeId IN ({parameters})";
                            }
                            else
                            {
                                return budgets.AsReadOnly();
                            }
                        }

                        if (budgetPeriodIds != null)
                        {
                            var ids = budgetPeriodIds.Select(p => p.ToByteArray()).ToList();
                            if (ids.Any())
                            {
                                var parameters = command.AddParameters("@BudgetPeriodId", ids, SqliteType.Blob);
                                command.CommandText += $" AND BudgetScheduleId IN ({parameters})";
                            }
                            else
                            {
                                return budgets.AsReadOnly();
                            }
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                budgets.Add(new BudgetDto
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Amount = Convert.ToDecimal(reader["Amount"], CultureInfo.InvariantCulture),
                                    IgnoreOverspend = Convert.ToBoolean(reader["IgnoreOverspend"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    BudgetPeriodId = new Guid(reader["BudgetScheduleId"] as byte[]),
                                    EnvelopeId = new Guid(reader["EnvelopeId"] as byte[])
                                });
                            }
                        }
                    }

                    return budgets.AsReadOnly();
                });
            }
        }

        public async Task UpdateBudgetDtoAsync(BudgetDto budget)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"UPDATE Budget 
                                                SET    Amount = @Amount, 
                                                       IgnoreOverspend = @IgnoreOverspend,
                                                       EnvelopeId = @EnvelopeId, 
                                                       BudgetScheduleId = @BudgetScheduleId,
                                                       ModifiedDateTime = @ModifiedDateTime
                                                WHERE  Id = @Id";

                        command.AddParameter("@Id", budget.Id.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@Amount", budget.Amount, SqliteType.Text);
                        command.AddParameter("@IgnoreOverspend", budget.IgnoreOverspend, SqliteType.Integer);
                        command.AddParameter("@BudgetScheduleId", budget.BudgetPeriodId.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@EnvelopeId", budget.EnvelopeId.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@ModifiedDateTime", budget.ModifiedDateTime, SqliteType.Text);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }
    }
}
