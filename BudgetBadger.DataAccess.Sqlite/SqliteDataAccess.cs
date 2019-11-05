using System;
using System.IO;
using System.Threading.Tasks;
using BudgetBadger.Core.Utilities;
using BudgetBadger.Models;
using Microsoft.Data.Sqlite;

namespace BudgetBadger.DataAccess.Sqlite
{
    public class SqliteDataAccess
    {
        protected readonly string _connectionString;

        public SqliteDataAccess(string connectionString)
        {
            _connectionString = connectionString;
            Init().FireAndForget();
        }

        public async Task Init()
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        if (File.Exists(db.DataSource))
                        {
                            UpgradeDatabase(db);
                            InsertInitial(db);
                        }
                        else
                        {
                            CreateDatabase(db);
                            InsertInitial(db);
                        }
                    }
                });
            }
        }

        void CreateDatabase(SqliteConnection db)
        {
            db.Open();
            var command = db.CreateCommand();
            command.CommandText = @"BEGIN TRANSACTION;

                                    CREATE TABLE IF NOT EXISTS Payee 
                                          ( 
                                             Id               BLOB PRIMARY KEY NOT NULL, 
                                             Description      TEXT NOT NULL, 
                                             Notes            TEXT, 
                                             CreatedDateTime  TEXT NOT NULL, 
                                             ModifiedDateTime TEXT NOT NULL, 
                                             DeletedDateTime  TEXT,
                                             HiddenDatetime   TEXT
                                          );

                                    CREATE TABLE IF NOT EXISTS Account 
                                          ( 
                                             Id               BLOB PRIMARY KEY NOT NULL, 
                                             Description      TEXT NOT NULL, 
                                             OnBudget         INTEGER NOT NULL, 
                                             Notes            TEXT, 
                                             CreatedDateTime  TEXT NOT NULL, 
                                             ModifiedDateTime TEXT NOT NULL, 
                                             DeletedDateTime  TEXT,
                                             HiddenDateTime   TEXT
                                          );

                                    CREATE TABLE IF NOT EXISTS EnvelopeGroup 
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
                                             HiddenDateTime   TEXT,
                                             FOREIGN KEY(EnvelopeGroupId) REFERENCES EnvelopeGroup(Id)
                                          ); 
            
                                    CREATE TABLE IF NOT EXISTS BudgetSchedule 
                                          ( 
                                             Id          BLOB PRIMARY KEY NOT NULL, 
                                             BeginDate   TEXT NOT NULL,
                                             EndDate     TEXT NOT NULL,
                                             CreatedDateTime  TEXT NOT NULL, 
                                             ModifiedDateTime TEXT NOT NULL
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
                                             FOREIGN KEY(BudgetScheduleId) REFERENCES BudgetSchedule(Id),
                                             FOREIGN KEY(EnvelopeId) REFERENCES Envelope(Id),
                                             UNIQUE(EnvelopeId, BudgetScheduleId)
                                          );

                                    CREATE TABLE IF NOT EXISTS [Transaction]
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
                                            DeletedDateTime    TEXT,
                                            FOREIGN KEY(AccountId) REFERENCES Account(Id),
                                            FOREIGN KEY(EnvelopeId) REFERENCES Envelope(Id),
                                            FOREIGN KEY(PayeeId) REFERENCES Payee(Id)
                                        );

                                    COMMIT;";
            command.ExecuteNonQuery();
            db.Close();
        }

        void InsertInitial(SqliteConnection db)
        {
            db.Open();
            var command = db.CreateCommand();

            command.CommandText = @"
                       INSERT OR IGNORE INTO EnvelopeGroup
                                    (Id, 
                                     Description, 
                                     Notes, 
                                     CreatedDateTime, 
                                     ModifiedDateTime, 
                                     DeletedDateTime) 
                        VALUES     (@DebtEnvelopeGroupId, 
                                    @DebtEnvelopeGroupDescription, 
                                    @DebtEnvelopeGroupNotes, 
                                    @DebtEnvelopeGroupCreatedDateTime, 
                                    @DebtEnvelopeGroupModifiedDateTime, 
                                    @DebtEnvelopeGroupDeletedDateTime),
                                   (@IncomeEnvelopeGroupId, 
                                    @IncomeEnvelopeGroupDescription, 
                                    @IncomeEnvelopeGroupNotes, 
                                    @IncomeEnvelopeGroupCreatedDateTime, 
                                    @IncomeEnvelopeGroupModifiedDateTime, 
                                    @IncomeEnvelopeGroupDeletedDateTime),
                                   (@SystemEnvelopeGroupId, 
                                    @SystemEnvelopeGroupDescription, 
                                    @SystemEnvelopeGroupNotes, 
                                    @SystemEnvelopeGroupCreatedDateTime, 
                                    @SystemEnvelopeGroupModifiedDateTime, 
                                    @SystemEnvelopeGroupDeletedDateTime);

                        INSERT OR IGNORE INTO Envelope 
                                    (Id, 
                                     Description, 
                                     EnvelopeGroupId, 
                                     Notes, 
                                     IgnoreOverspend,
                                     CreatedDateTime, 
                                     ModifiedDateTime, 
                                     DeletedDateTime,
                                     HiddenDateTime) 
                        VALUES     (@BufferEnvelopeId, 
                                    @BufferEnvelopeDescription, 
                                    @BufferEnvelopeEnvelopeGroupId,
                                    @BufferEnvelopeNotes, 
                                    @BufferEnvelopeIgnoreOverspend,
                                    @BufferEnvelopeCreatedDateTime, 
                                    @BufferEnvelopeModifiedDateTime, 
                                    @BufferEnvelopeDeletedDateTime,
                                    @BufferEnvelopeHiddenDateTime),
                                   (@IgnoredEnvelopeId, 
                                    @IgnoredEnvelopeDescription, 
                                    @IgnoredEnvelopeEnvelopeGroupId,
                                    @IgnoredEnvelopeNotes, 
                                    @IgnoredEnvelopeIgnoreOverspend,
                                    @IgnoredEnvelopeCreatedDateTime, 
                                    @IgnoredEnvelopeModifiedDateTime, 
                                    @IgnoredEnvelopeDeletedDateTime,
                                    @IgnoredEnvelopeHiddenDateTime),
                                   (@IncomeEnvelopeId, 
                                    @IncomeEnvelopeDescription, 
                                    @IncomeEnvelopeEnvelopeGroupId,
                                    @IncomeEnvelopeNotes, 
                                    @IncomeEnvelopeIgnoreOverspend,
                                    @IncomeEnvelopeCreatedDateTime, 
                                    @IncomeEnvelopeModifiedDateTime, 
                                    @IncomeEnvelopeDeletedDateTime,
                                    @IncomeEnvelopeHiddenDateTime);

                    INSERT OR IGNORE INTO Payee 
                                (Id, 
                                    Description, 
                                    Notes, 
                                    CreatedDateTime, 
                                    ModifiedDateTime, 
                                    DeletedDateTime,
                                    HiddenDateTime) 
                    VALUES     (@StartingBalancePayeeId, 
                                @StartingBalancePayeeDescription, 
                                @StartingBalancePayeeNotes, 
                                @StartingBalancePayeeCreatedDateTime, 
                                @StartingBalancePayeeModifiedDateTime, 
                                @StartingBalancePayeeDeletedDateTime,
                                @StartingBalancePayeeHiddenDateTime);";

            command.Parameters.AddWithValue("@DebtEnvelopeGroupId", Constants.DebtEnvelopeGroup.Id);
            command.Parameters.AddWithValue("@DebtEnvelopeGroupDescription", nameof(Constants.DebtEnvelopeGroup));
            command.Parameters.AddWithValue("@DebtEnvelopeGroupNotes", Constants.DebtEnvelopeGroup.Notes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DebtEnvelopeGroupCreatedDateTime", Constants.DebtEnvelopeGroup.CreatedDateTime);
            command.Parameters.AddWithValue("@DebtEnvelopeGroupModifiedDateTime", Constants.DebtEnvelopeGroup.ModifiedDateTime);
            command.Parameters.AddWithValue("@DebtEnvelopeGroupDeletedDateTime", Constants.DebtEnvelopeGroup.DeletedDateTime ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IncomeEnvelopeGroupId", Constants.IncomeEnvelopeGroup.Id);
            command.Parameters.AddWithValue("@IncomeEnvelopeGroupDescription", nameof(Constants.IncomeEnvelopeGroup));
            command.Parameters.AddWithValue("@IncomeEnvelopeGroupNotes", Constants.IncomeEnvelopeGroup.Notes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IncomeEnvelopeGroupCreatedDateTime", Constants.IncomeEnvelopeGroup.CreatedDateTime);
            command.Parameters.AddWithValue("@IncomeEnvelopeGroupModifiedDateTime", Constants.IncomeEnvelopeGroup.ModifiedDateTime);
            command.Parameters.AddWithValue("@IncomeEnvelopeGroupDeletedDateTime", Constants.IncomeEnvelopeGroup.DeletedDateTime ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@SystemEnvelopeGroupId", Constants.SystemEnvelopeGroup.Id);
            command.Parameters.AddWithValue("@SystemEnvelopeGroupDescription", nameof(Constants.SystemEnvelopeGroup));
            command.Parameters.AddWithValue("@SystemEnvelopeGroupNotes", Constants.SystemEnvelopeGroup.Notes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@SystemEnvelopeGroupCreatedDateTime", Constants.SystemEnvelopeGroup.CreatedDateTime);
            command.Parameters.AddWithValue("@SystemEnvelopeGroupModifiedDateTime", Constants.SystemEnvelopeGroup.ModifiedDateTime);
            command.Parameters.AddWithValue("@SystemEnvelopeGroupDeletedDateTime", Constants.SystemEnvelopeGroup.DeletedDateTime ?? (object)DBNull.Value);

            command.Parameters.AddWithValue("@BufferEnvelopeId", Constants.BufferEnvelope.Id);
            command.Parameters.AddWithValue("@BufferEnvelopeDescription", nameof(Constants.BufferEnvelope));
            command.Parameters.AddWithValue("@BufferEnvelopeEnvelopeGroupId", Constants.BufferEnvelope.Group?.Id);
            command.Parameters.AddWithValue("@BufferEnvelopeNotes", Constants.BufferEnvelope.Notes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@BufferEnvelopeIgnoreOverspend", Constants.BufferEnvelope.IgnoreOverspend);
            command.Parameters.AddWithValue("@BufferEnvelopeCreatedDateTime", Constants.BufferEnvelope.CreatedDateTime);
            command.Parameters.AddWithValue("@BufferEnvelopeModifiedDateTime", Constants.BufferEnvelope.ModifiedDateTime);
            command.Parameters.AddWithValue("@BufferEnvelopeDeletedDateTime", Constants.BufferEnvelope.DeletedDateTime ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@BufferEnvelopeHiddenDateTime", Constants.BufferEnvelope.HiddenDateTime ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IgnoredEnvelopeId", Constants.IgnoredEnvelope.Id);
            command.Parameters.AddWithValue("@IgnoredEnvelopeDescription", nameof(Constants.IgnoredEnvelope));
            command.Parameters.AddWithValue("@IgnoredEnvelopeEnvelopeGroupId", Constants.IgnoredEnvelope.Group?.Id);
            command.Parameters.AddWithValue("@IgnoredEnvelopeNotes", Constants.IgnoredEnvelope.Notes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IgnoredEnvelopeIgnoreOverspend", Constants.IgnoredEnvelope.IgnoreOverspend);
            command.Parameters.AddWithValue("@IgnoredEnvelopeCreatedDateTime", Constants.IgnoredEnvelope.CreatedDateTime);
            command.Parameters.AddWithValue("@IgnoredEnvelopeModifiedDateTime", Constants.IgnoredEnvelope.ModifiedDateTime);
            command.Parameters.AddWithValue("@IgnoredEnvelopeDeletedDateTime", Constants.IgnoredEnvelope.DeletedDateTime ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IgnoredEnvelopeHiddenDateTime", Constants.IgnoredEnvelope.HiddenDateTime ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IncomeEnvelopeId", Constants.IncomeEnvelope.Id);
            command.Parameters.AddWithValue("@IncomeEnvelopeDescription", nameof(Constants.IncomeEnvelope));
            command.Parameters.AddWithValue("@IncomeEnvelopeEnvelopeGroupId", Constants.IncomeEnvelope.Group?.Id);
            command.Parameters.AddWithValue("@IncomeEnvelopeNotes", Constants.IncomeEnvelope.Notes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IncomeEnvelopeIgnoreOverspend", Constants.IncomeEnvelope.IgnoreOverspend);
            command.Parameters.AddWithValue("@IncomeEnvelopeCreatedDateTime", Constants.IncomeEnvelope.CreatedDateTime);
            command.Parameters.AddWithValue("@IncomeEnvelopeModifiedDateTime", Constants.IncomeEnvelope.ModifiedDateTime);
            command.Parameters.AddWithValue("@IncomeEnvelopeDeletedDateTime", Constants.IncomeEnvelope.DeletedDateTime ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IncomeEnvelopeHiddenDateTime", Constants.IncomeEnvelope.HiddenDateTime ?? (object)DBNull.Value);

            command.Parameters.AddWithValue("@StartingBalancePayeeId", Constants.StartingBalancePayee.Id);
            command.Parameters.AddWithValue("@StartingBalancePayeeDescription", nameof(Constants.StartingBalancePayee));
            command.Parameters.AddWithValue("@StartingBalancePayeeNotes", Constants.StartingBalancePayee.Notes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@StartingBalancePayeeCreatedDateTime", Constants.StartingBalancePayee.CreatedDateTime);
            command.Parameters.AddWithValue("@StartingBalancePayeeModifiedDateTime", Constants.StartingBalancePayee.ModifiedDateTime);
            command.Parameters.AddWithValue("@StartingBalancePayeeDeletedDateTime", Constants.StartingBalancePayee.DeletedDateTime ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@StartingBalancePayeeHiddenDateTime", Constants.StartingBalancePayee.HiddenDateTime ?? (object)DBNull.Value);

            command.ExecuteNonQuery();
            db.Close();
        }

        void UpgradeDatabase(SqliteConnection db)
        {
            db.Open();
            var command = db.CreateCommand();
            command.CommandText = @"PRAGMA user_version;";
            int version = Convert.ToInt32(command.ExecuteScalar());
            db.Close();

            bool needVacuum = false;

            switch (version)
            {
                case 0:
                    UpgradeFromV0ToV1(db);
                    needVacuum = true;
                    goto default;
                default:
                    if (needVacuum)
                    {
                        VacuumDatabase(db);
                    }
                    break;
            }
        }

        void UpgradeFromV0ToV1(SqliteConnection db)
        {
            db.Open();
            var command = db.CreateCommand();
            command.CommandText = @"PRAGMA foreign_keys=off;
                                        BEGIN TRANSACTION;

                                        ALTER TABLE Payee RENAME TO _Payee_old;

                                        CREATE TABLE Payee 
                                          ( 
                                             Id               BLOB PRIMARY KEY NOT NULL, 
                                             Description      TEXT NOT NULL, 
                                             Notes            TEXT, 
                                             CreatedDateTime  TEXT NOT NULL, 
                                             ModifiedDateTime TEXT NOT NULL, 
                                             DeletedDateTime  TEXT,
                                             HiddenDatetime   TEXT
                                          );

                                        INSERT INTO Payee (Id, Description, Notes, CreatedDateTime, ModifiedDateTime, DeletedDateTime, HiddenDateTime)
                                        SELECT Id, Description, Notes, CreatedDateTime, ModifiedDateTime, NULL, DeletedDateTime
                                        FROM _Payee_old;

                                        UPDATE Payee
                                        SET ModifiedDateTime = @Now
                                        WHERE HiddenDateTime IS NOT NULL;

                                        DROP TABLE _Payee_old;


                                        ALTER TABLE Account RENAME TO _Account_old;

                                        CREATE TABLE Account 
                                          ( 
                                             Id               BLOB PRIMARY KEY NOT NULL, 
                                             Description      TEXT NOT NULL, 
                                             OnBudget         INTEGER NOT NULL, 
                                             Notes            TEXT, 
                                             CreatedDateTime  TEXT NOT NULL, 
                                             ModifiedDateTime TEXT NOT NULL, 
                                             DeletedDateTime  TEXT,
                                             HiddenDateTime   TEXT
                                          );

                                        INSERT INTO Account (Id, Description, OnBudget, Notes, CreatedDateTime, ModifiedDateTime, DeletedDateTime, HiddenDateTime)
                                        SELECT Id, Description, OnBudget, Notes, CreatedDateTime, ModifiedDateTime, NULL, DeletedDateTime
                                        FROM _Account_old;

                                        UPDATE Account
                                        SET ModifiedDateTime = @Now
                                        WHERE HiddenDateTime IS NOT NULL;

                                        DROP TABLE _Account_old;


                                        ALTER TABLE Envelope RENAME TO _Envelope_old;

                                        CREATE TABLE Envelope 
                                           ( 
                                             Id               BLOB PRIMARY KEY NOT NULL, 
                                             Description      TEXT NOT NULL, 
                                             EnvelopeGroupId  BLOB NOT NULL, 
                                             Notes            TEXT, 
                                             IgnoreOverspend  INTEGER NOT NULL,
                                             CreatedDateTime  TEXT NOT NULL, 
                                             ModifiedDateTime TEXT NOT NULL, 
                                             DeletedDateTime  TEXT,
                                             HiddenDateTime   TEXT,
                                             FOREIGN KEY(EnvelopeGroupId) REFERENCES EnvelopeGroup(Id)
                                          ); 

                                        INSERT INTO Envelope (Id, Description, EnvelopeGroupId, Notes, IgnoreOverspend, CreatedDateTime, ModifiedDateTime, DeletedDateTime, HiddenDateTime)
                                        SELECT Id, Description, EnvelopeGroupId, Notes, IgnoreOverspend, CreatedDateTime, ModifiedDateTime, NULL, DeletedDateTime
                                        FROM _Envelope_old;

                                        UPDATE Envelope
                                        SET ModifiedDateTime = @Now
                                        WHERE HiddenDateTime IS NOT NULL;

                                        DROP TABLE _Envelope_old;


                                        ALTER TABLE BudgetSchedule RENAME TO _BudgetSchedule_old;

                                        CREATE TABLE BudgetSchedule 
                                          ( 
                                             Id          BLOB PRIMARY KEY NOT NULL, 
                                             BeginDate   TEXT NOT NULL,
                                             EndDate     TEXT NOT NULL,
                                             CreatedDateTime  TEXT NOT NULL, 
                                             ModifiedDateTime TEXT NOT NULL
                                          );

                                        INSERT INTO BudgetSchedule (Id, BeginDate, EndDate, CreatedDateTime, ModifiedDateTime)
                                        SELECT Id, BeginDate, EndDate, CreatedDateTime, ModifiedDateTime
                                        FROM _BudgetSchedule_old;

                                        DROP TABLE _BudgetSchedule_old;


                                        ALTER TABLE Budget RENAME TO _Budget_old;

                                        CREATE TABLE Budget 
                                          ( 
                                             Id               BLOB PRIMARY KEY NOT NULL, 
                                             Amount           TEXT NOT NULL,
                                             IgnoreOverspend  INTEGER NOT NULL, 
                                             BudgetScheduleId BLOB NOT NULL, 
                                             EnvelopeId       BLOB NOT NULL,
                                             CreatedDateTime  TEXT NOT NULL, 
                                             ModifiedDateTime TEXT NOT NULL, 
                                             FOREIGN KEY(BudgetScheduleId) REFERENCES BudgetSchedule(Id),
                                             FOREIGN KEY(EnvelopeId) REFERENCES Envelope(Id),
                                             UNIQUE(EnvelopeId, BudgetScheduleId)
                                          );

                                        INSERT INTO Budget (Id, Amount, IgnoreOverspend, BudgetScheduleId, EnvelopeId, CreatedDateTime, ModifiedDateTime)
                                        SELECT Id, Amount, IgnoreOverspend, BudgetScheduleId, EnvelopeId, CreatedDateTime, ModifiedDateTime
                                        FROM _Budget_old
                                        GROUP BY EnvelopeId, BudgetScheduleId;

                                        DROP TABLE _Budget_old;


                                        ALTER TABLE [Transaction] RENAME TO _Transaction_old;

                                        CREATE TABLE [Transaction]
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
                                            DeletedDateTime    TEXT,
                                            FOREIGN KEY(AccountId) REFERENCES Account(Id),
                                            FOREIGN KEY(EnvelopeId) REFERENCES Envelope(Id),
                                            FOREIGN KEY(PayeeId) REFERENCES Payee(Id)
                                        );

                                        INSERT INTO [Transaction] (Id, Amount, Posted, ReconciledDateTime, AccountId, PayeeId, EnvelopeId, SplitId, ServiceDate, Notes, CreatedDateTime, ModifiedDateTime, DeletedDateTime)
                                        SELECT t.Id, t.Amount, t.Posted, t.ReconciledDateTime, t.AccountId, t.PayeeId, t.EnvelopeId, t.SplitId, t.ServiceDate, t.Notes, t.CreatedDateTime, t.ModifiedDateTime, t.DeletedDateTime
                                        FROM _Transaction_old t
                                        LEFT JOIN Account a ON t.AccountId = a.Id
                                        LEFT JOIN Payee p ON t.PayeeId = p.Id
                                        LEFT JOIN Envelope e ON t.EnvelopeId = e.Id
                                        WHERE a.Id IS NOT NULL AND p.Id IS NOT NULL AND e.Id IS NOT NULL;

                                        DROP TABLE _Transaction_old;
                
                                        PRAGMA user_version=1;
                                        COMMIT;

                                        PRAGMA foreign_keys=on;";

            command.Parameters.AddWithValue("@Now", DateTime.Now);

            command.ExecuteNonQuery();
            db.Close();
        }

        void VacuumDatabase(SqliteConnection db)
        {
            db.Open();
            var command = db.CreateCommand();
            command.CommandText = @"VACUUM;";
            command.ExecuteNonQuery();
            db.Close();
        }
    }
}
