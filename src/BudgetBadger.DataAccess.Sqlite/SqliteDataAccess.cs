using System;
using System.IO;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Utilities;
using BudgetBadger.Models;
using Microsoft.Data.Sqlite;

namespace BudgetBadger.DataAccess.Sqlite
{
    public partial class SqliteDataAccess : IDataAccess
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
                        if (DatabaseExists(db))
                        {
                            UpgradeDatabase(db);
                        }
                        else
                        {
                            CreateDatabase(db);
                        }
                    }
                });
            }
        }

        bool DatabaseExists(SqliteConnection db)
        {
            var exists = false;

            if (File.Exists(db.DataSource))
            {
                db.Open();
                var command = db.CreateCommand();
                command.CommandText = @"SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name='Account';";
                exists = Convert.ToBoolean(command.ExecuteScalar());
                db.Close();
            }

            return exists;
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
                                             HiddenDateTime   TEXT
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
                                             DeletedDateTime  TEXT,
                                             HiddenDateTime   TEXT
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

                                    INSERT OR IGNORE INTO EnvelopeGroup
                                                (Id, 
                                                 Description, 
                                                 CreatedDateTime, 
                                                 ModifiedDateTime) 
                                    VALUES     (@DebtEnvelopeGroupId, 
                                                @DebtEnvelopeGroupDescription, 
                                                @Now, 
                                                @Now),
                                               (@IncomeEnvelopeGroupId, 
                                                @IncomeEnvelopeGroupDescription, 
                                                @Now, 
                                                @Now),
                                               (@SystemEnvelopeGroupId, 
                                                @SystemEnvelopeGroupDescription, 
                                                @Now, 
                                                @Now);

                                    INSERT OR IGNORE INTO Envelope 
                                                (Id, 
                                                 Description, 
                                                 EnvelopeGroupId, 
                                                 IgnoreOverspend,
                                                 CreatedDateTime, 
                                                 ModifiedDateTime) 
                                    VALUES     (@BufferEnvelopeId, 
                                                @BufferEnvelopeDescription, 
                                                @BufferEnvelopeEnvelopeGroupId,
                                                @BufferEnvelopeIgnoreOverspend,
                                                @Now, 
                                                @Now),
                                               (@IgnoredEnvelopeId, 
                                                @IgnoredEnvelopeDescription, 
                                                @IgnoredEnvelopeEnvelopeGroupId,
                                                @IgnoredEnvelopeIgnoreOverspend,
                                                @Now, 
                                                @Now),
                                               (@IncomeEnvelopeId, 
                                                @IncomeEnvelopeDescription, 
                                                @IncomeEnvelopeEnvelopeGroupId,
                                                @IncomeEnvelopeIgnoreOverspend,
                                                @Now, 
                                                @Now);

                                INSERT OR IGNORE INTO Payee 
                                            (Id, 
                                                Description, 
                                                CreatedDateTime, 
                                                ModifiedDateTime) 
                                VALUES     (@StartingBalancePayeeId, 
                                            @StartingBalancePayeeDescription, 
                                            @Now, 
                                            @Now);

                                CREATE TRIGGER Payee_U BEFORE UPDATE ON Payee FOR EACH ROW
                                WHEN NEW.DeletedDateTime IS NOT NULL AND NEW.HiddenDateTime IS NULL
                                BEGIN
	                                SELECT RAISE(ABORT, ‘Error’);
                                END;

                                CREATE TRIGGER Payee_I BEFORE INSERT ON Payee FOR EACH ROW
                                WHEN NEW.DeletedDateTime IS NOT NULL AND NEW.HiddenDateTime IS NULL
                                BEGIN
	                                SELECT RAISE(ABORT, ‘Error’);
                                END;

                                CREATE TRIGGER Account_U BEFORE UPDATE ON Account FOR EACH ROW
                                WHEN NEW.DeletedDateTime IS NOT NULL AND NEW.HiddenDateTime IS NULL
                                BEGIN
	                                SELECT RAISE(ABORT, ‘Error’);
                                END;

                                CREATE TRIGGER Account_I BEFORE INSERT ON Account FOR EACH ROW
                                WHEN NEW.DeletedDateTime IS NOT NULL AND NEW.HiddenDateTime IS NULL
                                BEGIN
	                                SELECT RAISE(ABORT, ‘Error’);
                                END;

                                CREATE TRIGGER Envelope_U BEFORE UPDATE ON Envelope FOR EACH ROW
                                WHEN NEW.DeletedDateTime IS NOT NULL AND NEW.HiddenDateTime IS NULL
                                BEGIN
	                                SELECT RAISE(ABORT, ‘Error’);
                                END;

                                CREATE TRIGGER Envelope_I BEFORE INSERT ON Envelope FOR EACH ROW
                                WHEN NEW.DeletedDateTime IS NOT NULL AND NEW.HiddenDateTime IS NULL
                                BEGIN
	                                SELECT RAISE(ABORT, ‘Error’);
                                END;

                                CREATE TRIGGER EnvelopeGroup_U BEFORE UPDATE ON EnvelopeGroup FOR EACH ROW
                                WHEN NEW.DeletedDateTime IS NOT NULL AND NEW.HiddenDateTime IS NULL
                                BEGIN
	                                SELECT RAISE(ABORT, ‘Error’);
                                END;

                                CREATE TRIGGER EnvelopeGroup_I BEFORE INSERT ON EnvelopeGroup FOR EACH ROW
                                WHEN NEW.DeletedDateTime IS NOT NULL AND NEW.HiddenDateTime IS NULL
                                BEGIN
	                                SELECT RAISE(ABORT, ‘Error’);
                                END;

                                PRAGMA user_version=1;

                                COMMIT;";

            command.Parameters.AddWithValue("@Now", DateTime.Now);

            command.Parameters.AddWithValue("@DebtEnvelopeGroupId", Constants.DebtEnvelopeGroup.Id.ToByteArray());
            command.Parameters.AddWithValue("@DebtEnvelopeGroupDescription", nameof(Constants.DebtEnvelopeGroup));

            command.Parameters.AddWithValue("@IncomeEnvelopeGroupId", Constants.IncomeEnvelopeGroup.Id.ToByteArray());
            command.Parameters.AddWithValue("@IncomeEnvelopeGroupDescription", nameof(Constants.IncomeEnvelopeGroup));

            command.Parameters.AddWithValue("@SystemEnvelopeGroupId", Constants.SystemEnvelopeGroup.Id.ToByteArray());
            command.Parameters.AddWithValue("@SystemEnvelopeGroupDescription", nameof(Constants.SystemEnvelopeGroup));

            command.Parameters.AddWithValue("@BufferEnvelopeId", Constants.BufferEnvelope.Id.ToByteArray());
            command.Parameters.AddWithValue("@BufferEnvelopeDescription", nameof(Constants.BufferEnvelope));
            command.Parameters.AddWithValue("@BufferEnvelopeEnvelopeGroupId", Constants.BufferEnvelope.Group?.Id.ToByteArray());
            command.Parameters.AddWithValue("@BufferEnvelopeIgnoreOverspend", Constants.BufferEnvelope.IgnoreOverspend);

            command.Parameters.AddWithValue("@IgnoredEnvelopeId", Constants.IgnoredEnvelope.Id.ToByteArray());
            command.Parameters.AddWithValue("@IgnoredEnvelopeDescription", nameof(Constants.IgnoredEnvelope));
            command.Parameters.AddWithValue("@IgnoredEnvelopeEnvelopeGroupId", Constants.IgnoredEnvelope.Group?.Id.ToByteArray());
            command.Parameters.AddWithValue("@IgnoredEnvelopeIgnoreOverspend", Constants.IgnoredEnvelope.IgnoreOverspend);

            command.Parameters.AddWithValue("@IncomeEnvelopeId", Constants.IncomeEnvelope.Id.ToByteArray());
            command.Parameters.AddWithValue("@IncomeEnvelopeDescription", nameof(Constants.IncomeEnvelope));
            command.Parameters.AddWithValue("@IncomeEnvelopeEnvelopeGroupId", Constants.IncomeEnvelope.Group?.Id.ToByteArray());
            command.Parameters.AddWithValue("@IncomeEnvelopeIgnoreOverspend", Constants.IncomeEnvelope.IgnoreOverspend);

            command.Parameters.AddWithValue("@StartingBalancePayeeId", Constants.StartingBalancePayee.Id.ToByteArray());
            command.Parameters.AddWithValue("@StartingBalancePayeeDescription", nameof(Constants.StartingBalancePayee));

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
                                             HiddenDateTime   TEXT
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

                                        ALTER TABLE EnvelopeGroup RENAME TO _EnvelopeGroup_old;

                                        CREATE TABLE EnvelopeGroup 
                                           ( 
                                             Id               BLOB PRIMARY KEY NOT NULL, 
                                             Description      TEXT NOT NULL, 
                                             Notes            TEXT, 
                                             CreatedDateTime  TEXT NOT NULL, 
                                             ModifiedDateTime TEXT NOT NULL, 
                                             DeletedDateTime  TEXT,
                                             HiddenDateTime   TEXT
                                          ); 

                                        INSERT INTO EnvelopeGroup (Id, Description, Notes, CreatedDateTime, ModifiedDateTime, DeletedDateTime, HiddenDateTime)
                                        SELECT Id, Description, Notes, CreatedDateTime, ModifiedDateTime, NULL, DeletedDateTime
                                        FROM _EnvelopeGroup_old;

                                        UPDATE EnvelopeGroup
                                        SET ModifiedDateTime = @Now
                                        WHERE HiddenDateTime IS NOT NULL;

                                        DROP TABLE _EnvelopeGroup_old;


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

                                        CREATE TRIGGER Payee_U BEFORE UPDATE ON Payee FOR EACH ROW
                                        WHEN NEW.DeletedDateTime IS NOT NULL AND NEW.HiddenDateTime IS NULL
                                        BEGIN
	                                        SELECT RAISE(ABORT, ‘Error’);
                                        END;

                                        CREATE TRIGGER Payee_I BEFORE INSERT ON Payee FOR EACH ROW
                                        WHEN NEW.DeletedDateTime IS NOT NULL AND NEW.HiddenDateTime IS NULL
                                        BEGIN
	                                        SELECT RAISE(ABORT, ‘Error’);
                                        END;

                                        CREATE TRIGGER Account_U BEFORE UPDATE ON Account FOR EACH ROW
                                        WHEN NEW.DeletedDateTime IS NOT NULL AND NEW.HiddenDateTime IS NULL
                                        BEGIN
	                                        SELECT RAISE(ABORT, ‘Error’);
                                        END;

                                        CREATE TRIGGER Account_I BEFORE INSERT ON Account FOR EACH ROW
                                        WHEN NEW.DeletedDateTime IS NOT NULL AND NEW.HiddenDateTime IS NULL
                                        BEGIN
	                                        SELECT RAISE(ABORT, ‘Error’);
                                        END;

                                        CREATE TRIGGER Envelope_U BEFORE UPDATE ON Envelope FOR EACH ROW
                                        WHEN NEW.DeletedDateTime IS NOT NULL AND NEW.HiddenDateTime IS NULL
                                        BEGIN
	                                        SELECT RAISE(ABORT, ‘Error’);
                                        END;

                                        CREATE TRIGGER Envelope_I BEFORE INSERT ON Envelope FOR EACH ROW
                                        WHEN NEW.DeletedDateTime IS NOT NULL AND NEW.HiddenDateTime IS NULL
                                        BEGIN
	                                        SELECT RAISE(ABORT, ‘Error’);
                                        END;

                                        CREATE TRIGGER EnvelopeGroup_U BEFORE UPDATE ON EnvelopeGroup FOR EACH ROW
                                        WHEN NEW.DeletedDateTime IS NOT NULL AND NEW.HiddenDateTime IS NULL
                                        BEGIN
	                                        SELECT RAISE(ABORT, ‘Error’);
                                        END;

                                        CREATE TRIGGER EnvelopeGroup_I BEFORE INSERT ON EnvelopeGroup FOR EACH ROW
                                        WHEN NEW.DeletedDateTime IS NOT NULL AND NEW.HiddenDateTime IS NULL
                                        BEGIN
	                                        SELECT RAISE(ABORT, ‘Error’);
                                        END;
                
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
