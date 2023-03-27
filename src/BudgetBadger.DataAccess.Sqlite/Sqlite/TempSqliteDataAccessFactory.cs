using System.IO;

namespace BudgetBadger.DataAccess.Sqlite
{
    public class TempSqliteDataAccessFactory : ITempSqliteDataAccessFactory
    {
        public (string path, SqliteDataAccess sqliteDataAccess) Create()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var tempConnectionString = SqliteConnectionStringBuilder.Get(tempFile);
            var tempDataAccess = new SqliteDataAccess(tempConnectionString);
            return (tempFile, tempDataAccess);
        }
    }
}
