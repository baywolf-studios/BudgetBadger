using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BudgetBadger.Core.Utilities;
using BudgetBadger.Core.Models;
using Microsoft.Data.Sqlite;
using BudgetBadger.DataAccess.Dtos;
using System.Linq;
using System.Text;
using BudgetBadger.DataAccess;
using System.Linq.Expressions;

namespace BudgetBadger.DataAccess.Sqlite
{
    public partial class SqliteDataAccess
    {
        public async Task CreatePayeeAsync(PayeeModel payee)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"INSERT INTO Payee 
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

                        command.Parameters.AddWithValue("@Id", payee.Id.ToByteArray());
                        command.Parameters.AddWithValue("@Description", payee.Description);
                        command.Parameters.AddWithValue("@Notes", payee.Notes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedDateTime", payee.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", payee.ModifiedDateTime);
                        command.Parameters.AddWithValue("@DeletedDateTime", payee.DeletedDateTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@HiddenDateTime", payee.HiddenDateTime ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task<PayeeModel> ReadPayeeAsync(Guid id)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var payee = new PayeeModel();

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
                                        FROM   Payee 
                                        WHERE  Id = @Id";

                        command.Parameters.AddWithValue("@Id", id.ToByteArray());

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                payee = new PayeeModel
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

                    return payee;
                });
            }
        }

        public async Task<IReadOnlyList<PayeeModel>> ReadPayeesAsync()
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var payees = new List<PayeeModel>();

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
                                        FROM   Payee";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var payee = new PayeeModel
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Description = reader["Description"].ToString(),
                                    Notes = reader["Notes"].ToString(),
                                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"], CultureInfo.InvariantCulture),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    DeletedDateTime = reader["DeletedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedDateTime"], CultureInfo.InvariantCulture),
                                    HiddenDateTime = reader["HiddenDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["HiddenDateTime"], CultureInfo.InvariantCulture)
                                };
                                payees.Add(payee);
                            }
                        }
                    }

                    return payees;
                });
            }
        }

        public async Task UpdatePayeeAsync(PayeeModel payee)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"UPDATE Payee 
                                        SET    Description = @Description,
                                               Notes = @Notes, 
                                               CreatedDateTime = @CreatedDateTime, 
                                               ModifiedDateTime = @ModifiedDateTime, 
                                               DeletedDateTime = @DeletedDateTime,
                                               HiddenDateTime = @HiddenDateTime
                                        WHERE  Id = @Id ";

                        command.Parameters.AddWithValue("@Id", payee.Id.ToByteArray());
                        command.Parameters.AddWithValue("@Description", payee.Description);
                        command.Parameters.AddWithValue("@Notes", payee.Notes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedDateTime", payee.CreatedDateTime);
                        command.Parameters.AddWithValue("@ModifiedDateTime", payee.ModifiedDateTime);
                        command.Parameters.AddWithValue("@DeletedDateTime", payee.DeletedDateTime ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@HiddenDateTime", payee.HiddenDateTime ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task CreatePayeeDtoAsync(PayeeDto payee)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"INSERT INTO Payee 
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
                        command.AddParameter("@Id", payee.Id.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@Description", payee.Description, SqliteType.Text);
                        command.AddParameter("@Notes", payee.Notes, SqliteType.Text);
                        command.AddParameter("@CreatedDateTime", DateTime.Now, SqliteType.Text);
                        command.AddParameter("@ModifiedDateTime", payee.ModifiedDateTime, SqliteType.Text);
                        command.AddParameter("@DeletedDateTime", payee.Deleted ? DateTime.Now : null, SqliteType.Text);
                        command.AddParameter("@HiddenDateTime", payee.Hidden ? DateTime.Now : null, SqliteType.Text);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public async Task<IReadOnlyList<PayeeDto>> ReadPayeeDtosAsync(IEnumerable<Guid> payeeIds)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                return await Task.Run(() =>
                {
                    var payees = new List<PayeeDto>();

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
                                         FROM   Payee";

                        if (payeeIds != null)
                        {
                            var ids = payeeIds.Select(p => p.ToByteArray()).ToList();
                            if (ids.Any())
                            {
                                var parameters = command.AddParameters("@Id", ids, SqliteType.Blob);
                                command.CommandText += $" WHERE Id IN ({parameters})";
                            }
                            else
                            {
                                return payees.AsReadOnly();
                            }
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var payee = new PayeeDto
                                {
                                    Id = new Guid(reader["Id"] as byte[]),
                                    Description = reader["Description"].ToString(),
                                    Notes = reader["Notes"] == DBNull.Value ? (string)null : reader["Notes"].ToString(),
                                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"], CultureInfo.InvariantCulture),
                                    Deleted = reader["DeletedDateTime"] != DBNull.Value,
                                    Hidden = reader["HiddenDateTime"] != DBNull.Value
                                };
                                payees.Add(payee);
                            }
                        }
                    }

                    return payees.AsReadOnly();
                });
            }
        }

        public async Task UpdatePayeeDtoAsync(PayeeDto payee)
        {
            using (await MultiThreadLock.UseWaitAsync())
            {
                await Task.Run(() =>
                {
                    using (var db = new SqliteConnection(_connectionString))
                    {
                        db.Open();
                        var command = db.CreateCommand();

                        command.CommandText = @"UPDATE Payee 
                                        SET    Description = @Description,
                                               Notes = @Notes,
                                               ModifiedDateTime = @ModifiedDateTime, 
                                               DeletedDateTime = @DeletedDateTime,
                                               HiddenDateTime = @HiddenDateTime
                                        WHERE  Id = @Id ";

                        command.AddParameter("@Id", payee.Id.ToByteArray(), SqliteType.Blob);
                        command.AddParameter("@Description", payee.Description, SqliteType.Text);
                        command.AddParameter("@Notes", payee.Notes, SqliteType.Text);
                        command.AddParameter("@ModifiedDateTime", payee.ModifiedDateTime, SqliteType.Text);
                        command.AddParameter("@DeletedDateTime", payee.Deleted ? DateTime.Now : null, SqliteType.Text);
                        command.AddParameter("@HiddenDateTime", payee.Hidden ? DateTime.Now : null, SqliteType.Text);

                        command.ExecuteNonQuery();
                    }
                });
            }
        }
    }
}
