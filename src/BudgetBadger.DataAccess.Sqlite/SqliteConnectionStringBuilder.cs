namespace BudgetBadger.DataAccess.Sqlite
{
    public static class SqliteConnectionStringBuilder
    {
        public static string Get(string path) => "Data Source=" + path;
    }
}
