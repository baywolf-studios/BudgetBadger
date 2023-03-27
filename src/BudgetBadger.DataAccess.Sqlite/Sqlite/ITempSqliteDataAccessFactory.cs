namespace BudgetBadger.DataAccess.Sqlite
{
    public interface ITempSqliteDataAccessFactory
    {
        (string path, SqliteDataAccess sqliteDataAccess) Create();
    }
}
