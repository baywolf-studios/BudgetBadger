using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace BudgetBadger.DataAccess.Sqlite
{
    public static class SqliteDataAccess
    {
        public async static Task Init(string connectionString)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(connectionString))
                    {
                        if (File.Exists(db.DataSource))
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

        static void CreateDatabase(SqliteConnection db)
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
                                             HiddenDateTime   TEXT,
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
                                             HiddenDateTime   TEXT,
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
                                            HiddenDateTime     TEXT,
                                            FOREIGN KEY(AccountId) REFERENCES Account(Id),
                                            FOREIGN KEY(EnvelopeId) REFERENCES Envelope(Id),
                                            FOREIGN KEY(PayeeId) REFERENCES Payee(Id)
                                        );

                                    COMMIT;";
            command.ExecuteNonQuery();
            db.Close();
        }

        static void UpgradeDatabase(SqliteConnection db)
        {
            db.Open();
            var command = db.CreateCommand();
            command.CommandText = @"PRAGMA user_version;";
            int version = Convert.ToInt32(command.ExecuteScalar());
            db.Close();

            switch (version)
            {
                case 0:

                    goto case 1;
                case 1:
                    Console.WriteLine("Case 1");
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }
        }

        static void UpgradeFromV0ToV1(SqliteConnection db)
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
                                             HiddenDateTime   TEXT,
                                          );

                                        INSERT INTO Payee (Id, Description, OnBudget, Notes, CreatedDateTime, ModifiedDateTime, DeletedDateTime, HiddenDateTime)
                                        SELECT Id, Description, OnBudget, Notes, CreatedDateTime, ModifiedDateTime, NULL, DeletedDateTime
                                        FROM _Account_old;

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

                                        DROP TABLE _Envelope_old;


                
                                        PRAGMA user_version=1;
                                        COMMIT;

                                        PRAGMA foreign_keys=on;";

            command.ExecuteNonQuery();
            db.Close();
        }

        static void VacuumDatabase(SqliteConnection db)
        {

        }
    }
}
